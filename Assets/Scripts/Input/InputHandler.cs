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
        private readonly IPublisher<JumpMessage> jumpMessagePublisher;

        private InputHandler
            (
                InputConfig inputConfig,
                IPublisher<PlayerMoveMessage> playerMovePublisher,
                IPublisher<LookDeltaMessage> lookDeltaMessagePublisher,
                IPublisher<JumpMessage> jumpMessagePublisher
            )
        {
            this.inputConfig = inputConfig;
            this.playerMovePublisher = playerMovePublisher;
            this.lookDeltaMessagePublisher = lookDeltaMessagePublisher;
            this.jumpMessagePublisher = jumpMessagePublisher;
        }

        public void Start()
        {
            inputConfig.MoveInput.action.performed += OnMove;
            inputConfig.MoveInput.action.canceled += OnMove;
           
            inputConfig.LookInput.action.performed += OnLookDelta;
            // inputConfig.LookInput.action.started += OnLookDelta;
            // inputConfig.LookInput.action.canceled += OnLookDelta;
            
            inputConfig.JumpInput.action.started += OnJump;

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
        
        private void OnJump(InputAction.CallbackContext context)
        {
            jumpMessagePublisher.Publish(new JumpMessage());
        }
    }
}