using UnityEngine;

namespace Movement
{
    public sealed class CharacterControllerMovementProvider : IMovementDataProvider
    {
        private readonly CharacterController controller;
        private readonly Transform transform;
        private readonly Player.PlayerGrounding grounding;

        public CharacterControllerMovementProvider(
                CharacterController controller,
                Transform transform,
                Player.PlayerGrounding grounding
            )
        {
            this.controller = controller;
            this.transform = transform;
            this.grounding = grounding;
        }

        public Vector3 Velocity => controller.velocity;
        public bool IsGrounded => grounding.HasGroundSupport;
        public Transform Transform => transform;
    }
}
