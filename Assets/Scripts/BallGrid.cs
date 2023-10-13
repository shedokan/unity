using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BallGrid : MonoBehaviour {
    private const float Ang60 = 60 * (MathF.PI / 180);
    private static readonly float Sin60 = MathF.Sin(Ang60);

    /*
     * Config
     */
    public bool hexagonalPacking = true;
    // Reference to the Prefab. Drag a Prefab into this field in the Inspector.
    public byte zIndex = 70;
    public GameObject ball;
    public byte ballsPerRow = 10;
    public byte rowCount = 5;

    /*
     * Private
     */
    private Vector2 _canvasBottomLeft, _canvasTopRight;

    // Calculated using screen size
    private float _ballDiameter;

    private readonly List<List<GameObject>> _grid = new();

    // Start is called before the first frame update
    void Start() {
        // Validate parameters
        Assert.IsNotNull(GetComponent<Canvas>());
        Assert.IsNotNull(ball);

        CalculateScreen();
    }

    private static readonly TimeSpan UpdateEvery = new TimeSpan(0, 0, 1);
    private DateTime _lastUpdate;

    // Update is called once per frame
    void Update() {
        // Wait for 1 second before each update to reduce load
        if((DateTime.Now - _lastUpdate) < UpdateEvery) {
            return;
        }

        _lastUpdate = DateTime.Now;

        CreateBallGrid();
    }

    void CalculateScreen() {
        // Fetch the RectTransform from the GameObject
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        // _rectTransform.transform.
        rectTransform.GetWorldCorners(corners);

        _canvasBottomLeft = corners[0];
        _canvasTopRight = corners[2];

        Debug.LogFormat("left x: {0}, right x {1}", _canvasBottomLeft.x, _canvasTopRight.x);

        CalculateBallDiameter();
    }

    void CalculateBallDiameter() {
        // Resize balls
        _ballDiameter = (_canvasTopRight.x - _canvasBottomLeft.x) / ballsPerRow;
        Assert.IsTrue(_ballDiameter > 0);
        // Debug.LogFormat("ballSize: {0}", _ballDiameter);
    }

#if UNITY_EDITOR
    void OnRectTransformDimensionsChange() {
        print("OnRectTransformDimensionsChange");
    }
#endif

    // TODO: Reuse pool, instead of instantiate
    private void CreateBallGrid() {
        // Clear all existing
        if(_grid.Count != 0) {
            foreach(var cell in _grid.SelectMany(row => row)) {
                Destroy(cell);
            }

            _grid.Clear();
        }

        var ballRadius = _ballDiameter / 2;

        var yOff = hexagonalPacking ? Sin60 * _ballDiameter : _ballDiameter;

        Assert.IsTrue(_ballDiameter > 0);
        for(var yPos = 0; yPos < rowCount; yPos++) {
            var y = _canvasTopRight.y - ballRadius - yOff * yPos;
            
            var oddRow = yPos % 2; // 1 or 0
            var countForRow = ballsPerRow +1 - (hexagonalPacking ? oddRow : 0);

            // TODO: Don't recreate rows
            var row = new List<GameObject>(countForRow);
            _grid.Add(row);
            for(byte xPos = 0; xPos < countForRow - 1; xPos++) {
                var x = _canvasBottomLeft.x + ballRadius + _ballDiameter * xPos;
                if(hexagonalPacking && oddRow > 0) {
                    x += ballRadius;
                }
                
                // TODO: Use object pooling
                var pos = new Vector3(x, y, zIndex);
                var newObject = Instantiate(ball, pos, Quaternion.identity);
                row.Add(newObject);

                newObject.transform.localScale = new Vector2(_ballDiameter, _ballDiameter);
            }
        }
    }
}