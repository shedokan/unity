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

    // Can't initialize on Start() due to usage before Start()
    private TargetJoint2D targetJoint2D {
        get {
            if(!_targetJoint2D) {
                _targetJoint2D = GetComponent<TargetJoint2D>();
                Assert.IsNotNull(_targetJoint2D);
            }

            return _targetJoint2D;
        }
    }

    private bool moving => hexCoords is null;

    private void Start() {
        _rigidBody2d = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rigidBody2d);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!moving || _dropped || !collision.collider.CompareTag("Ball")) return;

        Debug.Log($"OnCollisionEnter2D, collider tag: {collision.collider}");

        HitBall();
    }

    public static Color RandomColor() {
        var randIndex = Random.Range(0, Colors.Length);
        return Colors[randIndex];
    }

    public Vector2 LockPosition(Hex newHex, bool skipReposition = false) {
        hexCoords = newHex;
        targetJoint2D.enabled = true;

        var pos = BallGrid.current.PosInGrid(newHex);
        if(!skipReposition) transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
        // Debug.Log($"LockPosition({newHex}: {pos}");


        return pos;
    }

    public void Shoot(Vector2 force) {
        gameObject.layer = Layers.MovingBalls;
        hexCoords = null;

        targetJoint2D.enabled = false;

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

        var pos = LockPosition(newHex);
        var worldPos = ballGrid.transform.TransformPoint(pos);
        targetJoint2D.target = worldPos;

        _rigidBody2d.velocity = Vector2.zero;

        ballGrid.CheckHit(newHex, color);
    }

    public void Drop() {
        targetJoint2D.enabled = false;
        _rigidBody2d.gravityScale = 5;


        // TODO: Collide only with the dead zone
        // TODO: Don't let other balls collide with this one
        // GetComponent<Collider2D>().enabled = false;

        if(hexCoords is { } hex) {
            BallGrid.current.RemoveGameObject(hex);
            hexCoords = null;
        }

        _dropped = true;
        gameObject.layer = Layers.Falling;
    }
}