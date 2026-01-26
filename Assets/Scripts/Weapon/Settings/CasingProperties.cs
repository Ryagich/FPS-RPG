using System;
using UnityEngine;

namespace Weapon.Settings
{
    [Serializable]
    public class CasingProperties
    {
        [field: SerializeField] public Vector2 ForceRange { get; private set; } = new(-1.5f, 2.5f);
        [field: SerializeField] public float EjectTorque { get; private set; } = 1.0f;
        [field: SerializeField] public float ConeAngle { get; private set; } = 15.0f;
        [field: SerializeField] public float ProjectileSpeed { get; private set; } = 400.0f;
        [field: SerializeField] public float MovementMultiply { get; private set; } = 1.0f;
    }
}