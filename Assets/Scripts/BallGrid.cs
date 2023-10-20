using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


/*
 * Holds the ball grid, and places balls in those positions.
 *
 * Expect to be a component
 */
public class BallGrid : MonoBehaviour {
    /*
     * Config
     */
    public RectTransform gridOrigin;
    public GameObject ballObject;
    public byte ballsPerRow = 10;
    public byte rowCount = 5;
    public bool hexagonalPacking = true;
    public int groupHitThreshold = 3;

    private readonly HexMap<BallController> _grid = new();

    /// Calculated using screen size
    private float _ballDiameter;

    private HexLayout _gridLayout;

    /// Local space
    private Rect _rect;

    /*
     * Private
     */
    private RectTransform _rectTransform;
    public static BallGrid current { get; private set; }

    // Start is called before the first frame update
    private void Start() {
        Assert.IsNull(current);
        current = this;

        // Validate parameters
        Assert.IsNotNull(ballObject);
        _rectTransform = GetComponent<RectTransform>();
        Assert.IsNotNull(_rectTransform);

        CalculateScreen();
        BallsCreate();
    }

#if UNITY_EDITOR
    private void OnRectTransformDimensionsChange() {
        print("OnRectTransformDimensionsChange");
        if(_rectTransform)
            // TODO: Replace with didStart in unity 2023
            CalculateScreen();
    }
#endif

    private void CalculateScreen() {
        // Fetch the RectTransform from the GameObject
        _rect = _rectTransform.rect;

        if(CalculateBallDiameter()) {
            var radiusVector2 = new Vector2(_ballDiameter / 2, _ballDiameter / 2);
            _gridLayout = new HexLayout(HexOrientation.Circular, radiusVector2, radiusVector2);

            BallReposition();
        }
    }

    private bool CalculateBallDiameter() {
        var prevBallDiameter = _ballDiameter;

        // Recalculate, keeping half empty
        _ballDiameter = _rect.width / (ballsPerRow + 0.5f);
        Assert.IsTrue(_ballDiameter > 0);
        // Rescale prefab
        ballObject.transform.localScale = new Vector2(_ballDiameter, _ballDiameter);

        return !Mathf.Approximately(_ballDiameter, prevBallDiameter);
    }

    private void BallsCreate() {
        _grid.FillRectangle(Vector2Int.zero, new Vector2Int(ballsPerRow - 1, rowCount - 1), hex => {
            var ball = CreateBall();
            ball.LockPosition(hex);

            return ball;
        });

        Debug.Log("CreateBalls: Done");
    }

    private void BallReposition() {
        foreach(var (hex, value) in _grid.hexes) value.LockPosition(hex);
    }


    private BallController CreateBall() {
        // TODO: Use object pooling
        var newObject = Instantiate(ballObject, transform);

        var ballController = newObject.GetComponent<BallController>();
        ballController.color = BallController.RandomColor();

        return ballController;
    }

    private void ClearGrid() {
        if(_grid.hexes.Count == 0) return;

        foreach(var (_, ball) in _grid.hexes) Destroy(ball.gameObject);

        _grid.hexes.Clear();
    }

    public Vector2 RoundToNearestGrid(Vector2 point) {
        return PosInGrid(WorldPosToHex(point).Round());
    }

    /// <param name="worldPos">A position in world space</param>
    /// <returns>Hex coordinates of the given position</returns>
    public FractionalHex WorldPosToHex(Vector3 worldPos) {
        var point3d = transform.InverseTransformPoint(worldPos);
        var originPos = transform.InverseTransformPoint(_rectTransform.position);

        var gridPos = new Vector2(
            point3d.x - originPos.x,
            -(point3d.y - originPos.y)
        );
        return _gridLayout.PixelToHex(gridPos);
    }

    public Vector2Int PosToOffset(Vector2 point) {
        return WorldPosToHex(point).Round().ToOffset();
    }

    /// <returns>
    ///     A Vector position in local space
    /// </returns>
    // TODO: Move ball grid to global space, without scaling issues
    public Vector3 PosInGrid(Hex hex) {
        var originPos = _rectTransform.InverseTransformPoint(_rectTransform.position);

        var pos = _gridLayout.HexToPixel(hex);
        pos.x += originPos.x;
        pos.y = originPos.y - pos.y;

        return pos;
    }

    /// <summary>
    ///     Tries to place the <paramref name="ball" /> in <paramref name="hex" />
    ///     If it can't tries to position it in one of the neighbours and updates <paramref name="hex" />
    /// </summary>
    /// <param name="hex">Placement coordinates</param>
    /// <param name="ballController">Controller of the ball to place</param>
    /// <returns>True if successful</returns>
    public bool TryToPlaceBall(ref Hex hex, BallController ballController) {
        var canPlace = !_grid.hexes.ContainsKey(hex);
        // Find a different place
        if(!canPlace)
            foreach(var direction in Hex.directions) {
                var hexInDir = hex.Add(direction);
                canPlace = !_grid.hexes.ContainsKey(hexInDir);
                if(canPlace) {
                    hex = hexInDir;
                    break;
                }
            }

        if(!canPlace) return false;

        _grid.hexes.Add(hex, ballController);

        return true;
    }

    public void RemoveGameObject(Hex hex) {
        _grid.hexes.Remove(hex);
    }

    /// <summary>
    ///     Checks if there is a group of balls of the same color that is more than the threshold
    /// </summary>
    /// <param name="origHex">Starting coordinates</param>
    /// <param name="color">Color to filter</param>
    /// <returns>true if there was a hit</returns>
    public bool CheckHit(Hex origHex, Color color) {
        HashSet<Hex> visited = new();
        HashSet<Hex> same = new();
        Queue<Hex> toVisit = new();
        toVisit.Enqueue(origHex);

        while(toVisit.Count > 0) {
            var hex = toVisit.Dequeue();
            same.Add(hex);
            visited.Add(hex);

            foreach(var hexInDir in Hex.directions.Select(dir => hex.Add(dir))) {
                if(visited.Contains(hexInDir)) continue;

                if(_grid.hexes.TryGetValue(hexInDir, out var currBall) && currBall.color == color) {
                    toVisit.Enqueue(hexInDir);
                    continue;
                }

                // Add it to avoid fetching it again from grid
                visited.Add(hexInDir);
            }
        }

        var overThreshold = same.Count >= groupHitThreshold;
        if(!overThreshold) return false;

        Debug.Log($"countSame: {same.Count}");
        foreach(var hex in same) _grid.hexes[hex].Drop();

        return true;
    }
}