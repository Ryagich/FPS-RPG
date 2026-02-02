using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Ammo;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;
using Object = UnityEngine.Object;

namespace Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Inventory : IStartable
    {
        //was | now
        public event Action<Ammo.Ammo, Ammo.Ammo> AmmoChanged;
        public event Action<InventorySlot, InventorySlot> SlotChanged;
        public event Action<IEnumerable<InventorySlot>> SlotsCreated;

        private readonly InventoryConfig inventoryConfig;
        private readonly LifetimeScope scope;
        private readonly AmmoStorage ammoStorage;
        private readonly Transform parentTransform;
        
        public List<InventorySlot> Slots { get; } = new();
        public InventorySlot CurrentSlot { get; private set; } = null!;
        public Ammo.Ammo CurrentAmmo { get; private set; } = null!;

        // [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public Inventory
            (
               InventoryConfig inventoryConfig,
               LifetimeScope scope,
               AmmoStorage ammoStorage,
               [Key("ParentTransformForWeapon")] Transform parentTransform
            )
        {
            this.inventoryConfig = inventoryConfig;
            this.scope = scope;
            this.ammoStorage = ammoStorage;
            this.parentTransform = parentTransform;

            var primarySlot = new InventorySlot();
            Slots.Add(primarySlot);

            var secondarySlot = new InventorySlot();
            Slots.Add(secondarySlot);

            // SlotsCreated?.Invoke(Slots);
        }
        
        public async void Start()
        {
            await ammoStorage.Ready;
            
            CreateWeapon(WeaponRole.Primary, inventoryConfig.TestWeaponConfig1.WeaponPref);
            CreateWeapon(WeaponRole.Secondary, inventoryConfig.TestWeaponConfig2.WeaponPref);
            
            SelectWeapon(WeaponRole.Primary);
        }

        public void CreateWeapon(WeaponRole role, WeaponLifetimeScope weaponPrefab)
        {
            var weaponScope = scope.CreateChildFromPrefab(weaponPrefab);
            
            var weaponInstance = weaponScope.Container.Resolve<Weapon.Weapon>();
            var weaponTrans = weaponScope.transform;
            var pos = weaponTrans.localPosition;
            weaponTrans.SetParent(parentTransform);
            weaponTrans.localPosition = pos;

            var slot = GetSlot(role);

            slot.SetItem(weaponInstance);
            slot.SetAmmo(ammoStorage.Ammo.First(ammo => ammo.AmmoConfig.ID.Equals(weaponScope.Config.AmmoConfig.ID)));
            slot.Disable();
        }

        private InventorySlot GetSlot(WeaponRole role)
        {
            var id = role switch
            {
                WeaponRole.Primary => 0,
                WeaponRole.Secondary => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };

            return Slots[id];
        }

        public void DropWeapon()
        {
            var slot = GetSlot(((Weapon.Weapon)CurrentSlot.Item).Config.Role);
            if (slot.Item != null)
            {
                var itemObjTransform = slot.Item.GameObject.transform;
                var dropItemPrefab = slot.Item.GetDropPrefab();
                Object.Instantiate(dropItemPrefab,itemObjTransform.position, itemObjTransform.rotation);
                Object.Destroy(itemObjTransform.gameObject);
                slot.SetItem(null);
            }
        }

        public void ClearSlots()
        {
            foreach (var slot in Slots)
            {
                if (slot.Item != null)
                {
                    var itemObjTransform = slot.Item.GameObject.transform;
                    Object.Destroy(itemObjTransform.gameObject);
                    slot.SetItem(null);
                }
            }
        }
        
        public void ChangeWeapon(WeaponConfig weaponConfig)
        {
            var slot = GetSlot(weaponConfig.Role);
            if (slot.Item != null)
            {
                var itemObjTransform = slot.Item.GameObject.transform;
                var dropItemPrefab = slot.Item.GetDropPrefab();
                Object.Instantiate(dropItemPrefab,itemObjTransform.position, itemObjTransform.rotation);
                Object.Destroy(itemObjTransform.gameObject);
            }
            CreateWeapon(weaponConfig.Role, weaponConfig.WeaponPref);
            // SelectWeapon(weaponConfig.Role);
        }
        
        public void SelectWeapon(WeaponRole role)
        {
            if (CurrentSlot is null)
            {
                CurrentSlot = GetSlot(role);
                CurrentSlot.Activate();
                CurrentAmmo = ammoStorage.Ammo
                                         .First(ammo => ammo.AmmoConfig.ID.Equals(((Weapon.Weapon)CurrentSlot.Item).Config.AmmoConfig.ID));
                
                SlotChanged?.Invoke(null, CurrentSlot);
                
                return;
            }
            
            if (((Weapon.Weapon)CurrentSlot.Item).IsShooting)
            {
                return;
            }

            var was = CurrentSlot;
            CurrentSlot.Disable();

            CurrentSlot = GetSlot(role);
            CurrentSlot.Activate();

            ChangeCurrentAmmo(((Weapon.Weapon)CurrentSlot.Item).Config.AmmoConfig);
            SlotChanged?.Invoke(was, CurrentSlot);
        }

        private void ChangeCurrentAmmo(AmmoConfig ammoConfig)
        {
            var was = CurrentAmmo;
            var ammo = ammoStorage.Ammo.FirstOrDefault(a => a.AmmoConfig.ID.Equals(ammoConfig.ID));

            CurrentAmmo = ammo ?? throw new ArgumentException("Пришел тип патронов не записанный в инвентарь");
            AmmoChanged?.Invoke(was, CurrentAmmo);
        }
    }
}
