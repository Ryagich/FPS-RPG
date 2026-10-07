using System;
using MessagePipe;
using Messages;
using Movement;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponJumpBobbing : ILateTickable, IDisposable
    {
        private readonly Transform transform;
        private readonly IMovementDataProvider movement;
        private readonly IDisposable subscriptions;
        private readonly WeaponConfig config;

        private float jumpBlend;
        private float jumpBlendVel;
        private float fallBlend;
        private float fallBlendVel;

        private JumpBobbingSettings currentSettings;

        public WeaponJumpBobbing
            (
                WeaponConfig config,
                IMovementDataProvider movement,
                Transform transform,
                ISubscriber<AimChangedMessage> aimChangedMessageSubscriber
            )
        {
            this.config = config;
            this.movement = movement;
            this.transform = transform;

            SetCurrentSettings(false);

            subscriptions = aimChangedMessageSubscriber.Subscribe(SetCurrentSettings);
        }

        public void LateTick()
        {
            if (!transform.gameObject.activeInHierarchy)
                return;
            // 1) Вертикальная скорость (универсально)
            var velY = movement.Velocity.y;

            // 2) Целевые значения blend
            var targetJump = velY > currentSettings.JumpThreshold ? 1f : 0f;
            var targetFall = velY < -currentSettings.FallThreshold ? 1f : 0f;

            // 3) Плавные переходы
            jumpBlend = Mathf.SmoothDamp(
                jumpBlend,
                targetJump,
                ref jumpBlendVel,
                currentSettings.JumpTransitionTime
            );

            fallBlend = Mathf.SmoothDamp(
                fallBlend,
                targetFall,
                ref fallBlendVel,
                currentSettings.FallTransitionTime
            );

            // 4) Позиционные оффсеты
            var jumpPosOff = currentSettings.JumpPositionOffset * jumpBlend;
            var fallPosOff = currentSettings.FallPositionOffset * fallBlend;

            // 5) Ротационные оффсеты
            var jumpRotOff = Quaternion.Slerp(
                Quaternion.identity,
                Quaternion.Euler(currentSettings.JumpRotationEuler),
                jumpBlend
            );

            var fallRotOff = Quaternion.Slerp(
                Quaternion.identity,
                Quaternion.Euler(currentSettings.FallRotationEuler),
                fallBlend
            );

            // 6) Аддитивное применение
            transform.localPosition += jumpPosOff + fallPosOff;
            transform.localRotation *= jumpRotOff * fallRotOff;
        }
        
        public void Dispose() => subscriptions.Dispose();

        private void SetCurrentSettings(AimChangedMessage msg)
        {
            SetCurrentSettings(msg.IsAiming);
        }
        
        private void SetCurrentSettings(bool isAim)
        {
            currentSettings = isAim
                ? config.WeaponAnimationSettings.AimJumpBobbingSettings
                : config.WeaponAnimationSettings.JumpBobbingSettings;
        }
    }
}
