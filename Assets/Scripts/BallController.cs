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
    public Color Color {
        get => _color;
        set {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = _color = value;
        }
    }
    
    public OffsetCoord? Coord;

    private bool Moving => Coord is not null;
    private TargetJoint2D _targetJoint2D;

    private TargetJoint2D TargetJoint2D {
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
    void ResetBall(Color color, OffsetCoord coord) {
        Color = color;
        Coord = coord;
        
        TargetJoint2D.enabled = true;
    }

    [UsedImplicitly]
    void StartMoving(Color color) {
        Color = color;
        Coord = null;

        TargetJoint2D.enabled = false;
    }

    void StopMoving() {
        TargetJoint2D.autoConfigureTarget = false;
        var coord = BallGrid.Current.PosToOffset(transform.position);
        BallGrid.Current.PlaceGameObject(coord, this);
        TargetJoint2D.target = BallGrid.Current.PosInGrid(coord);
        // TODO: Add to grid
        TargetJoint2D.enabled = true;

        Coord = coord;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if(!Moving || !collision.collider.CompareTag("Ball")) return;

        // TODO: Check if collided with wall
        // collision.otherCollider.CompareTag("Hitable")
        Debug.Log($"OnCollisionEnter2D, collider tag: {collision.collider}");
        // foreach (ContactPoint contact in collision.contacts)
        // {
        //     Debug.DrawLine(contact.point, contact.point + contact.normal * 100, Color.white);
        // }

        StopMoving();


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

    public void Reposition(Vector2 pos) {
        transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
    }
}