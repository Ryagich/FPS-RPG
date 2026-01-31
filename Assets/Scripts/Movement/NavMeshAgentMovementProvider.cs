using UnityEngine;
using UnityEngine.AI;

namespace Movement
{
    public sealed class NavMeshAgentMovementProvider : IMovementDataProvider
    {
        private readonly NavMeshAgent agent;
        private readonly Transform transform;

        public NavMeshAgentMovementProvider(
                NavMeshAgent agent,
                Transform transform
            )
        {
            this.agent = agent;
            this.transform = transform;
        }

        public Vector3 Velocity => agent.velocity;
        public bool IsGrounded => agent.isOnNavMesh; // или своя логика
        public Transform Transform => transform;
    }
}