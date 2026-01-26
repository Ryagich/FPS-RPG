using Gravity;
using MessagePipe;
using Messages;
using UnityEngine;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerJump
    {
        private readonly PlayerMovementConfig playerMovementConfig;
        private readonly GravityConfig gravityConfig;
        private readonly CharacterController controller;

        private float verticalVelocity;

        public PlayerJump
            (
                PlayerMovementConfig playerMovementConfig,
                GravityConfig gravityConfig,
                CharacterController controller,
                ISubscriber<JumpMessage> jumpSubscriber
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.gravityConfig = gravityConfig;
            this.controller = controller;

            jumpSubscriber.Subscribe(OnJump);
        }

        private void OnJump(JumpMessage msg)
        {
            if (!controller.isGrounded)
                return;

            verticalVelocity = Mathf.Sqrt(2f * gravityConfig.Gravity * playerMovementConfig.JumpHeight);
        }

        public Vector3 GetVelocity()
        {
            var velocity = Vector3.up * verticalVelocity * Time.deltaTime;
            verticalVelocity = Mathf.Clamp(verticalVelocity + -gravityConfig.Gravity * Time.deltaTime, .0f, int.MaxValue);
            return velocity;
        }
    }
}