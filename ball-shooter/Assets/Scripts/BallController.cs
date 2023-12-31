using UnityEngine;
using UnityEngine.Assertions;

public class BallController : MonoBehaviour {
    private static readonly Color[] Colors = {
        new(1f, 0.7803922f, 0f, 1f),
        Color.green,
        Color.red,
        Color.magenta,
        Color.cyan
    };

    private Color _color;
    private bool _dropped;
    private Rigidbody2D _rigidBody2d;
    private TargetJoint2D _targetJoint2D; // Note: Auto reconfigure target is useful fo recalculation

    public Hex? hexCoords { get; private set; }

    public Color color {
        get => _color;
        set {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = _color = value;
        }
    }

    private bool moving => hexCoords is null;

    private void Awake() {
        _targetJoint2D = GetComponent<TargetJoint2D>();
        Assert.IsNotNull(_targetJoint2D);
        _rigidBody2d = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!moving || _dropped || !collision.collider.CompareTag("Ball")) return;

        Debug.Log($"OnCollisionEnter2D, collider tag: {collision.collider}");

        HitBall();
    }

#if UNITY_EDITOR
    private void OnValidate() {
        Assert.IsNotNull(GetComponent<Rigidbody2D>(), "Must have Rigidbody2D");
        Assert.IsNotNull(GetComponent<TargetJoint2D>(), "Must have TargetJoint2D");
    }
#endif

    /// <summary>Recycle the pool object for another use</summary>
    public void Recycle() {
        _dropped = false;
        hexCoords = null;
        ResetMovement();
    }

    private void ResetMovement() {
        _rigidBody2d.velocity = Vector2.zero;
        _rigidBody2d.gravityScale = 0;

        _targetJoint2D.enabled = true;
    }

    public static Color RandomColor() {
        var randIndex = Random.Range(0, Colors.Length);
        return Colors[randIndex];
    }

    public Vector2 LockPosition(Hex newHex, bool skipReposition = false) {
        hexCoords = newHex;

        var pos = BallGrid.current.PosInGrid(newHex);
        if(!skipReposition) transform.SetLocalPositionAndRotation(pos, Quaternion.identity);

        return pos;
    }

    public void Shoot(Vector2 force) {
        gameObject.layer = Layers.MovingBalls;
        hexCoords = null;

        _targetJoint2D.enabled = false;

        _rigidBody2d.AddForce(force);
    }

    private void HitBall() {
        gameObject.layer = Layers.IdleBalls;

        var ballGrid = BallGrid.current;
        var newHex = ballGrid.WorldPosToHex(transform.position).Round();
        if(!ballGrid.TryToPlaceBall(ref newHex, this)) {
            Destroy(gameObject);
            Debug.LogWarning($"Couldn't find place for ball in {newHex}, destroyed it instead");
            return;
        }

        ResetMovement();

        var pos = LockPosition(newHex);
        var worldPos = ballGrid.transform.TransformPoint(pos);
        _targetJoint2D.target = worldPos;

        ballGrid.CheckHit(newHex, color);
    }

    public void Drop() {
        _targetJoint2D.enabled = false;
        _rigidBody2d.gravityScale = 5;

        if(hexCoords is { } hex) {
            BallGrid.current.RemoveGameObject(hex);
            hexCoords = null;
        }

        _dropped = true;
        gameObject.layer = Layers.Falling;
    }

    public void PrepToShoot(RectTransform rectTransform) {
        transform.SetPositionAndRotation(rectTransform.position, rectTransform.rotation);
        gameObject.layer = Layers.Aimer;
    }
}