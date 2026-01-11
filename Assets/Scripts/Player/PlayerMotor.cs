using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerMotor : ITickable
    {
        private readonly CharacterController controller;
        private readonly PlayerGravity gravity;
        private readonly PlayerJump jump;
        private readonly PlayerMovement movement;

        private bool isSprint;
        private bool isCrouching;
        
        public PlayerMotor
            (
                CharacterController controller,
                PlayerGravity gravity,
                PlayerJump jump,
                PlayerMovement movement,
                ISubscriber<StartSprintMessage> startSprintMessageSubscriber,
                ISubscriber<CancelSprintMessage> cancelSprintMessageSubscriber
            )
        {
            this.controller = controller;
            this.gravity = gravity;
            this.jump = jump;
            this.movement = movement;

            startSprintMessageSubscriber.Subscribe(OnStartSprint);
            cancelSprintMessageSubscriber.Subscribe(OnCancelSprint);

        }

        public void Tick()
        {
            controller.Move(gravity.GetVelocity() 
                          + jump.GetVelocity() 
                          + movement.GetVelocity(isSprint));
        }

        private void OnStartSprint(StartSprintMessage msg)
        {
            isSprint = true;
        }
        
        private void OnCancelSprint(CancelSprintMessage msg)
        {
            isSprint = false;
        }
    }
}