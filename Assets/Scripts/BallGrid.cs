using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Mathf = UnityEngine.Mathf;

/*
 * Holds the ball grid, and places balls in those positions.
 *
 * Expect to be a component
 */
public class BallGrid : MonoBehaviour {
    public static BallGrid Current { get; private set; }

    private const float Ang60 = 60 * (MathF.PI / 180);
    private static readonly float Sin60 = MathF.Sin(Ang60);

    /*
     * Config
     */
    public RectTransform gridOrigin;
    public GameObject ball;
    public byte ballsPerRow = 10;
    public byte rowCount = 5;
    public bool hexagonalPacking = true;

    /*
     * Private
     */
    private RectTransform _rectTransform;
    private Rect _rect;

    // Calculated using screen size
    private float _ballDiameter;

    private readonly List<List<GameObject>> _grid = new();

    // Start is called before the first frame update
    void Start() {
        Assert.IsNull(Current);
        Current = this;

        // Validate parameters
        Assert.IsNotNull(ball);
        _rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(_rectTransform);

        CalculateScreen();
    }

    private static readonly TimeSpan UpdateEvery = new(0, 0, 1);
    private DateTime _lastUpdate;

    // Update is called once per frame
    void Update() {
        // Wait for 1 second before each update to reduce load
        if((DateTime.Now - _lastUpdate) < UpdateEvery) {
            return;
        }

        _lastUpdate = DateTime.Now;
    }

    void CalculateScreen() {
        // Fetch the RectTransform from the GameObject
        _rect = _rectTransform.rect;

        if(CalculateBallDiameter()) {
            CreateBallGrid();
        }
    }

    bool CalculateBallDiameter() {
        var prevBallDiameter = _ballDiameter;
        
        // Recalculate
        _ballDiameter = _rect.width / ballsPerRow;
        Assert.IsTrue(_ballDiameter > 0);
        // Debug.LogFormat("Ball Diameter: {0}", _ballDiameter);

        return !Mathf.Approximately(_ballDiameter, prevBallDiameter);
    }

#if UNITY_EDITOR
    void OnRectTransformDimensionsChange() {
        print("OnRectTransformDimensionsChange");
        if(_rectTransform) { // TODO: Replace with didStart in unity 2023
            CalculateScreen();
        }
    }
#endif

    // TODO: Reuse pool, instead of instantiate
    private void CreateBallGrid() {
        // Clear all existing
        ClearGrid();

        var ballRadius = _ballDiameter / 2;
        var yOff = hexagonalPacking ? Sin60 * _ballDiameter : _ballDiameter;

        Vector2 pos = new();
        for(var yPos = 0; yPos < rowCount; yPos++) {
            pos.y = -ballRadius - yOff * yPos;

            var oddRow = hexagonalPacking && yPos % 2 != 0;
            var countForRow = ballsPerRow + 1;
            if(hexagonalPacking && oddRow) {
                countForRow -= 1;
            }

            // TODO: Don't recreate rows
            var row = new List<GameObject>(countForRow);
            _grid.Add(row);
            for(byte xPos = 0; xPos < countForRow - 1; xPos++) {
                pos.x = ballRadius + _ballDiameter * xPos;
                if(hexagonalPacking && oddRow) {
                    pos.x += ballRadius;
                }

                PutBall(ref pos, ref row);
            }
        }
    }

    private void ClearGrid() {
        if(_grid.Count == 0) return;
        
        foreach(var cell in _grid.SelectMany(row => row)) {
            Destroy(cell);
        }

        _grid.Clear();
    }

    void PutBall(ref Vector2 pos, ref List<GameObject> row) {
        // TODO: Use object pooling
        var newObject = Instantiate(ball, transform);
        row.Add(newObject);

        newObject.transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
    }

    public Vector2 RoundToNearestGrid(Vector2 point) {
        var (col, row) = PixelToHex(point);
        var originPos = transform.InverseTransformPoint(gridOrigin.position);
        var y = originPos.y - row * (Sin60 * _ballDiameter) - _ballDiameter / 2;

        var oddRow = hexagonalPacking && row % 2 != 0;

        var x = originPos.x + (col + 0.5f) * _ballDiameter;
        if(hexagonalPacking && oddRow) {
            x += _ballDiameter / 2;
        }

        return transform.TransformPoint(new Vector2(x, y));
    }

    public (int, int) PixelToHex(Vector2 point) {
        var point3d = transform.InverseTransformPoint(point);
        var originPos = transform.InverseTransformPoint(gridOrigin.position);
        // print($"{rect.xMax}: {(point.x - localPos.x)}; {rect.yMin}: {(localPos.y - point.y)}");
        // print($"{rect.xMax}: {(point.x - localPos.x)}; {rect.yMin}: {(localPos.y - point.y)}");
        var yOff = hexagonalPacking ? (Sin60 * _ballDiameter) : _ballDiameter;
        var row = (int)Math.Floor((originPos.y - point3d.y) / yOff); // Row
        if(row < 0) row = 0;
        ;

        var oddRow = hexagonalPacking && row % 2 != 0;
        var xInGrid = point3d.x - originPos.x;
        if(hexagonalPacking && oddRow) {
            xInGrid -= _ballDiameter / 2;
        }

        var col = (int)Math.Floor(xInGrid / _ballDiameter);
        col = Math.Clamp(col, 0, hexagonalPacking && oddRow ? ballsPerRow - 2 : ballsPerRow);
        return (col, row);
    }
}