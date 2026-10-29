using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Input;
using UnityEngine;
using VContainer.Unity;

namespace Player
{
    using KINEMATION.FPSAnimationFramework.Runtime.Layers.IkMotionLayer;

    /// <summary>Передаёт плавные presentation-параметры в Animator, не влияя на физику.</summary>
    public sealed class PlayerAnimationDriver : ITickable
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private static readonly int Moving = Animator.StringToHash("Moving");
        private static readonly int InAir = Animator.StringToHash("InAir");
        private static readonly int JumpInitiated = Animator.StringToHash("JumpInitiated");
        private static readonly int Sprinting = Animator.StringToHash("Sprinting");
        private static readonly int FullBodyWeight = Animator.StringToHash("FullBodyWeight");

        private readonly Animator animator;
        private readonly FPSAnimator fpsAnimator;
        private readonly UserInputController kinemationInput;
        private readonly PlayerGrounding grounding;
        private readonly MoveStates states;
        private readonly PlayerMovementConfig movementConfig;

        // Это именно состояние анимации, отдельно от физической скорости PlayerMovement.
        private Vector2 animatorVelocity;
        private float sprintWeight;

        public PlayerAnimationDriver(PlayerAvatarView avatarView, PlayerGrounding grounding, MoveStates states,
                                     PlayerMovementConfig movementConfig)
        {
            animator = avatarView.Animator;
            fpsAnimator = animator.GetComponent<FPSAnimator>();
            kinemationInput = animator.GetComponent<UserInputController>();
            this.grounding = grounding;
            this.states = states;
            this.movementConfig = movementConfig;
        }

        public void Tick()
        {
            var targetVelocity = grounding.IsFalling ? Vector2.zero : states.MoveInput;
            var smoothing = GetAnimationSmoothing(targetVelocity);
            var alpha = 1f - Mathf.Exp(-smoothing * Time.deltaTime);

            // Повторяет FPSMovement из ассета: input мгновенный, AnimatorVelocity — плавный.
            animatorVelocity = Vector2.Lerp(animatorVelocity, targetVelocity, alpha);
            sprintWeight = Mathf.Lerp(sprintWeight, states.IsSprinting.Value ? 1f : 0f, alpha);

            animator.SetFloat(MoveX, animatorVelocity.x);
            animator.SetFloat(MoveY, animatorVelocity.y);
            animator.SetFloat(Velocity, animatorVelocity.magnitude);
            animator.SetBool(Moving, targetVelocity.sqrMagnitude > .001f);
            animator.SetBool(InAir, grounding.IsFalling);
            animator.SetBool(JumpInitiated, grounding.IsJumpInitiated);
            animator.SetFloat(Sprinting, sprintWeight);

            // Kinemation must receive the same filtered velocity that drives the locomotion blend tree.
            // This mirrors the framework's FPSMovement and keeps all presentation layers on one source.
            if (fpsAnimator.HasLinkedProfile)
            {
                kinemationInput.SetValue(FPSANames.MoveInput, new Vector4(animatorVelocity.x, animatorVelocity.y));
                // Same body/locomotion hand-off used by FPS_WA. This is not weapon-specific.
                kinemationInput.SetValue(FPSANames.PlayablesWeight, 1f - animator.GetFloat(FullBodyWeight));
            }
        }

        private float GetAnimationSmoothing(Vector2 targetVelocity)
        {
            if (states.IsProne.Value)
                return movementConfig.ProneAnimationSmoothing;

            if (states.IsCrouching.Value)
                return movementConfig.CrouchAnimationSmoothing;

            if (states.IsSprinting.Value)
                return movementConfig.SprintAnimationSmoothing;

            return targetVelocity.sqrMagnitude > .001f
                ? movementConfig.WalkAnimationSmoothing
                : movementConfig.IdleAnimationSmoothing;
        }
    }

    /// <summary>
    /// Coordinates Kinemation body transition layers with project-owned movement state.
    /// The source asset uses MonoBehaviour callbacks; this project keeps the same behavior in the player scope.
    /// </summary>
    public sealed class PlayerBodyAnimationController : ITickable
    {
        private readonly FPSAnimator fpsAnimator;
        private readonly UserInputController input;
        private readonly PlayerMovementConfig config;
        private readonly PlayerGrounding grounding;
        private readonly PlayerPoseController poseController;
        private readonly PlayerMovement movement;
        private readonly MoveStates states;

        private bool isInitialized;
        private bool wasCrouching;
        private bool wasFalling;
        private bool wasMoving;
        private int handledJumpStartedVersion;

        public PlayerBodyAnimationController(PlayerAvatarView avatarView, PlayerMovementConfig config,
                                             PlayerGrounding grounding, PlayerPoseController poseController,
                                             PlayerMovement movement, MoveStates states)
        {
            var animator = avatarView.Animator;
            fpsAnimator = animator.GetComponent<FPSAnimator>();
            input = animator.GetComponent<UserInputController>();
            this.config = config;
            this.grounding = grounding;
            this.poseController = poseController;
            this.movement = movement;
            this.states = states;
        }

        public void Tick()
        {
            if (!fpsAnimator.HasLinkedProfile)
                return;

            // The same resolved pose that owns the capsule drives Kinemation transitions.
            var isCrouching = poseController.CurrentPose == PlayerPose.Crouching;
            var isFalling = grounding.IsFalling;
            var isMoving = movement.IsMoving;
            var jumpStarted = handledJumpStartedVersion != grounding.JumpStartedVersion;
            handledJumpStartedVersion = grounding.JumpStartedVersion;

            if (isInitialized)
            {
                if (isCrouching != wasCrouching)
                    Link(config.CrouchMotion);

                // A confirmed jump starts its motion from the input event, while an unplanned
                // fall still starts it only after PlayerGrounding's fall delay has elapsed.
                if (jumpStarted || isFalling != wasFalling)
                    Link(config.JumpMotion);

                if (wasMoving && !isMoving)
                    Link(config.StopMotion);
            }
            else if (jumpStarted)
            {
                // Covers a jump received before this entry point's first Tick.
                Link(config.JumpMotion);
            }

            // Matches FPS_WA sprint callbacks while retaining project-owned sprint rules.
            input.SetValue(FPSANames.StabilizationWeight, states.IsSprinting.Value ? 0f : 1f);
            input.SetValue("LookLayerWeight", states.IsSprinting.Value ? .3f : 1f);

            wasCrouching = isCrouching;
            wasFalling = isFalling;
            wasMoving = isMoving;
            isInitialized = true;
        }

        private void Link(IkMotionLayerSettings motion)
        {
            if (motion != null)
                fpsAnimator.LinkAnimatorLayer(motion);
        }
    }
}
