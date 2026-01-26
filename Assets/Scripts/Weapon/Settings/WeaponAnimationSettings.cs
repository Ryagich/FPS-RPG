using System;
using UnityEngine;

namespace Weapon.Settings
{
    [Serializable]
    public class WeaponAnimationSettings
    {
        [field: Header("=====Lowering=====")]
        [field: Tooltip("Смещение позиции при опускании")]
        [field: SerializeField] public Vector3 loweredPositionOffset { get; private set; } = new(.0f, -0.4f, .0f);
        [field: Tooltip("Углы Эйлера при опускании")]
        [field: SerializeField] public Vector3 loweredRotationEuler { get; private set; } = new(30.0f, .0f, .0f);
        [field: Tooltip("Время перехода")]
        [field: SerializeField] public float loweredTransitionTime { get; private set; } = .2f;
        [field: SerializeField] public KickBackSettings KickBackSettings { get; private set; } = null!;
        [field: SerializeField] public KickBackSettings AimKickBackSettings { get; private set; } = null!;
        [field: SerializeField] public SwaySettings SwaySettings { get; private set; } = null!;
        [field: SerializeField] public SwaySettings AimSwaySettings { get; private set; } = null!;
        [field: Tooltip("Время плавного перехода в/из прицеливания")]
        [field: SerializeField] public float aimBobbingTransitionTime { get; private set; } = .1f;
        [field: SerializeField] public BobbingSettings BobbingSettings { get; private set; } = null!;
        [field: SerializeField] public BobbingSettings AimBobbingSettings { get; private set; } = null!;
        [field: SerializeField] public RunBobbingSettings RunBobbingSettings { get; private set; } = null!;
        [field: SerializeField] public JumpBobbingSettings JumpBobbingSettings { get; private set; } = null!;
        [field: SerializeField] public JumpBobbingSettings AimJumpBobbingSettings { get; private set; } = null!;
    }
    
    [Serializable]
    public class KickBackSettings
    {
        [field:Header("Kickback Offset Settings")]
        [field: SerializeField] public float speed { get; private set; } = 5.0f;
        [field: SerializeField] public float zKickBack { get; private set; } = -0.05f;
        [field:Header("Recoil Rotation Settings")]
        [field: SerializeField] public float rotationRecoilX { get; private set; } = 5.0f;
        [field: SerializeField] public float rotationRecoilY { get; private set; } = 5.0f;
        [field: SerializeField] public float rotationReturnSpeed { get; private set; } = 10.0f;
        [field:Header("Recoil Multipliers")]
        [field: SerializeField] public float recoilMultiplyX { get; private set; } = 0.025f;
        [field: SerializeField] public float recoilMultiplyY { get; private set; } = 0.015f;
    }

    [Serializable]
    public class SwaySettings
    {
        [field: Header("Position Sway")] 
        [field: Tooltip("Насколько сильно оружие смещается по осям X/Y при движении мыши")]
        [field: SerializeField] public float positionMultiplier { get; private set; } = 0.002f;
        [field: Tooltip("Максимальное смещение по X/Y")]
        [field: SerializeField] public float maxPositionOffset { get; private set; } = 0.05f;
        [field: Tooltip("Скорость сглаживания позиции")]
        [field: SerializeField] public float positionSmooth { get; private set; } = 4.0f;
        [field: Header("Rotation Sway")] 
        [field: Tooltip("Насколько сильно оружие вращается по осям при движении мыши")]
        [field: SerializeField] public float rotationMultiplier { get; private set; } = 1.5f;
        [field: Tooltip("Максимальный угол вращения по каждой оси")]
        [field: SerializeField] public float maxRotationOffset { get; private set; } = 5.0f;
        [field: Tooltip("Скорость сглаживания вращения")]
        [field: SerializeField] public float rotationSmooth { get; private set; } = 8.0f;
    }

    [Serializable]
    public class BobbingSettings
    {
        public BobbingSettings()
        {
             BreatheAmplitude = .0f;
             BreatheFrequency = .0f;
             HeartAmplitude = .0f;
             HeartFrequency = .0f;
             HeartRotationAngle = .0f;

             MaxWalkSpeed = .0f;
             WalkVerticalAmplitude = .0f;
             WalkHorizontalAmplitude = .0f;
             WalkFrequency = .0f;
             StrafeTiltAngle = .0f; 
             WalkSmoothTime = .0f;
             RotationInfluence = .0f;
        }
        
        // ReSharper disable once MemberInitializerValueIgnored
        [field: Header("=== Idle Bobbing ===")]
        [field: SerializeField] public float BreatheAmplitude { get; private set; } = .005f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float BreatheFrequency { get; private set; } = .25f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float HeartAmplitude { get; private set; } = .002f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float HeartFrequency { get; private set; } = .5f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float HeartRotationAngle { get; private set; } = 1.0f;
        [field: Header("=== Walk Bobbing ===")]
        [field: Tooltip("Максимальная скорость для нормализации")]
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float MaxWalkSpeed { get; private set; } = 5.0f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float WalkVerticalAmplitude { get; private set; } = .02f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float WalkHorizontalAmplitude { get; private set; } = .02f;
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float WalkFrequency { get; private set; } = 1.5f;
        [field: Tooltip("Угол наклона оружия при страйфинге")]
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float StrafeTiltAngle { get; private set; } = 7.5f;
        [field: Tooltip("Сглаживание изменения интенсивности ходового боба")]
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float WalkSmoothTime { get; private set; } = 2.0f;
        [field: Header("=== Rotation Influence ===")]
        [field: Tooltip("Множитель, усиливающий вращение боба")]
        [field: Range(0f, 3f)]
        // ReSharper disable once MemberInitializerValueIgnored
        [field: SerializeField] public float RotationInfluence { get; private set; } = 1.0f;
        [field: SerializeField] public bool IsDev { get; private set; }
    }
    
    [Serializable]
    public class RunBobbingSettings
    {
        [field: Tooltip("Вектор локального смещения оружия при беге")]
        public Vector3 RunPositionOffset = new(.175f, -.07f, .125f);
        [field: Tooltip("Локальные углы Эйлера оружия при беге")]
        public Vector3 RunRotationEuler = new(-12.5f, -65.0f, 10.0f);
        [field: Header("Run Bobbing")] 
        [field: Tooltip("Амплитуда бокового боба при беге (позиция)")]
        public float RunBobPositionAmplitude = .02f;
        [field: Tooltip("Амплитуда поворотного боба при беге (градусы)")]
        public float RunBobRotationAmplitude = 1.0f;
        [field: Tooltip("Частота бега (циклов в секунду)")]
        public float RunBobFrequency = 2.0f;
        [field: Tooltip("Время плавного перехода в/из бега (с)")]
        public float RunTransitionTime = 0.15f;
    }

    [Serializable]
    public class JumpBobbingSettings
    {
        [field: Tooltip("Порог скорости Y для распознавания прыжка")]
        public float JumpThreshold = .1f;
        [field: Tooltip("Порог скорости Y для распознавания падения")]
        public float FallThreshold = .1f;
        [field: Header("Position Offsets")] 
        [field: Tooltip("Смещение при прыжке (опускание оружия)")]
        public Vector3 JumpPositionOffset = new(.0f, -.05f, -.05f);
        [field: Tooltip("Смещение при падении (подъём оружия)")]
        public Vector3 FallPositionOffset = new(.0f, .05f, .05f);
        [field: Header("Rotation Offsets")] 
        [field: Tooltip("Углы Эйлера при прыжке (наклон вниз)")]
        public Vector3 JumpRotationEuler = new(10f, .0f, .0f);
        [field: Tooltip("Углы Эйлера при падении (наклон вверх)")]
        public Vector3 FallRotationEuler = new(-10f, .0f, .0f);
        [field: Header("Transition Times")] 
        [field: Tooltip("Время плавного перехода в состояние прыжка")]
        public float JumpTransitionTime = .2f;
        [field: Tooltip("Время плавного перехода в состояние падения")]
        public float FallTransitionTime = .2f;
    }
}
