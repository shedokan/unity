using UnityEngine;

public class Layers {
    public static readonly int Edges = LayerMask.NameToLayer("Edges");
    public static readonly int Aimer = LayerMask.NameToLayer("Aimer");

    public static readonly int MovingBalls = LayerMask.NameToLayer("MovingBalls");
    public static readonly int IdleBalls = LayerMask.NameToLayer("IdleBalls");
    public static readonly int Falling = LayerMask.NameToLayer("Falling");

    public static readonly int RaycastLayers = (1 << Edges) | (1 << IdleBalls);
}