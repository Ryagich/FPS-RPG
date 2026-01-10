using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "configs/Player/PlayerMovement")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [field: Header("Movement Settings")]
        [field: SerializeField] public float WalkSpeed { get; private set; } = 3.0f;
        [field: SerializeField] public float SprintSpeed { get; private set; } = 6.0f;
        [field: SerializeField] public float Acceleration { get; private set; } = 20.0f;
        [field: SerializeField] public float Deceleration { get; private set; } = 40.0f;
        [field: Header("Crouch")]
        [field: SerializeField] public float CrouchSpeed { get; private set; } = 1.5f;
        [field: SerializeField] public float StandingHeight { get; private set; } = 1.8f;
        [field: SerializeField] public float CrouchingHeight { get; private set; } = 0.8f;
        [field: SerializeField] public LayerMask CrouchCheckMask { get; private set; }
        [field: Header("Jump")]
        [field: SerializeField] public float JumpHeight { get; private set; } = 1.2f;
        [field: Header("Camera Settings")]
        [field: SerializeField] public float Sensitivity { get; private set; } = 100.0f;
        [field: SerializeField] public Vector2 cameraYClamp { get; private set; } = new(-80.0f,80.0f);
    }
}