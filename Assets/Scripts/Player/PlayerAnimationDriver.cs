using UnityEngine;
using VContainer.Unity;

namespace Player
{
    /// <summary>Передаёт плавные presentation-параметры в Animator, не влияя на физику.</summary>
    public sealed class PlayerAnimationDriver : ITickable
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private static readonly int Moving = Animator.StringToHash("Moving");
        private static readonly int InAir = Animator.StringToHash("InAir");
        private static readonly int Sprinting = Animator.StringToHash("Sprinting");

        private readonly Animator animator;
        private readonly CharacterController controller;
        private readonly MoveStates states;
        private readonly PlayerMovementConfig movementConfig;

        // Это именно состояние анимации, отдельно от физической скорости PlayerMovement.
        private Vector2 animatorVelocity;
        private float sprintWeight;

        public PlayerAnimationDriver(PlayerAvatarView avatarView, CharacterController controller, MoveStates states,
                                     PlayerMovementConfig movementConfig)
        {
            animator = avatarView.Animator;
            this.controller = controller;
            this.states = states;
            this.movementConfig = movementConfig;
        }

        public void Tick()
        {
            var targetVelocity = controller.isGrounded ? states.MoveInput : Vector2.zero;
            var smoothing = GetAnimationSmoothing(targetVelocity);
            var alpha = 1f - Mathf.Exp(-smoothing * Time.deltaTime);

            // Повторяет FPSMovement из ассета: input мгновенный, AnimatorVelocity — плавный.
            animatorVelocity = Vector2.Lerp(animatorVelocity, targetVelocity, alpha);
            sprintWeight = Mathf.Lerp(sprintWeight, states.IsSprinting.Value ? 1f : 0f, alpha);

            animator.SetFloat(MoveX, animatorVelocity.x);
            animator.SetFloat(MoveY, animatorVelocity.y);
            animator.SetFloat(Velocity, animatorVelocity.magnitude);
            animator.SetBool(Moving, targetVelocity.sqrMagnitude > .001f);
            animator.SetBool(InAir, !controller.isGrounded);
            animator.SetFloat(Sprinting, sprintWeight);
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
}
