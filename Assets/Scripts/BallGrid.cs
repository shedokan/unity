using System;
using UnityEngine;
using UnityEngine.Assertions;


/*
 * Holds the ball grid, and places balls in those positions.
 *
 * Expect to be a component
 */
public class BallGrid : MonoBehaviour
{
    public static BallGrid current { get; private set; }

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
    /// Local space
    private Rect _rect; 

    /// Calculated using screen size
    private float _ballDiameter;

    private HexLayout _gridLayout;
    private readonly HexMap<BallController> _grid = new();

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsNull(current);
        current = this;

        // Validate parameters
        Assert.IsNotNull(ballObject);
        _rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(_rectTransform);

        CalculateScreen();
        BallsCreate();
    }

    private static readonly TimeSpan UpdateEvery = new(0, 0, 1);
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
    }

    void CalculateScreen()
    {
        // Fetch the RectTransform from the GameObject
        _rect = _rectTransform.rect;

        if(CalculateBallDiameter())
        {
            var radiusVector2 = new Vector2(_ballDiameter / 2, _ballDiameter / 2);
            _gridLayout = new HexLayout(HexOrientation.Circular, radiusVector2, radiusVector2);

            BallReposition();
        }
    }

    bool CalculateBallDiameter()
    {
        var prevBallDiameter = _ballDiameter;

        // Recalculate, keeping half empty
        _ballDiameter = _rect.width / (ballsPerRow + 0.5f);
        Assert.IsTrue(_ballDiameter > 0);
        // Rescale prefab
        ballObject.transform.localScale = new Vector2(_ballDiameter, _ballDiameter);

        return !Mathf.Approximately(_ballDiameter, prevBallDiameter);
    }

#if UNITY_EDITOR
    void OnRectTransformDimensionsChange()
    {
        print("OnRectTransformDimensionsChange");
        if(_rectTransform)
        {
            // TODO: Replace with didStart in unity 2023
            CalculateScreen();
        }
    }
#endif
    private void BallsCreate()
    {
        _grid.FillRectangle(Vector2Int.zero, new Vector2Int(ballsPerRow - 1, rowCount - 1), hex =>
        {
            var ball = CreateBall();
            ball.LockPosition(hex);

            return ball;
        });

        Debug.Log("CreateBalls: Done");
    }

    private void BallReposition()
    {
        foreach(var (hex, value) in _grid.hexes)
        {
            value.LockPosition(hex);
        }
    }


    private BallController CreateBall()
    {
        // TODO: Use object pooling
        var newObject = Instantiate(ballObject, transform);

        var ballController = newObject.GetComponent<BallController>();
        ballController.color = BallController.RandomColor();

        return ballController;
    }

    private void ClearGrid()
    {
        if(_grid.hexes.Count == 0) return;

        foreach(var (_, ball) in _grid.hexes)
        {
            Destroy(ball.gameObject);
        }

        _grid.hexes.Clear();
    }

    public Vector2 RoundToNearestGrid(Vector2 point)
    {
        return PosInGrid(WorldPosToHex(point).Round());
    }

    /// <param name="worldPos">A position in world space</param>
    /// <returns>Hex coordinates of the given position</returns>
    public FractionalHex WorldPosToHex(Vector3 worldPos)
    {
        var point3d = transform.InverseTransformPoint(worldPos);
        var originPos = transform.InverseTransformPoint(_rectTransform.position);

        var gridPos = new Vector2(
            point3d.x - originPos.x,
            -(point3d.y - originPos.y)
        );
        return _gridLayout.PixelToHex(gridPos);
    }

    public Vector2Int PosToOffset(Vector2 point)
    {
        return WorldPosToHex(point).Round().ToOffset();
    }

    /// <returns>
    /// A Vector position in local space
    /// </returns>
    // TODO: Move ball grid to global space, without scaling issues
    public Vector3 PosInGrid(Hex hex)
    {
        var originPos = _rectTransform.InverseTransformPoint(_rectTransform.position);

        var pos = _gridLayout.HexToPixel(hex);
        pos.x += originPos.x;
        pos.y = originPos.y - pos.y;

        return pos;
    }

    public Hex PlaceGameObject(Hex hex, BallController ballController)
    {
        var canPlace = !_grid.hexes.ContainsKey(hex);
        // Find a different place
        if(!canPlace)
        {
            foreach(var direction in Hex.directions)
            {
                var newHex = hex.Add(direction);
                canPlace = !_grid.hexes.ContainsKey(newHex);
                if(canPlace)
                {
                    hex = newHex;
                    break;
                }
            }
        }

        Assert.IsTrue(canPlace, $"Can't place: {hex}");

        _grid.hexes.Add(hex, ballController);

        return hex;
    }
}