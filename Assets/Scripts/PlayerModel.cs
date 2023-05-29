using UnityEngine;
public class PlayerModel
{
    // Movement parameters
    public float MoveSpeed { get; set; }       // Speed at which the player moves
    public float RayLength { get; set; }       // Length of the raycast used for ground detection
    public float RayOffsetX { get; set; }      // X-axis offset of the raycast origin
    public float RayOffsetY { get; set; }      // Y-axis offset of the raycast origin
    public float RayOffsetZ { get; set; }      // Z-axis offset of the raycast origin

    // Layer masks for collision detection
    public LayerMask WalkableMask { get; set; }        // Layer mask for walkable surfaces
    public LayerMask CollidableMask { get; set; }      // Layer mask for collidable surfaces

    // Fall parameters
    public float MaxFallCastDistance { get; set; }     // Maximum distance to cast the fall raycast
    public float FallSpeed { get; set; }               // Speed at which the player falls
    public bool Falling { get; set; }                  // Flag indicating if the player is currently falling
    public float TargetFallHeight { get; set; }        // Target height for falling

    // Movement flags and positions
    public bool Moving { get; set; }                   // Flag indicating if the player is currently moving
    public Vector3 TargetPosition { get; set; }        // Target position for movement
    public Vector3 StartPosition { get; set; }         // Starting position of the player

    // Offsets and origins for raycasts
    public Vector3 XOffset { get; set; }               // Offset in the X-axis direction
    public Vector3 YOffset { get; set; }               // Offset in the Y-axis direction
    public Vector3 ZOffset { get; set; }               // Offset in the Z-axis direction

    public string Name { get; set; }
    public PlayerModel()
    {
        // Set default values

        // Movement parameters
        MoveSpeed = 12.07f;
        RayLength = 1.4f;
        RayOffsetX = 0.48f;
        RayOffsetY = 0.5f;
        RayOffsetZ = 0.48f;

        // Layer masks
        WalkableMask = LayerMask.GetMask("Ground");
        CollidableMask = LayerMask.GetMask("Default", "Ground");

        // Fall parameters
        MaxFallCastDistance = 100f;
        FallSpeed = 30f;
        Falling = false;
        TargetFallHeight = 0f;

        // Movement flags and positions
        Moving = false;
        TargetPosition = Vector3.zero;
        StartPosition = Vector3.zero;

        // Offsets and origins
        XOffset = Vector3.zero;
        YOffset = Vector3.zero;
        ZOffset = Vector3.zero;

        Name = "";
    }
}
