using System;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponLowering : ILateTickable
    {
        public event Action Lowered;
        public event Action Raised;

        private readonly WeaponConfig config;
        private readonly Transform transform;

        public bool IsLowered { get; private set; }

        /// <summary>Флаг: сейчас идёт анимация опускания</summary>
        public bool IsLowering { get; private set; }
        /// <summary>Флаг: сейчас идёт анимация подъёма</summary>
        public bool IsRaising { get; private set; }

        // база для жёсткой установки трансформа
        private readonly Vector3 originalLocalPosition;
        private readonly Quaternion originalLocalRotation;

        private float blend;
        private float blendVel;

        public WeaponLowering
            (
                WeaponConfig config,
                Transform transform
            )
        {
            this.config = config;
            this.transform = transform;
            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
            blend = IsLowered ? 1f : 0f;
            blendVel = 0f;
            IsLowering = false;
            IsRaising = false;
        }

        public void LateTick()
        {
            if (!transform.gameObject.activeInHierarchy)
                return;
            // 1) плавим blend → target
            var target = IsLowered ? 1f : 0f;
            blend = Mathf.SmoothDamp(blend, target, ref blendVel, config.WeaponAnimationSettings.loweredTransitionTime, Mathf.Infinity, Time.deltaTime);

            // 2) абсолютная установка трансформа
            var posOff = config.WeaponAnimationSettings.loweredPositionOffset * blend;
            var rotOff = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(config.WeaponAnimationSettings.loweredRotationEuler), blend);
            transform.localPosition = originalLocalPosition + posOff;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.localRotation = originalLocalRotation * rotOff;

            // 3) эвенты по окончании
            if (IsLowered && IsLowering && blend >= 0.99f)
            {
                IsLowering = false;
                Lowered?.Invoke();
            }
            else if (IsRaising && blend <= 0.01f)
            {
                IsRaising = false;
                Raised?.Invoke();
            }
        }

        /// <summary>
        /// Мгновенно ставит оружие в самое нижнее положение,
        /// сбрасывает lastState и запускает плавный подъём.
        /// </summary>
        public void ResetLowering()
        {
            blendVel = 0f;
            blend = 1f;
            IsLowered = false;

            // сразу помечаем как «начался подъём»
            IsRaising = true;
            IsLowering = false;

            transform.localPosition = originalLocalPosition + config.WeaponAnimationSettings.loweredPositionOffset;
            transform.localRotation = originalLocalRotation * Quaternion.Euler(config.WeaponAnimationSettings.loweredRotationEuler);
        }

        /// <summary>Запустить анимацию опускания.</summary>
        public void Lower()
        {
            IsLowered = true;
            IsLowering = true;
            IsRaising = false;
        }

        /// <summary>Запустить анимацию подъёма.</summary>
        public void Raise()
        {
            IsLowered = false;
            IsRaising = true;
            IsLowering = false;
        }
    }
}
