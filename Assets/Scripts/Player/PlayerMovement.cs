using MessagePipe;
using Messages;
using UnityEngine;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerMovement
    {
        private readonly PlayerMovementConfig playerMovementConfig;
        private readonly Transform playerTransform;
        private readonly CharacterController characterController;

        private Vector2 direction;
        private Vector3 currentVelocity;

        public PlayerMovement
            (
                PlayerMovementConfig playerMovementConfig,
                Transform playerTransform,
                CharacterController characterController,
                ISubscriber<PlayerMoveMessage> playerMoveMessageSubscriber
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.playerTransform = playerTransform;
            this.characterController = characterController;

            playerMoveMessageSubscriber.Subscribe(OnMove);
        }

        public Vector3 GetVelocity(bool isSprint)
        {
            if (!characterController.isGrounded)
                return currentVelocity * Time.deltaTime;
            var canSprintForward = isSprint && direction.y > 0f;
            var currSpeed = canSprintForward ? playerMovementConfig.SprintSpeed : playerMovementConfig.WalkSpeed;
            var currentAccelerationRate = canSprintForward ? playerMovementConfig.SprintAccelerationRates : playerMovementConfig.WalkAccelerationRates;
            var input = Vector3.ClampMagnitude(playerTransform.forward * direction.y + playerTransform.right * direction.x, 1f);
            var targetVelocity = input * currSpeed;
            var accel = input.sqrMagnitude > 0.001f ? currentAccelerationRate.x : currentAccelerationRate.y;
            
            currentVelocity = Vector3.MoveTowards(currentVelocity,
                                                  targetVelocity,
                                                  accel * Time.deltaTime);
            return currentVelocity * Time.deltaTime;
        }
        
        private void OnMove(PlayerMoveMessage msg)
        {
            direction = msg.Direction;
        }
    }
}