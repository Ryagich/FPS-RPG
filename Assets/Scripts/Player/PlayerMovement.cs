using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerMovement : ITickable
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

        public void Tick()
        {
            var input = Vector3.ClampMagnitude(playerTransform.forward * direction.y + playerTransform.right * direction.x, 1f);
            var targetVelocity = input * playerMovementConfig.WalkSpeed;
            var accel = input.sqrMagnitude > 0.001f
                            ? playerMovementConfig.Acceleration
                            : playerMovementConfig.Deceleration;
            currentVelocity = Vector3.MoveTowards(currentVelocity,
                                                  targetVelocity,
                                                  accel * Time.deltaTime);

            characterController.Move(currentVelocity * Time.deltaTime);
        }

        private void OnMove(PlayerMoveMessage msg)
        {
            direction = msg.Direction;
        }
    }
}