using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Characters
{
    [CreateAssetMenu(fileName = "CharacterMovementConfig", menuName = "configs/Character/Movement")]
    [MovedFrom(true, "Player", null, "PlayerMovementConfig")]
    public sealed class CharacterMovementConfig : ScriptableObject
    {
        [field: Header("Movement Settings")]
        [field: SerializeField] public Vector3 WalkSpeed { get; private set; } = new(3.0f, 2.0f, 1.5f);
        [field: SerializeField] public float SprintSpeed { get; private set; } = 6.0f;
        [field: SerializeField] public Vector3 CrouchSpeed { get; private set; } = new(1.2f, .9f, .5f);
        [field: SerializeField] public Vector2 WalkAccelerationRates { get; private set; } = new(20.0f, 40.0f);
        [field: SerializeField] public Vector2 SprintAccelerationRates { get; private set; } = new(20.0f, 40.0f);
        [field: SerializeField] public Vector2 CrouchAccelerationRates { get; private set; } = new(20.0f, 40.0f);
        [field: Header("Crouch")]
        [field: SerializeField] public float CrouchingHeight { get; private set; } = .8f;
        [field: SerializeField] public float CrouchChangedSpeed { get; private set; } = 5f;
        [field: SerializeField] public float CameraPositionInCrouching { get; private set; } = .7f;
        [field: SerializeField] public LayerMask CrouchCheckMask { get; private set; }
        [field: Header("Jump")]
        [field: SerializeField] public float JumpHeight { get; private set; } = 1.2f;
        [field: Header("Camera Settings")]
        [field: SerializeField] public float Sensitivity { get; private set; } = 100.0f;
        [field: SerializeField] public Vector2 CameraPitchLimits { get; private set; } = new(-80f, 80f);
    }
}
