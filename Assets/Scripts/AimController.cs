using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

/// <summary>Add to aim center</summary>
public class AimController : MonoBehaviour {
    [Header("Aim")] [Tooltip("Aim pointer object")]
    public RectTransform pointer;

    [Range(0, 180)] public float maxAngle = 90;

    [Header("Shooting")] public GameObject projectilePrefab;

    [Min(0)] public float shootThrust = 5000; // Should be positive

    private Vector2? _activeTarget;
    private float _aimAngle;
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
        _activeTarget = null;
        var pointerWorld = PointerWorldPos();
        if(pointerWorld is null) {
            return;
        }

        (_aimRay, _aimAngle) = PointAt(_rectTransform.position, (Vector2)pointerWorld);

        if(Mathf.Abs(_aimAngle) > maxAngle) {
            Debug.Log($"OnFire: Over max angle: abs({_aimAngle}) > {maxAngle}");
            return;
        }

        var hit = Physics2D.Raycast(_aimRay.origin, _aimRay.direction, float.PositiveInfinity,
            Layers.RaycastLayers);
        if(!hit) return;

        DottedLine.DottedLine.Instance.DrawDottedLine(_aimRay.origin, hit.point);

        Debug.DrawLine(_aimRay.origin, hit.point, Color.white);
        _activeTarget = hit.point;
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
Angle: {_aimAngle}
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
        if(_activeTarget is null) return;

        Debug.DrawLine(_aimRay.origin, (Vector2)_activeTarget, Color.red, 2);

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
    private (Ray2D, float) PointAt(Vector2 myWorldPos, Vector2 targetWorldPos) {
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

        return (lookingRay, angle);
    }

    private BallController NewProjectile() {
        var projectile = BallGrid.current.pool.Get();
        projectile.PrepToShoot(_rectTransform);
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