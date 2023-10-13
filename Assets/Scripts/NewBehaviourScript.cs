using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;

public class NewBehaviourScript : MonoBehaviour
{
    private static readonly bool HexagonalPacking = true;
    const float Ang60 = 60 * (MathF.PI / 180);
    static readonly float Cos60 = MathF.Cos(Ang60);
    static readonly float Sin60 = MathF.Sin(Ang60);

    /*
     * Config
     */
    // Reference to the Prefab. Drag a Prefab into this field in the Inspector.
    public byte zIndex = 70;
    public GameObject ball;
    public byte ballsPerRow = 10;
    public byte rowCount = 5;

    private Vector3 _canvasBottomLeft;
    private Vector3 _canvasTopRight;
    
    private List<List<GameObject>> _grid = new List<List<GameObject>>();

    // Calculated using screen size
    private float _ballDiameter;
    
    // Start is called before the first frame update
    void Start()
    {
        // Validate parameters
        Assert.IsNotNull(GetComponent<Canvas>());
        Assert.IsNotNull(ball);

        CalculateScreen();
    }

    private static readonly TimeSpan UpdateEvery = new TimeSpan(0, 0, 1);
    private DateTime _lastUpdate;

    // Update is called once per frame
    void Update()
    {
        // Wait for 1 second before each update to reduce load
        if((DateTime.Now - _lastUpdate) < UpdateEvery)
        {
            return;
        }
        
        _lastUpdate = DateTime.Now;
        
        CreateBallGrid();
    }

    void CalculateScreen()
    {
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

    void CalculateBallDiameter()
    {
        // Resize balls
        _ballDiameter = (_canvasTopRight.x - _canvasBottomLeft.x) / ballsPerRow;
        Assert.IsTrue(_ballDiameter > 0);
        // Debug.LogFormat("ballSize: {0}", _ballDiameter);
    }

    #if UNITY_EDITOR
    void OnRectTransformDimensionsChange()
    {
        print("OnRectTransformDimensionsChange");
    }
    #endif

    // TODO: Reuse pool, instead of instantiate
    private void CreateBallGrid()
    {
        // Clear all existing
        if(_grid.Count != 0)
        {
            foreach(var row in _grid)
            {
                foreach(var cell in row)
                {
                    Destroy(cell);
                }
            }
            _grid.Clear();
        }

        var ballRadius = _ballDiameter / 2;
        
        var yOff = Sin60 * _ballDiameter;
        
        // (topRight.x - bottomLeft.x) / ballsPerRow
        Assert.IsTrue(_ballDiameter > 0);
        for(var yPos = 0; yPos < rowCount; yPos++)
        {
            var oddRow = yPos % 2; // 1 or 0
            var countForRow = ballsPerRow - oddRow + 1;

            var row = new List<GameObject>(countForRow);
            _grid.Add(row);
            for(byte xPos = 0; xPos < countForRow - 1; xPos++)
            {
                var x = _canvasBottomLeft.x + ballRadius + _ballDiameter * xPos;
                if(oddRow > 0)
                {
                    x += ballRadius;
                }
                var y = _canvasTopRight.y - ballRadius - yOff * yPos;
                var pos = new Vector3(x, y, zIndex);
                // Instantiate at position (0, 0, 0) and zero rotation.
                var newObject = Instantiate(ball, pos, Quaternion.identity);
                // TODO: Set Layer
                row.Add(newObject);


                // Text position
                // SpriteRenderer rectTransform = newObject.GetComponentInChildren<SpriteRenderer>();
                newObject.transform.localScale = new Vector2(_ballDiameter, _ballDiameter);
                // rectTransform.localPosition = new Vector3(0, 0, 0);
                // rectTransform.sizeDelta = new Vector2(400, 200);
                // break;
            }
        }
    }
}