using System.Collections.Generic;
using Inventory.Pools.Impact;
using UnityEngine;
using Weapon;
using Weapon.Settings;

namespace Inventory
{
    [CreateAssetMenu(fileName = "InventoryConfig", menuName = "configs/Inventory/InventoryConfig")]
    public class InventoryConfig : ScriptableObject
    {
        [SerializeField] private List<Ammo.Ammo> ammoList = new();
        [field: SerializeField] public Projectile ProjectilePref { get; private set; }
        [field: SerializeField] public GameObject CasingPref { get; private set; }
        [field: SerializeField] public float casingLifeTime { get; private set; } = 5f;
        
        [SerializeField] private List<ImpactConfig> impacts = new();
        public List<ImpactConfig> Impacts => impacts;
        public List<Ammo.Ammo> AmmoList => ammoList;

        [field: SerializeField] public WeaponConfig TestWeaponConfig1 { get; private set; }
        [field: SerializeField] public WeaponConfig TestWeaponConfig2 { get; private set; }
        }
}