using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    public GameObject pivotAround;

    private Camera _mainCamera;
    private RectTransform _rectTransform;
    private RectTransform _pivotRectTransform;
    private Vector2 _offset;

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
    }

    void OnGUI() {
        var mouse = Mouse.current.position.value;
        Vector3 mouse3d = mouse;
        mouse3d.z = 100;

        var mouseWorld = _mainCamera.ScreenToWorldPoint(mouse3d);

        var position = _rectTransform.position;
        var pivotPosition = _pivotRectTransform.position;

        GUI.Box(new Rect(5, 25, 400, 100), "");
        //The Labels show what the Sliders represent
        GUI.Label(new Rect(10, 30, 400, 150), $@"Mouse X: {mouse.x}, Y: {mouse.y}
This(Screen) X: {_mainCamera.WorldToScreenPoint(position).x}, Y: {_mainCamera.WorldToScreenPoint(position).y}
Pivot(Screen) X: {_mainCamera.WorldToScreenPoint(pivotPosition).x}, Y: {_mainCamera.WorldToScreenPoint(pivotPosition).y}
Mouse(World) X: {mouseWorld.x}, Y: {mouseWorld.y}, Z: {mouseWorld.z}
This X: {position.x}, Y : {position.y}, Z: {position.z}
Pivot X: {pivotPosition.x}, Y : {pivotPosition.y}, Z: {pivotPosition.z}
Distance: {Vector2.Distance(position, mouseWorld)}
");
    }

    // Update is called once per frame
    private void FixedUpdate() {
        var pivotAroundPos = _pivotRectTransform.position;
        Vector3 mouse3d = Pointer.current.position.value;
        mouse3d.z = pivotAroundPos.z;
        var mouseWorld = _mainCamera.ScreenToWorldPoint(mouse3d);

        LookAt(mouseWorld, pivotAroundPos);
    }

    private Ray LookAt(Vector3 target, Vector3 pivotAroundPos) {
        var moveDirection = target - pivotAroundPos;
        // TODO: Maybe replace with Vector2.Angle()
        var angle = Angle(moveDirection);
        ;
        angle = (angle >= 90.0f) ? angle - 90.0f : 270.0f + angle;
        var angleAxis = Quaternion.AngleAxis(angle, Vector3.forward);
        _rectTransform.rotation = angleAxis;

        var lookingRay = new Ray(pivotAroundPos, angleAxis * Vector3.up);

        if(pivotAround == gameObject) return lookingRay;

        _rectTransform.position = lookingRay.GetPoint(_offset.magnitude);

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