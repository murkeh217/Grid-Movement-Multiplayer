using UnityEngine;

public class PlayerModel
{
    public float MoveSpeed { get; set; }
    public float RayLength { get; set; }
    public float RayOffsetX { get; set; }
    public float RayOffsetY { get; set; }
    public float RayOffsetZ { get; set; }
    public LayerMask WalkableMask { get; set; }
    public LayerMask CollidableMask { get; set; }
    public float MaxFallCastDistance { get; set; }
    public float FallSpeed { get; set; }
    public bool Falling { get; set; }
    public float TargetFallHeight { get; set; }
    public bool Moving { get; set; }
    public Vector3 TargetPosition { get; set; }
    public Vector3 StartPosition { get; set; }
    public Transform CameraRotator { get; set; }
    public Vector3 XOffset { get; set; }
    public Vector3 YOffset { get; set; }
    public Vector3 ZOffset { get; set; }
    public Vector3 ZAxisOriginA { get; set; }
    public Vector3 ZAxisOriginB { get; set; }
    public Vector3 XAxisOriginA { get; set; }
    public Vector3 XAxisOriginB { get; set; }

    public PlayerModel()
    {
        // Set default values
        MoveSpeed = 12.07f;
        RayLength = 1.4f;
        RayOffsetX = 0.48f;
        RayOffsetY = 0.5f;
        RayOffsetZ = 0.48f;
        WalkableMask =  LayerMask.GetMask("Ground");
        CollidableMask =  LayerMask.GetMask("Default","Ground");
        MaxFallCastDistance = 100f;
        FallSpeed = 30f;
        Falling = false;
        TargetFallHeight = 0f;
        Moving = false;
        TargetPosition = Vector3.zero;
        StartPosition = Vector3.zero;
        CameraRotator = null;
        XOffset = Vector3.zero;
        YOffset = Vector3.zero;
        ZOffset = Vector3.zero;
        ZAxisOriginA = Vector3.zero;
        ZAxisOriginB = Vector3.zero;
        XAxisOriginA = Vector3.zero;
        XAxisOriginB = Vector3.zero;
    }
}

