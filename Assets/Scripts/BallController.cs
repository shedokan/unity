using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

public class BallController : MonoBehaviour {
    private static readonly Color[] Colors = new[] {
        new Color(1f, 0.7803922f, 0f, 1f),
        Color.green,
        Color.red,
        Color.magenta,
        Color.cyan
    };

    public static Color RandomColor() {
        var randIndex = Random.Range(0, Colors.Length);
        return Colors[randIndex];
    }

    private Color _color;
    public Color color {
        get => _color;
        set {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = _color = value;
        }
    }
    
    private Hex? _hexCoords;

    private bool moving => _hexCoords is null;
    private TargetJoint2D _targetJoint2D;

    private TargetJoint2D targetJoint2D {
        get {
            if(!_targetJoint2D) {
                _targetJoint2D = GetComponent<TargetJoint2D>();
                Assert.IsNotNull(_targetJoint2D);
            }

            return _targetJoint2D;
        }
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    [UsedImplicitly]
    public Vector2 LockPosition(Hex newHex) {
        _hexCoords = newHex;
        targetJoint2D.enabled = true;

        var pos = BallGrid.current.PosInGrid(newHex);
        Debug.Log($"LockPosition({newHex}: {pos}");
        transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
        // transform.position = pos;

        
        // targetJoint2D.autoConfigureTarget = false;
        // targetJoint2D.target =  BallGrid.current.transform.TransformPoint(pos);;
        
        return pos;
    }

    [UsedImplicitly]
    void StartMoving(Color color_) {
        this.color = color_;
        _hexCoords = null;

        targetJoint2D.enabled = false;
    }

    void StopMoving() {
        var newHex = BallGrid.current.PosToHex(transform.position).Round();
        newHex = BallGrid.current.PlaceGameObject(newHex, this);
        // TODO: Add to grid

        var pos = LockPosition(newHex);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if(!moving || !collision.collider.CompareTag("Ball")) return;

        // TODO: Check if collided with wall
        // collision.otherCollider.CompareTag("Hitable")
        Debug.Log($"OnCollisionEnter2D, collider tag: {collision.collider}");
        // foreach (ContactPoint contact in collision.contacts)
        // {
        //     Debug.DrawLine(contact.point, contact.point + contact.normal * 100, Color.white);
        // }

        StopMoving();
        
        Debug.Log("sdasd");


        // var colliderBallController = collision.collider.gameObject.GetComponent<BallController>();
        // if(colliderBallController.moving) return;
        // // Falling
        // collision.collider.attachedRigidbody.gravityScale = 1;
        // collision.collider.gameObject.GetComponent<TargetJoint2D>().enabled = false;
        // collision.collider.gameObject.GetComponent<BallController>().moving = true;
        //
        // // Us
        // collision.otherCollider.attachedRigidbody.gravityScale = 1;

        // if (collision.relativeVelocity.magnitude > 2)
        //     audioSource.Play();
    }
}