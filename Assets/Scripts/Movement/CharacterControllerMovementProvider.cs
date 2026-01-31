using UnityEngine;

namespace Movement
{
    public sealed class CharacterControllerMovementProvider : IMovementDataProvider
    {
        private readonly CharacterController controller;
        private readonly Transform transform;

        public CharacterControllerMovementProvider(
                CharacterController controller,
                Transform transform
            )
        {
            this.controller = controller;
            this.transform = transform;
        }

        public Vector3 Velocity => controller.velocity;
        public bool IsGrounded => controller.isGrounded;
        public Transform Transform => transform;
    }
}