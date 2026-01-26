using UnityEngine;

namespace Sounds.Movement
{
    [CreateAssetMenu(
                        fileName = "Distance To Play Movement Sound Config",
                        menuName = "configs/Sounds/Distance To Play Movement Sound")]
    public class MovementSoundConfig : ScriptableObject
    {
        [field: SerializeField] public float WalkStepDistance { get; private set; } = 1.8f;
        [field: SerializeField] public float SprintStepDistance { get; private set; } = 1.2f;
        [field: SerializeField] public float CrouchStepDistance { get; private set; } = 2.4f;

        [field: SerializeField] public LayerMask GroundMask { get; private set; }
        [field: SerializeField] public LayerMask WoodMask { get; private set; }
        [field: SerializeField] public LayerMask RockMask { get; private set; }

        [field: SerializeField] public SoundConfig StepOnRockSoundConfig { get; private set; } = null!;
        [field: SerializeField] public SoundConfig StepOnGroundSoundConfig { get; private set; } = null!;
        [field: SerializeField] public SoundConfig StepOnWoodSoundConfig { get; private set; } = null!;

        [field: Header("Camera Step Shake")]
        [field: SerializeField, Tooltip("Длительность покачивания камеры от одного шага")]
        public float StepShakeDuration { get; private set; } = 0.15f;

        [field: SerializeField, Tooltip("Амплитуда покачивания (градусы)")]
        public float StepShakeAmplitude { get; private set; } = 1.2f;

        [field: SerializeField, Tooltip("Частота шума для шага")]
        public float StepShakeFrequency { get; private set; } = 6f;

        [field: SerializeField, Tooltip("Кривая затухания покачивания шага")]
        public AnimationCurve StepShakeFalloff { get; private set; }
            = AnimationCurve.EaseInOut(0, 1, 1, 0);
    }
}