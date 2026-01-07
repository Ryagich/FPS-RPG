using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Input
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InputHandler : IStartable
    {
        private readonly InputConfig inputConfig;
        private readonly IPublisher<PlayerMoveMessage> playerMovePublisher;
        private readonly IPublisher<LookDeltaMessage> lookDeltaMessagePublisher;

        private InputHandler
            (
                InputConfig inputConfig,
                IPublisher<PlayerMoveMessage> playerMovePublisher,
                IPublisher<LookDeltaMessage> lookDeltaMessagePublisher
            )
        {
            this.inputConfig = inputConfig;
            this.playerMovePublisher = playerMovePublisher;
            this.lookDeltaMessagePublisher = lookDeltaMessagePublisher;
        }

        public void Start()
        {
            inputConfig.MoveInput.action.performed += OnMove;
            inputConfig.MoveInput.action.canceled += OnMove;
           
            inputConfig.LookInput.action.performed += OnLookDelta;
            // inputConfig.LookInput.action.started += OnLookDelta;
            // inputConfig.LookInput.action.canceled += OnLookDelta;
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();
            playerMovePublisher.Publish(new PlayerMoveMessage(dir));
        }

        private void OnLookDelta(InputAction.CallbackContext context)
        {
            var delta = context.ReadValue<Vector2>();
            lookDeltaMessagePublisher.Publish(new LookDeltaMessage(delta));
        }
    }
}