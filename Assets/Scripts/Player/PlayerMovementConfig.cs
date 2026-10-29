using UnityEngine;

namespace Player
{
    using KINEMATION.FPSAnimationFramework.Runtime.Layers.IkMotionLayer;

    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "configs/Player/PlayerMovement")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [field: Header("Movement Settings")]
        [field: SerializeField] public Vector3 WalkSpeed { get; private set; } = new (3.0f, 2.0f, 1.5f);
        [field: SerializeField] public float SprintSpeed { get; private set; } = 6.0f;
        [field: SerializeField] public Vector3 CrouchSpeed { get; private set; } = new ( 1.2f, .9f, .5f);
        [field: SerializeField] public Vector2 WalkAccelerationRates { get; private set; } = new (20.0f, 40.0f);
        [field: SerializeField] public Vector2 SprintAccelerationRates { get; private set; } = new (20.0f, 40.0f);
        [field: SerializeField] public Vector2 CrouchAccelerationRates { get; private set; } = new (20.0f, 40.0f);
        [field: Header("Animation Smoothing")]
        [field: SerializeField] public float IdleAnimationSmoothing { get; private set; } = 6.5f;
        [field: SerializeField] public float WalkAnimationSmoothing { get; private set; } = 5.0f;
        [field: SerializeField] public float SprintAnimationSmoothing { get; private set; } = 3.0f;
        [field: SerializeField] public float CrouchAnimationSmoothing { get; private set; } = 4.0f;
        [field: SerializeField] public float ProneAnimationSmoothing { get; private set; } = 2.0f;
        [field: Header("Crouch")]
        [field: SerializeField] public float CrouchingHeight { get; private set; } = .8f;
        [field: SerializeField] public float CrouchChangedSpeed { get; private set; } = 5f;
        [field: SerializeField] public float CameraPositionInCrouching { get; private set; } = .7f;
        [field: SerializeField] public LayerMask CrouchCheckMask { get; private set; }
        [field: Header("Prone")]
        [field: SerializeField] public Vector3 ProneSpeed { get; private set; } = new (.8f, .6f, .4f);
        [field: SerializeField] public float ProneHeight { get; private set; } = .45f;
        [field: SerializeField] public float CameraPositionInProne { get; private set; } = .38f;
        [field: SerializeField] public float ProneChangedSpeed { get; private set; } = 3f;
        [field: Header("Lean")]
        [field: SerializeField, Range(0f, 90f)] public float LeanAngle { get; private set; } = 25f;
        [field: SerializeField] public IkMotionLayerSettings LeanMotion { get; private set; }
        [field: Header("Body Transition Motions")]
        [field: SerializeField] public IkMotionLayerSettings CrouchMotion { get; private set; }
        [field: SerializeField] public IkMotionLayerSettings JumpMotion { get; private set; }
        [field: SerializeField] public IkMotionLayerSettings StopMotion { get; private set; }
        [field: Header("Jump")]
        [field: SerializeField] public float JumpHeight { get; private set; } = 1.2f;
        [field: Header("Jump Capsule")]
        [field: SerializeField, Min(0.1f)] public float AirborneCapsuleHeight { get; private set; } = 1.25f;
        [field: SerializeField, Min(0.01f)] public float AirborneCapsuleChangeSpeed { get; private set; } = 4f;
        [field: SerializeField] public Vector3 AirborneCapsuleCenter { get; private set; } = new(0f, .705f, 0f);
        [field: SerializeField, Min(0.01f)] public float AirborneCapsuleCenterChangeSpeed { get; private set; } = 2f;
        [field: SerializeField, Min(0.01f)] public float LandingCapsuleRecoverySpeed { get; private set; } = .65f;
        [field: SerializeField, Min(0.01f)] public float LandingCapsuleCenterRecoverySpeed { get; private set; } = .35f;
        [field: Header("Grounding")]
        [field: SerializeField] public LayerMask GroundCheckMask { get; private set; } = ~(1 << 6);
        [field: SerializeField, Min(0f)] public float GroundProbeDistance { get; private set; } = .1f;
        [field: SerializeField, Range(.1f, 1f)] public float GroundProbeRadiusScale { get; private set; } = .9f;
        [field: SerializeField, Min(0f)] public float FallDelay { get; private set; } = .12f;
        [field: Header("Camera Settings")]
        [field: SerializeField] public float Sensitivity { get; private set; } = 100.0f;
        [field: SerializeField] public Vector2 cameraYClamp { get; private set; } = new(-80.0f,80.0f);
    }
}
