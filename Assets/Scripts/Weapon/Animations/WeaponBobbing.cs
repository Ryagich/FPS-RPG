using System;
using MessagePipe;
using Messages;
using Movement;
using UnityEngine;
using VContainer.Unity;
using Weapon.Attachments;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponBobbing : ILateTickable, IDisposable
    {
        public ScopesSettings ScopeSettings { get; set; }
        
        private readonly IDisposable subscriptions;
        private readonly WeaponConfig config;
        private readonly Transform transform;
        private readonly IMovementDataProvider movement;
        
        private bool isAiming;

        // внутренние для блендов и фаз
        private float aimBlend;
        private float aimBlendVelocity;
        private float walkSpeedNorm;
        private float walkSpeedVel;
        private float forwardAmtSmooth;
        private float forwardAmtVel;
        private float rightAmtSmooth;
        private float rightAmtVel;
        private float bobPhase;
        private BobbingSettings currentSettings;
        
        public WeaponBobbing
            (
                WeaponConfig config,
                Transform transform,
                IMovementDataProvider movement,
                ISubscriber<AimChangedMessage> aimChangedMessageSubscriber
            )
        {
            this.config = config;
            this.transform = transform;
            this.movement = movement;

            SetCurrentSettings(false);
            
            subscriptions = aimChangedMessageSubscriber.Subscribe(SetCurrentSettings);
        }
        
        public void LateTick()
        {
            if (!transform.gameObject.activeInHierarchy)
                return;
            var t = Time.time;

            // 1) Блендим прицел
            var targetAim = isAiming ? 1f : 0f;
            aimBlend = Mathf.SmoothDamp(aimBlend, targetAim, ref aimBlendVelocity, config.WeaponAnimationSettings.aimBobbingTransitionTime);
            var aimPosOff = ScopeSettings is null ? Vector3.zero : (ScopeSettings.AimPosition * aimBlend);
            var aimRotOff = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(ScopeSettings?.aimRotationEuler ?? Vector3.zero), aimBlend);

            // 2) Idle–bobbing
            var breatheOff = Mathf.Sin(2 * Mathf.PI * currentSettings.BreatheFrequency * t) * currentSettings.BreatheAmplitude;
            var heartOff   = Mathf.Sin(2 * Mathf.PI * currentSettings.HeartFrequency   * t) * currentSettings.HeartAmplitude;
            var idlePosOff = Vector3.up * (breatheOff + heartOff);
            var idleRotOff = Quaternion.Euler(
                Mathf.Sin(2 * Mathf.PI * currentSettings.BreatheFrequency * t) * currentSettings.HeartRotationAngle,  // наклон по X
                0f,
                Mathf.Sin(2 * Mathf.PI * currentSettings.HeartFrequency   * t) * currentSettings.HeartRotationAngle   // крен по Z
            );

            // 3) Walk intensity
            var velocity = movement.Velocity;
            var speed = new Vector2(velocity.x, velocity.z).magnitude;

            // var speed = movement.velocity.magnitude;
            var targetNorm = movement.IsGrounded && currentSettings.MaxWalkSpeed > 0.001f
                ? Mathf.Clamp01(speed / currentSettings.MaxWalkSpeed) : 0f;
            walkSpeedNorm = Mathf.SmoothDamp(walkSpeedNorm, targetNorm, ref walkSpeedVel, currentSettings.WalkSmoothTime);

            // 4) Направления
            var velDir = speed > 0.001f ? new Vector3(velocity.x, 0f, velocity.z).normalized : Vector3.zero;
            // var velDir = characterController.velocity.normalized;
            var rawF = Vector3.Dot(movement.Transform.forward, velDir);
            var rawR = Vector3.Dot(movement.Transform.right, velDir);
            forwardAmtSmooth = Mathf.SmoothDamp(forwardAmtSmooth, rawF, ref forwardAmtVel, currentSettings.WalkSmoothTime);
            rightAmtSmooth   = Mathf.SmoothDamp(rightAmtSmooth,   rawR, ref rightAmtVel, currentSettings.WalkSmoothTime);

            // 5) Фаза
            bobPhase += Time.deltaTime * currentSettings.WalkFrequency;

            // 6) Walk–bobbing и страйф-наклон
            var walkPosOff = Vector3.zero;
            var walkRotOff = Quaternion.identity;
            if (walkSpeedNorm > 0.001f)
            {
                var phase = bobPhase * 2 * Mathf.PI;
                walkPosOff = new Vector3(
                    Mathf.Sin(phase) * currentSettings.WalkHorizontalAmplitude * walkSpeedNorm * rightAmtSmooth,
                    Mathf.Cos(phase) * currentSettings.WalkVerticalAmplitude   * walkSpeedNorm * Mathf.Abs(forwardAmtSmooth),
                    0f
                );
                walkRotOff = Quaternion.Euler(
                    -forwardAmtSmooth * currentSettings.StrafeTiltAngle * walkSpeedNorm,  // тангаж при ходьбе
                    0f,
                    -rightAmtSmooth * currentSettings.StrafeTiltAngle * walkSpeedNorm   // крен при страйфинге
                );
            }

            // 7) Итоговые оффсеты
            var totalPosOff = aimPosOff + idlePosOff + walkPosOff;
            var totalRotOff = aimRotOff * idleRotOff * walkRotOff;

            // усиливаем вращение
            totalRotOff = Quaternion.Slerp(Quaternion.identity, totalRotOff, currentSettings.RotationInfluence);

            // 8) Накладываем на уже установленный WeaponLowering трансформ
            var trans = transform;
            trans.localPosition += totalPosOff;
            trans.localRotation *= totalRotOff;
        }
   
        // public void StartAim() => isAim = true;
        // public void StopAim() => isAim = false;
        
        public void Dispose() => subscriptions.Dispose();

        private void SetCurrentSettings(AimChangedMessage msg)
        {
            SetCurrentSettings(msg.IsAiming);
        }
        
        private void SetCurrentSettings(bool value)
        {
            isAiming = value;
            currentSettings = value
                            ? config.WeaponAnimationSettings.AimBobbingSettings
                            : config.WeaponAnimationSettings.BobbingSettings;
            currentSettings = currentSettings.IsDev ? new BobbingSettings() : currentSettings;
        }
    }
}
