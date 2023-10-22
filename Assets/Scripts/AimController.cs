using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

/// <summary>Add to aim center</summary>
public class AimController : MonoBehaviour {
    [Tooltip("Aim pointer object")] public RectTransform pointer;

    public GameObject projectilePrefab;
    public Transform projectileParent;

    public float shootThrust = 5000; // Should be positive

    private Ray2D _aimRay;
    private BallController _currProjectile;
    private Camera _mainCamera;
    private Vector2 _pointerOffset;
    private RectTransform _rectTransform;

    // Start is called before the first frame update
    private void Awake() {
        _mainCamera = Camera.main;
        _rectTransform = GetComponent<RectTransform>();

        if(pointer) {
            _pointerOffset = pointer.position - _rectTransform.position;
        }

        var ballController = projectilePrefab.GetComponent<BallController>();
        Assert.IsNotNull(ballController);
    }

    private void Start() {
        _currProjectile = NewProjectile();
    }

    // Update is called once per frame
    private void FixedUpdate() {
        var pointerWorld = PointerWorldPos();
        if(pointerWorld is not null) {
            _aimRay = PointAt(_rectTransform.position, (Vector2)pointerWorld);
        }
    }

    private void OnGUI() {
        var pointerWorldPos = PointerWorldPos() ?? Vector2.zero;

        if(!BallGrid.current) return;

        var pointerHex = BallGrid.current.WorldPosToHex(pointerWorldPos);
        var roundedGrid = BallGrid.current.RoundToNearestGrid(pointerWorldPos);
        var gridRectTransform = BallGrid.current.GetComponent<RectTransform>();
        GUI.Box(new Rect(5, 25, 400, 100), "");
        //The Labels show what the Sliders represent
        GUI.Label(new Rect(10, 30, 400, 200), $@"Pointer(World): {pointerWorldPos}
Pointer(Hex): {pointerHex}
Pointer(Rounded): {roundedGrid}
ballGrid: {gridRectTransform.worldToLocalMatrix * pointerWorldPos}
Ray: {_aimRay.origin}
");
    }

#if UNITY_EDITOR
    private void OnRectTransformDimensionsChange() {
        if(_currProjectile) {
            var projTransform = _currProjectile.transform;
            projTransform.position = _rectTransform.position;
            projTransform.localScale = projectilePrefab.transform.localScale;
        }
    }
#endif

    private Vector2? PointerWorldPos() {
        // TODO: Support touch
        var inputPointer = Pointer.current;
        if(inputPointer is null || !inputPointer.wasUpdatedThisFrame) return null;

        Vector3 pointer3d = inputPointer.position.value;
        pointer3d.z = _rectTransform.position.z;
        return _mainCamera.ScreenToWorldPoint(pointer3d);
    }

    private void OnFire(InputValue value) {
        Debug.Log($"OnFire: {value}");
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
            pointer.SetPositionAndRotation(
                new Vector3(point.x, point.y, pointer.position.z),
                angleAxis);
        }

        return lookingRay;
    }

    private BallController NewProjectile() {
        var projectile = BallGrid.current.pool.Get();
        projectile.transform.SetPositionAndRotation(_rectTransform.position, _rectTransform.rotation);
        projectile.gameObject.layer = Layers.Aimer;
        projectile.color = BallController.RandomColor();

        return projectile;
    }

    /// <summary>
    /// </summary>
    /// <param name="ray"></param>
    private void ShootProjectile(Ray2D ray) {
        if(!_currProjectile) return;

        _currProjectile.Shoot(ray.direction * shootThrust);
    }
}