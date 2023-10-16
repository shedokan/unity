using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    public GameObject pivotAround;
    public GameObject projectile;
    public GameObject projectileParent;
    public float shootThrust = 5000; // Should be positive

    private Camera _mainCamera;
    private RectTransform _rectTransform;
    private RectTransform _pivotRectTransform;
    private Vector2 _offset;
    private Ray2D _aimRay;

    // Start is called before the first frame update
    void Start() {
        _mainCamera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();

        if(pivotAround) {
            _pivotRectTransform = pivotAround.GetComponent<RectTransform>();
            _offset = _rectTransform.position - _pivotRectTransform.position;
        }
        else {
            pivotAround = gameObject;
            _pivotRectTransform = _rectTransform;
        }

        var rb2D = projectile.GetComponent<Rigidbody2D>();
        Assert.IsNotNull(rb2D);
    }

    void OnGUI() {
        var mouse = Mouse.current.position.value;
        Vector3 mouse3d = mouse;
        mouse3d.z = 100;

        var mouseWorld = _mainCamera.ScreenToWorldPoint(mouse3d);

        var position = _rectTransform.position;
        var pivotPosition = _pivotRectTransform.position;

        if(!BallGrid.current) return;
        var gridRectTransform = BallGrid.current.GetComponent<RectTransform>();
        var coord = BallGrid.current.PosToOffset(mouseWorld);
        var roundedGrid = BallGrid.current.RoundToNearestGrid(mouseWorld);
        GUI.Box(new Rect(5, 25, 400, 100), "");
        //The Labels show what the Sliders represent
        GUI.Label(new Rect(10, 30, 400, 200), $@"Mouse(World) X: {mouseWorld.x}, Y: {mouseWorld.y}, Z: {mouseWorld.z}
This X: {position.x}, Y : {position.y}, Z: {position.z}
ballGrid X: {gridRectTransform.worldToLocalMatrix * mouseWorld}
Pivot X: {pivotPosition.x}, Y : {pivotPosition.y}, Z: {pivotPosition.z}
Hex Col: {coord.col}, Row: {coord.row}
Rounded X: {roundedGrid.x}, Col: {roundedGrid.y}
Distance: {Vector2.Distance(position, mouseWorld)}
");
    }

    // Update is called once per frame
    private void FixedUpdate() {
        // TODO: Support touch
        var mouse = Mouse.current;
        if(mouse is null || !mouse.wasUpdatedThisFrame) return;

        var pivotAroundPos = _pivotRectTransform.position;
        Vector3 mouse3d = mouse.position.value;
        mouse3d.z = pivotAroundPos.z;
        var mouseWorld = _mainCamera.ScreenToWorldPoint(mouse3d);

        _aimRay = LookAt(mouseWorld, pivotAroundPos);
    }

    private void OnFire(InputValue value) {
        var hit = Physics2D.Raycast(_aimRay.origin, _aimRay.direction);
        if(!hit) return;
        Debug.DrawLine(_aimRay.origin, hit.point, Color.red);

        ShootBall(_aimRay);
    }

    private void ShootBall(Ray2D ray) {
        if(!projectile) return;

        var ball = Instantiate(projectile, _rectTransform.position, _rectTransform.rotation,
            projectileParent.transform);
        ball.SendMessage("StartMoving", BallController.RandomColor());

        var rb2D = ball.GetComponent<Rigidbody2D>();
        rb2D.AddForce(ray.direction * shootThrust);
    }

    private Ray2D LookAt(Vector2 target, Vector2 pivotAroundPos) {
        var moveDirection = target - pivotAroundPos;
        // TODO: Maybe replace with Vector2.Angle()
        var angle = Angle(moveDirection);

        angle = (angle >= 90.0f) ? angle - 90.0f : 270.0f + angle;
        var angleAxis = Quaternion.AngleAxis(angle, Vector3.forward);
        _rectTransform.rotation = angleAxis;

        var lookingRay = new Ray2D(pivotAroundPos, angleAxis * Vector2.up);


        if(pivotAround == gameObject) return lookingRay;

        var point = lookingRay.GetPoint(_offset.magnitude);
        _rectTransform.position = new Vector3(point.x, point.y, _rectTransform.position.z);

        return lookingRay;
    }

    // Return the angle in degrees of vector v relative to the x-axis. 
    // Angles are towards the positive y-axis (typically counter-clockwise) and between 0 and 360.
    private static float Angle(Vector2 v) {
        var angle = (float)Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if(angle < 0) angle += 360;
        return angle;
    }
}