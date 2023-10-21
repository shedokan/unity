using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

/// <summary>Add to aim center</summary>
public class AimController : MonoBehaviour {
    /// <summary>Object used to show direction</summary>
    public GameObject pointer;

    /// <summary>Prefab used to shoot</summary>
    public GameObject projectilePrefab;

    /// <summary>Where to add the projectile upon shooting</summary>
    public GameObject projectileParent;

    public float shootThrust = 5000; // Should be positive

    private Ray2D _aimRay;
    private BallController _currProjectile;
    private Camera _mainCamera;
    private Vector2 _pointerOffset;
    private RectTransform _pointerRectTransform;
    private RectTransform _rectTransform;

    // Start is called before the first frame update
    private void Start() {
        _mainCamera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();

        if(pointer) {
            _pointerRectTransform = pointer.GetComponent<RectTransform>();
            _pointerOffset = _pointerRectTransform.position - _rectTransform.position;
        }

        var rb2D = projectilePrefab.GetComponent<Rigidbody2D>();
        Assert.IsNotNull(rb2D);

        _currProjectile = NewProjectile();
    }

    // Update is called once per frame
    private void FixedUpdate() {
        // TODO: Support touch
        var mouse = Mouse.current;
        if(mouse is null || !mouse.wasUpdatedThisFrame) return;

        var pivotPos = _rectTransform.position;
        Vector3 mouse3d = mouse.position.value;
        mouse3d.z = pivotPos.z;
        var mouseWorld = _mainCamera.ScreenToWorldPoint(mouse3d);

        _aimRay = PointAt(pivotPos, mouseWorld);
    }

    private void OnGUI() {
        var mouse = Mouse.current.position.value;
        Vector3 mouse3d = mouse;
        mouse3d.z = 100;

        var mouseWorld = _mainCamera.ScreenToWorldPoint(mouse3d);

        var position = _pointerRectTransform.position;
        var pivotPosition = _rectTransform.position;

        if(!BallGrid.current) return;
        var mouseHex = BallGrid.current.WorldPosToHex(mouseWorld);
        var roundedGrid = BallGrid.current.RoundToNearestGrid(mouseWorld);
        var gridRectTransform = BallGrid.current.GetComponent<RectTransform>();
        GUI.Box(new Rect(5, 25, 400, 100), "");
        //The Labels show what the Sliders represent
        GUI.Label(new Rect(10, 30, 400, 200), $@"Mouse(World) X: {mouseWorld.x}, Y: {mouseWorld.y}, Z: {mouseWorld.z}
Mouse(Hex): Q: {mouseHex.q}, R: {mouseHex.r}
Mouse(Rounded) X: {roundedGrid.x}, Col: {roundedGrid.y}
This X: {position.x}, Y : {position.y}, Z: {position.z}
ballGrid X: {gridRectTransform.worldToLocalMatrix * mouseWorld}
Pivot X: {pivotPosition.x}, Y : {pivotPosition.y}, Z: {pivotPosition.z}
Distance: {Vector2.Distance(position, mouseWorld)}
");
    }

#if UNITY_EDITOR
    private void OnRectTransformDimensionsChange() {
        if(_currProjectile) {
            // TODO: Reposition projectile
        }
    }
#endif

    private void OnFire(InputValue value) {
        var hit = Physics2D.Raycast(_aimRay.origin, _aimRay.direction);
        if(!hit) return;
        Debug.DrawLine(_aimRay.origin, hit.point, Color.red, 2);

        ShootProjectile(_aimRay);

        _currProjectile = NewProjectile();
    }

    /// <summary>
    ///     Points the <see cref="pointer" /> towards <paramref name="targetWorldPos" />
    ///     and returns a 2D ray towards it
    /// </summary>
    /// <param name="myWorldPos">Pivot to point at (world coordinates)</param>
    /// <param name="targetWorldPos">Target to point at (world coordinates)</param>
    /// <returns>A ray from <paramref name="myWorldPos" /> to <paramref name="targetWorldPos" /></returns>
    private Ray2D PointAt(Vector2 myWorldPos, Vector2 targetWorldPos) {
        var moveDirection = targetWorldPos - myWorldPos;
        var angle = Vector2.SignedAngle(Vector2.up, moveDirection);
        var angleAxis = Quaternion.AngleAxis(angle, Vector3.forward);
        var lookingRay = new Ray2D(myWorldPos, angleAxis * Vector2.up);

        if(pointer) {
            var point = lookingRay.GetPoint(_pointerOffset.magnitude);
            _pointerRectTransform.SetPositionAndRotation(
                new Vector3(point.x, point.y, _pointerRectTransform.position.z),
                angleAxis);
        }

        return lookingRay;
    }

    private BallController NewProjectile() {
        var projectile = Instantiate(projectilePrefab, _rectTransform.position, _rectTransform.rotation,
            projectileParent.transform);
        projectile.layer = Layers.Aimer;

        var ballController = projectile.GetComponent<BallController>();
        ballController.color = BallController.RandomColor();

        return ballController;
    }

    /// <summary>
    /// </summary>
    /// <param name="ray"></param>
    private void ShootProjectile(Ray2D ray) {
        if(!_currProjectile) return;

        _currProjectile.Shoot(ray.direction * shootThrust);
    }
}