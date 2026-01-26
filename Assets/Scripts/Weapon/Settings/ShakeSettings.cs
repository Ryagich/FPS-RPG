using System;
using UnityEngine;

namespace Weapon.Settings
{
    [Serializable]
    public class ShakeSettings
    {
        [field: Header("Shake Settings")]
        [field: Tooltip("Длительность покачивания камеры / Общая длительность тряски (в секундах).")]
        [field: SerializeField] public float duration { get; private set; } = 0.2f;
        [field: Tooltip("Амплитуда покачивания (градусы) / Максимальный угол отклонения (в градусах).")] 
        [field: SerializeField] public float amplitude { get; private set; } = 5f;
        [field: Tooltip("Частота шума.")] 
        [field: SerializeField] public float frequency { get; private set; } = 20f;
        [field: Tooltip("Кривая затухания покачивания / Кривая убывания амплитуды от 1→0 по времени.")] 
        [field: SerializeField] public AnimationCurve falloffCurve { get; private set; } = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [field: SerializeField] public float maxShakePower { get; private set; } = .5f;
        [field: SerializeField] public float RecoilMultiplier { get; private set; } = 0.3f;
    }
}