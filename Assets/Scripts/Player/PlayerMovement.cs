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
        private readonly CharacterController controller;

        private Vector2 direction;

        public PlayerMovement
            (
                PlayerMovementConfig playerMovementConfig,
                Transform playerTransform,
                CharacterController controller,
                ISubscriber<PlayerMoveMessage> playerMoveMessageSubscriber
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.playerTransform = playerTransform;
            this.controller = controller;

            playerMoveMessageSubscriber.Subscribe(OnMove);
        }
        
        public void Tick()
        {
            if (direction is { x: 0, y: 0 })
            {
                return;
            }
            var move = Vector3.ClampMagnitude(playerTransform.forward * direction.y + playerTransform.right   * direction.x, 1f);
            controller.Move(move * playerMovementConfig.WalkSpeed * Time.deltaTime);
        }

        private void OnMove(PlayerMoveMessage msg)
        {
            direction = msg.Direction;
        }
    }
}