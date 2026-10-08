using Gravity;
using UnityEngine;
using VContainer.Unity;

namespace Characters
{
    public sealed class CharacterMotor : ITickable
    {
        private readonly CharacterController controller;
        private readonly CharacterMovement movement;
        private readonly CharacterLook look;
        private readonly CharacterState state;
        private readonly CharacterMovementConfig config;
        private readonly GravityConfig gravity;
        private readonly CharacterMovementCommands commands;
        private float verticalVelocity;

        public CharacterMotor(CharacterController controller, CharacterMovement movement,
            CharacterLook look, CharacterState state, CharacterMovementConfig config,
            GravityConfig gravity, CharacterMovementCommands commands)
        {
            this.controller = controller;
            this.movement = movement;
            this.look = look;
            this.state = state;
            this.config = config;
            this.gravity = gravity;
            this.commands = commands;
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f || !controller.enabled)
                return;
            look.ApplyPending(deltaTime);
            var horizontalVelocity = movement.GetVelocity(deltaTime);
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            if (commands.ConsumeJump() && controller.isGrounded && !state.IsCrouching.Value
                && state.CrouchProgress <= 0f)
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Max(0f, gravity.Gravity * config.JumpHeight));
            verticalVelocity -= gravity.Gravity * deltaTime;
            var collisions = controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime);
            if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
            state.Velocity = controller.velocity;
            state.LocalVelocity = controller.transform.InverseTransformDirection(state.Velocity);
            state.IsGrounded = controller.isGrounded;
        }
    }
}
