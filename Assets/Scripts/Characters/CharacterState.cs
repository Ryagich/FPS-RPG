using System;
using UniRx;
using UnityEngine;

namespace Characters
{
    public sealed class CharacterState : IDisposable
    {
        public Vector2 MoveRequest { get; internal set; }
        public bool SprintRequested { get; internal set; }
        public bool CrouchRequested { get; internal set; }
        public bool JumpRequested { get; internal set; }
        public bool IsAiming { get; internal set; }
        public bool IsGrounded { get; internal set; }
        public Vector3 Velocity { get; internal set; }
        public Vector3 LocalVelocity { get; internal set; }
        public float MaxSpeed { get; internal set; }
        public float Pitch { get; internal set; }
        public float YawRate { get; internal set; }
        private readonly ReactiveProperty<bool> isSprinting = new();
        private readonly ReactiveProperty<bool> isCrouching = new();
        public IReadOnlyReactiveProperty<bool> IsSprinting => isSprinting;
        public IReadOnlyReactiveProperty<bool> IsCrouching => isCrouching;
        internal void SetSprinting(bool value) => isSprinting.Value = value;
        internal void SetCrouching(bool value) => isCrouching.Value = value;
        public float HorizontalSpeed => new Vector2(Velocity.x, Velocity.z).magnitude;

        public void Dispose()
        {
            isSprinting.Dispose();
            isCrouching.Dispose();
        }
    }
}
