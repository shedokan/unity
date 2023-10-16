using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Mathf = UnityEngine.Mathf;
using Vector2 = UnityEngine.Vector2;

/*
 * Holds the ball grid, and places balls in those positions.
 *
 * Expect to be a component
 */
public class BallGrid : MonoBehaviour {
    public static BallGrid current { get; private set; }

    private const float Ang60 = 60 * (MathF.PI / 180);
    private static readonly float Sin60 = MathF.Sin(Ang60);

    /*
     * Config
     */
    public RectTransform gridOrigin;
    public GameObject ballObject;
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

    private readonly List<List<BallController?>> _grid = new();
    private byte _maxRows;

    // Start is called before the first frame update
    void Start() {
        Assert.IsNull(current);
        current = this;

        // Validate parameters
        Assert.IsNotNull(ballObject);
        _rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(_rectTransform);

        CalculateScreen();
    }

    private static readonly TimeSpan UpdateEvery = new(0, 0, 1);
    private DateTime _lastUpdate;
    private float _yOff;

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
            _yOff = hexagonalPacking ? Sin60 * _ballDiameter : _ballDiameter;
            _maxRows = (byte)Math.Round(_rect.height / _yOff);

            CreateBallGrid();
            CreateBalls();
        }

        // Debug.LogFormat("CalculateScreen() - rect: {0}, pivot: {1}", gridOrigin.rect, gridOrigin.position);
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
    private void CreateBallGrid() {
        // Clear all existing
        // TODO: Stop clearing grid
        ClearGrid();

        // TODO: Remove excess
        // if (_grid.Count > _maxRows) _grid.RemoveRange(_maxRows, _grid.Count - _maxRows);
        _grid.Capacity = _maxRows;

        for(var yIndex = _grid.Count; yIndex < _maxRows; yIndex++) {
            var oddRow = hexagonalPacking && yIndex % 2 != 0;
            var countForRow = ballsPerRow + 1;
            if(hexagonalPacking && oddRow) {
                countForRow -= 1;
            }

            var row = new List<BallController?>(countForRow);
            _grid.Add(row);
        }
    }

    private void CreateBalls() {
        var ballRadius = _ballDiameter / 2;

        Vector2 pos = new();
        for(byte yIndex = 0; yIndex < rowCount; yIndex++) {
            var row = _grid[yIndex];
            pos.y = -ballRadius - _yOff * yIndex;

            var oddRow = hexagonalPacking && yIndex % 2 != 0;
            var countForRow = row.Capacity;

            for(byte xIndex = 0; xIndex < countForRow - 1; xIndex++) {
                var ball = row.ElementAtOrDefault(xIndex);
                if(ball is null) {
                    ball = CreateBall();
                    row.Insert(xIndex, ball);
                }

                pos.x = ballRadius + _ballDiameter * xIndex;
                if(hexagonalPacking && oddRow) {
                    pos.x += ballRadius;
                }

                ball.SendMessage("ResetBall", ball.Color);
                ball.Reposition(pos);
            }
        }
    }


    private BallController CreateBall() {
        // TODO: Use object pooling
        var newObject = Instantiate(ballObject, transform);

        var ballController = newObject.GetComponent<BallController>();
        ballController.Color = BallController.RandomColor();

        return ballController;
    }


    private void ClearGrid() {
        if(_grid.Count == 0) return;

        foreach(var cell in _grid.SelectMany(row => row)) {
            if(cell) Destroy(cell.gameObject);
        }

        _grid.Clear();
    }

    public Vector2 RoundToNearestGrid(Vector2 point) {
        return PosInGrid(PosToOffset(point));
    }

    // Point is in world space
    public Vector2Int PosToOffset(Vector2 point) {
        var point3d = transform.InverseTransformPoint(point);
        var originPos = transform.InverseTransformPoint(gridOrigin.position);
        var yOff = hexagonalPacking ? (Sin60 * _ballDiameter) : _ballDiameter;
        var row = (int)Math.Floor((originPos.y - point3d.y) / yOff); // Row
        if(row < 0) row = 0;


        var oddRow = hexagonalPacking && row % 2 != 0;
        var xInGrid = point3d.x - originPos.x;
        if(hexagonalPacking && oddRow) {
            xInGrid -= _ballDiameter / 2;
        }

        var col = (int)Math.Floor(xInGrid / _ballDiameter);
        col = Math.Clamp(col, 0, hexagonalPacking && oddRow ? ballsPerRow - 2 : ballsPerRow);
        return new Vector2Int(col, row);
    }

    /**
     * Returns position in local space
     */
    // TODO: Move ball grid to global space, without scaling issues
    public Vector2 PosInGrid(Vector2Int coord) {
        var originPos = transform.InverseTransformPoint(gridOrigin.position);
        var y = originPos.y - coord.y * (Sin60 * _ballDiameter) - _ballDiameter / 2;

        var oddRow = hexagonalPacking && coord.y % 2 != 0;

        var x = originPos.x + (coord.x + 0.5f) * _ballDiameter;
        if(hexagonalPacking && oddRow) {
            x += _ballDiameter / 2;
        }

        return transform.TransformPoint(new Vector2(x, y));
    }

    public void PlaceGameObject(Vector2Int coord, BallController ballController) {
        var row = _grid.ElementAtOrDefault(coord.y);
        Assert.IsNotNull(row);
        var cell = row?.ElementAtOrDefault(coord.x);
        Assert.IsNull(cell);
        // TODO: prep this ahead of time
        while(row.Count < coord.x) row.Add(null);

        row.Add(ballController);
    }
}