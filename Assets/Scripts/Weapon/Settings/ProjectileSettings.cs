using UnityEngine;

namespace Weapon.Settings
{
    [CreateAssetMenu(fileName = "ProjectileSettings", menuName = "configs/Weapons/Projectile")]
    public sealed class ProjectileSettings : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float Lifetime { get; private set; } = 2f;
        [field: SerializeField, Min(0f)] public float NormalDistance { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float TrailTime { get; private set; } = 0.2f;
    }
}
