using System;
using Dependencies;
using MessagePipe;
using Messages;
using UnityEngine;

namespace Characters
{
    public sealed class CharacterLook : IDisposable
    {
        private readonly Transform body;
        private readonly Transform aim;
        private readonly CharacterMovementConfig config;
        private readonly CharacterState state;
        private readonly IDisposable subscription;
        private Vector2 pendingDelta;

        public CharacterLook(Transform body, AimTransform aimReference,
            CharacterMovementConfig config, CharacterState state, ISubscriber<LookCommand> look)
        {
            var aim = aimReference.Value;
            this.body = body;
            this.aim = aim;
            this.config = config;
            this.state = state;
            subscription = look.Subscribe(command => pendingDelta += command.Delta);
        }

        public void ApplyPending(float deltaTime)
        {
            state.YawRate = pendingDelta.x / deltaTime;
            Rotate(pendingDelta);
            pendingDelta = Vector2.zero;
        }

        public void Rotate(Vector2 delta)
        {
            state.Pitch = Mathf.Clamp(state.Pitch - delta.y, config.CameraPitchLimits.x, config.CameraPitchLimits.y);
            body.Rotate(Vector3.up, delta.x, Space.World);
            // Preserve the roll applied by camera shake.
            aim.localRotation = Quaternion.Euler(state.Pitch, 0f, aim.localEulerAngles.z);
        }

        public void Dispose() => subscription.Dispose();
    }
}
