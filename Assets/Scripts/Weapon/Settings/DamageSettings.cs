using System;
using UnityEngine;

namespace Weapon.Settings
{
    [Serializable]
    public class DamageSettings
    {
        [field: SerializeField] public float Damage { get; private set; } = 10;
        [field: Tooltip("Как урон меняется, взависимости от расстояния")]
        [field: SerializeField] public AnimationCurve DistanceChanged { get; private set; } = AnimationCurve.EaseInOut(0, 1, 1, 0);
    }
}