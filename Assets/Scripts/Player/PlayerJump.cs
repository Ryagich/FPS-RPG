using Gravity;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerJump : ITickable
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
            Debug.Log($"OnJump isGrounded {controller.isGrounded}");
            if (!controller.isGrounded)
                return;

            verticalVelocity = Mathf.Sqrt(2f * gravityConfig.Gravity * playerMovementConfig.JumpHeight);
        }

        public void Tick()
        {
            verticalVelocity += -gravityConfig.Gravity * Time.deltaTime;

            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }
    }
}