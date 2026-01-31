using UnityEngine;

namespace Movement
{
    public interface IMovementDataProvider
    {
        public Vector3 Velocity { get; }
        public bool IsGrounded { get; }
        public Transform Transform { get; }
    }
}