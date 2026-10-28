using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using KINEMATION.FPSAnimationFramework.Runtime.Layers.IkMotionLayer;
using UnityEngine;
using Weapon.Settings;

namespace Weapon.Melee
{
    [CreateAssetMenu(fileName = "MeleeWeaponConfig", menuName = "Configs/Weapons/Melee Weapon")]
    public sealed class MeleeWeaponConfig : WeaponConfigBase
    {
        [field: Header("Container prefab")]
        [field: SerializeField] public MeleeWeaponLifetimeScope WeaponPref { get; private set; } = null!;

        [field: Header("Kinemation animation")]
        [field: SerializeField] public RuntimeAnimatorController OverrideController { get; private set; } = null!;
        [field: SerializeField] public IkMotionLayerSettings EquipMotion { get; private set; } = null!;
        [field: SerializeField] public IkMotionLayerSettings UnequipMotion { get; private set; } = null!;
        [field: SerializeField] public FPSAnimationAsset AttackAnimation { get; private set; } = null!;
    }
}
