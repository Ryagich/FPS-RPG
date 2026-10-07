using System;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponSway : ILateTickable, IDisposable
    {
        private Vector2 lookDelta;
        private readonly float sensitivity;
        
        private Vector3 posSwayVelocity;
        private Vector3 rotSwayVelocity;
        private Vector3 currentPosOffset;
        private Vector3 currentRotOffset;
        
        private readonly IDisposable subscriptions;
        private readonly WeaponConfig config;
        private readonly Transform transform;
        private SwaySettings currentSettings;

        public WeaponSway
            (
                WeaponConfig config,
                Characters.CharacterMovementConfig movementConfig,
                Transform transform,
                ISubscriber<LookCommand> lookDeltaMessageSubscriber,
                ISubscriber<AimChangedMessage> aimChangedMessageSubscriber
            )
        {
            this.config = config;
            sensitivity = Mathf.Max(0.001f, movementConfig.Sensitivity);
            this.transform = transform;

            SetCurrentSettings(false);
                
            var bag = DisposableBag.CreateBuilder();
            lookDeltaMessageSubscriber.Subscribe(OnLookDeltaChanged).AddTo(bag);
            aimChangedMessageSubscriber.Subscribe(SetCurrentSettings).AddTo(bag);
            subscriptions = bag.Build();
        }

        private void OnLookDeltaChanged(LookCommand msg)
        {
            lookDelta += msg.Delta / sensitivity;
        }
        
        public void LateTick()
        {
            if (!transform.gameObject.activeInHierarchy)
                return;
            // 1) Берём дельту мыши
            var mouseX = lookDelta.x;
            var mouseY = lookDelta.y;
            lookDelta = Vector2.zero;
            
            // 2) Вычисляем целевое позиционное смещение (инвертируем X/Y для "против сваев")
            var targetPosOffset = new Vector3(
                                                  -mouseX * currentSettings.positionMultiplier,
                                                  -mouseY * currentSettings.positionMultiplier,
                                                  0f
                                                 );
            // Ограничиваем
            targetPosOffset.x = Mathf.Clamp(targetPosOffset.x, -currentSettings.maxPositionOffset, currentSettings.maxPositionOffset);
            targetPosOffset.y = Mathf.Clamp(targetPosOffset.y, -currentSettings.maxPositionOffset, currentSettings.maxPositionOffset);
            
            // Плавно приближаем текущее смещение к целевому
            currentPosOffset = Vector3.SmoothDamp(
                                                  currentPosOffset,
                                                  targetPosOffset,
                                                  ref posSwayVelocity,
                                                  1f / currentSettings.positionSmooth
                                                 );
            
            // 3) Вычисляем целевое вращательное смещение
            var targetRotOffset = new Vector3(
                                                  mouseY * currentSettings.rotationMultiplier,        // наклон вверх/вниз
                                                  -mouseX * currentSettings.rotationMultiplier,       // поворот влево/вправо
                                                  -mouseX * currentSettings.rotationMultiplier * 0.5f // небольшой ролевой эффект
                                                 );
            // Ограничиваем
            targetRotOffset.x = Mathf.Clamp(targetRotOffset.x, -currentSettings.maxRotationOffset, currentSettings.maxRotationOffset);
            targetRotOffset.y = Mathf.Clamp(targetRotOffset.y, -currentSettings.maxRotationOffset, currentSettings.maxRotationOffset);
            targetRotOffset.z = Mathf.Clamp(targetRotOffset.z, -currentSettings.maxRotationOffset, currentSettings.maxRotationOffset);
            
            // Плавно приближаем текущее вращение к целевому
            currentRotOffset = Vector3.SmoothDamp(
                                                  currentRotOffset,
                                                  targetRotOffset,
                                                  ref rotSwayVelocity,
                                                  1f / currentSettings.rotationSmooth
                                                 );
            
            // 4) Добавляем смещения к тому, что уже задала анимация
            transform.localPosition += currentPosOffset;
            transform.localRotation *= Quaternion.Euler(currentRotOffset);
        }
        
        public void Dispose() => subscriptions.Dispose();

        private void SetCurrentSettings(AimChangedMessage msg)
        {
            SetCurrentSettings(msg.IsAiming);
        }
        
        private void SetCurrentSettings(bool isAim)
        {
            currentSettings = isAim
                            ? config.WeaponAnimationSettings.AimSwaySettings
                            : config.WeaponAnimationSettings.SwaySettings;
        }
    }
}
