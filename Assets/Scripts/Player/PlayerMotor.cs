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
        
        public PlayerMotor
            (
                CharacterController controller,
                PlayerGravity gravity,
                PlayerJump jump,
                PlayerMovement movement
            )
        {
            this.controller = controller;
            this.gravity = gravity;
            this.jump = jump;
            this.movement = movement;
        }

        public void Tick()
        {
            controller.Move(gravity.GetVelocity() 
                          + jump.GetVelocity() 
                          + movement.GetVelocity());
        }
    }
}