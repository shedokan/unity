using JetBrains.Annotations;
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

    private Hex? _hexCoords;

    // Note: Auto reconfigure target is useful fo recalculation
    private TargetJoint2D _targetJoint2D;

    public Color color {
        get => _color;
        set {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = _color = value;
        }
    }

    private bool moving => _hexCoords is null;

    private TargetJoint2D targetJoint2D {
        get {
            if(!_targetJoint2D) {
                _targetJoint2D = GetComponent<TargetJoint2D>();
                Assert.IsNotNull(_targetJoint2D);
            }

            return _targetJoint2D;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!moving || !collision.collider.CompareTag("Ball")) return;

        Debug.Log($"OnCollisionEnter2D, collider tag: {collision.collider}");

        StopMoving();
    }

    public static Color RandomColor() {
        var randIndex = Random.Range(0, Colors.Length);
        return Colors[randIndex];
    }

    [UsedImplicitly]
    public Vector2 LockPosition(Hex newHex, bool skipReposition = false) {
        _hexCoords = newHex;
        targetJoint2D.enabled = true;

        var pos = BallGrid.current.PosInGrid(newHex);
        if(!skipReposition) transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
        // Debug.Log($"LockPosition({newHex}: {pos}");

        return pos;
    }

    [UsedImplicitly]
    private void StartMoving(Color color_) {
        color = color_;
        _hexCoords = null;

        targetJoint2D.enabled = false;
    }

    private void StopMoving() {
        var newHex = BallGrid.current.WorldPosToHex(transform.position).Round();
        newHex = BallGrid.current.PlaceGameObject(newHex, this);

        var pos = LockPosition(newHex);
        var worldPos = BallGrid.current.transform.TransformPoint(pos);
        targetJoint2D.target = worldPos;
    }
}