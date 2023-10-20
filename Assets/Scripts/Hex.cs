using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// https://www.redblobgames.com/grids/hexagons/
// TODO: Reuse Vector3Int for performance?
public readonly struct FractionalHex {
    public float q { get; }
    public float r { get; }
    public float s { get; }

    // Cube
    public FractionalHex(float q, float r, float s) {
        this.q = q;
        this.r = r;
        this.s = s;
        Assert.IsTrue(Mathf.Approximately(q + r + s, 0f), $"FractionalHex(r: {r}, q: {q}, s: {s}) - sum={q + r + s}");
    }

    // Axial
    public FractionalHex(float q, float r)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;
    }

    public Hex Round() {
        var q = (int)Mathf.Round(this.q);
        var r = (int)Mathf.Round(this.r);
        var s = (int)Mathf.Round(this.s);

        var qDiff = Mathf.Abs(q - this.q);
        var rDiff = Mathf.Abs(r - this.r);
        var sDiff = Mathf.Abs(s - this.s);

        if(qDiff > rDiff && qDiff > sDiff)
            q = -r - s;
        else if(rDiff > sDiff)
            r = -q - s;
        else
            s = -q - r;

        return new Hex(q, r, s);
    }
}


// Supports both Cube and Axial coordinates
public readonly struct Hex {
    public int q { get; }
    public int r { get; }
    public int s { get; }

    // Cube
    public Hex(int q, int r, int s) {
        this.q = q;
        this.r = r;
        this.s = s;
        Assert.IsTrue(q + r + s == 0, $"Hex(r: {r}, q: {q}, s: {s}), sum={q + r + s}");
    }

    // Axial
    public Hex(int q, int r) : this(q, r, -q -r) { }

    public Hex Add(Hex b) {
        return new Hex(q + b.q, r + b.r, s + b.s);
    }


    public Hex Subtract(Hex b) {
        return new Hex(q - b.q, r - b.r, s - b.s);
    }


    public Hex Scale(int k) {
        return new Hex(q * k, r * k, s * k);
    }


    public Hex RotateLeft() {
        return new Hex(-s, -q, -r);
    }


    public Hex RotateRight() {
        return new Hex(-r, -s, -q);
    }

    public static readonly List<Hex> directions = new(6) {
        new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)
    };

    public static Hex Direction(int direction) {
        return directions[direction];
    }
    
    public Hex Neighbor(int direction) {
        Assert.IsTrue(direction is >= 0 and < 6);
        return Add(Direction(direction));
    }

    public static readonly List<Hex> diagonals = new(6) {
        new Hex(2, -1, -1), new Hex(1, -2, 1), new Hex(-1, -1, 2), new Hex(-2, 1, 1), new Hex(-1, 2, -1),
        new Hex(1, 1, -2)
    };

    public Hex DiagonalNeighbor(int direction) {
        Assert.IsTrue(direction is >= 0 and < 6);
        return Add(diagonals[direction]);
    }
    
    public int Length() {
        return (Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2;
    }
    
    public int Distance(Hex b) {
        return Subtract(b).Length();
    }

    public Vector2Int ToOffset() {
        // TODO: Implement
        // throw new NotImplementedException();
        return new Vector2Int(0, 0);
    }

    public static Hex PixelToPointyHex(Vector2 point, float hexSize) {
        var q = (Mathf.Sqrt(3) / 3f * point.x - 1f / 3f * point.y) / hexSize;
        var r = 2f / 3f * point.y / hexSize; // Row
        return new FractionalHex(q, r).Round();
    }

    public override string ToString()
    {
        return $"Hex(q: {q}, r={r}, s={s})";
    }
}

readonly struct HexOrientation
{
    private HexOrientation(float qToX, float rToX, float qToY, float rToY, float xToQ, float yToQ, float xToR, float yToR, float startAngle)
    {
        this.qToX = qToX;
        this.rToX = rToX;
        this.qToY = qToY;
        this.rToY = rToY;
        this.xToQ = xToQ;
        this.yToQ = yToQ;
        this.xToR = xToR;
        this.yToR = yToR;
        this.startAngle = startAngle;
    }
    // Hex to pixel
    public readonly float qToX; // f0, q -> x
    public readonly float rToX; // f1, r -> x
    public readonly float qToY; // f2, q -> y
    public readonly float rToY; // f3, r -> y
    
    // Pixel to Hex
    public readonly float xToQ; // b0
    public readonly float yToQ; // b1
    public readonly float xToR; // b2
    public readonly float yToR; // b3
    public readonly float startAngle;

    private const float Ang60 = 60 * (MathF.PI / 180);
    private static readonly float Sin60 = MathF.Sin(Ang60);
    private static readonly float Cos60 = MathF.Cos(Ang60);

    public static HexOrientation Pointy = new(Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2.0f, 0.0f, 3.0f / 2.0f,
        Mathf.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f, 0.5f);
    public static HexOrientation Flat = new(3.0f / 2.0f, 0.0f, Mathf.Sqrt(3.0f) / 2.0f, Mathf.Sqrt(3.0f), 2.0f / 3.0f, 0.0f, -1.0f / 3.0f, Mathf.Sqrt(3.0f) / 3.0f, 0.0f);
    public static HexOrientation Ball = new(2f, 2 * Cos60, 0.0f, 2 * Sin60,
        Mathf.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f, 0.5f);
}

readonly struct HexLayout {
    public HexLayout(HexOrientation orientation, Vector2 size, Vector2 origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }
    public readonly HexOrientation orientation;
    public readonly Vector2 size;
    public readonly Vector2 origin;

    public Vector2 HexToPixel(Hex h)
    {
        var x = (orientation.qToX * h.q + orientation.rToX * h.r) * size.x;
        var y = (orientation.qToY * h.q + orientation.rToY * h.r) * size.y;
        return new Vector2(x + origin.x, y + origin.y);
    }


    public FractionalHex PixelToHex(Vector2 p)
    {
        var pt = new Vector2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        var q = orientation.xToQ * pt.x + orientation.yToQ * pt.y;
        var r = orientation.xToR * pt.x + orientation.yToR * pt.y;
        return new FractionalHex(q, r);
    }


    public Vector2 HexCornerOffset(int corner)
    {
        var M = orientation;
        var angle = 2.0f * Mathf.PI * (M.startAngle - corner) / 6.0f;
        return new Vector2(size.x * Mathf.Cos(angle), size.y * Mathf.Sin(angle));
    }


    public List<Vector2> PolygonCorners(Hex h)
    {
        var corners = new List<Vector2>{};
        var center = HexToPixel(h);
        for (int i = 0; i < 6; i++)
        {
            Vector2 offset = HexCornerOffset(i);
            corners.Add(new Vector2(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }
}

public class HexMap<T> {
    public readonly Dictionary<Hex, T> hexes = new();

    public HexMap() { }

    public void FillRectangle(Vector2Int topLeft, Vector2Int bottomRight, Func<Hex, T> f) {
        for(var r = topLeft.y; r <= bottomRight.y; r++) { // pointy top
            var rOffset = r >> 1; // (int)Math.Floor(r / 2.0)
            // var rOffset = (int)Math.Floor(r / 2.0); // or r>>1
            for(var q = topLeft.x - rOffset; q <= bottomRight.x - rOffset; q++) {
                // Debug.Log($"r: {r}, q: {q}");
                var hex = new Hex(q, r);
                Assert.IsFalse(hexes.ContainsKey(hex));
                hexes.Add(hex, f(hex));
            }
        }
    }
}