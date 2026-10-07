using VContainer;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Inventory.Ammo;
using UnityEngine;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;
using Object = UnityEngine.Object;

namespace Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Inventory : IAsyncStartable, IDisposable
    {
        //was | now
        public event Action<Ammo.Ammo, Ammo.Ammo> AmmoChanged;
        public event Action<InventorySlot, InventorySlot> SlotChanged;
        public event Action<IEnumerable<InventorySlot>> SlotsCreated;

        private bool disposed;
        private readonly Dictionary<string, Ammo.Ammo> characterAmmo = new();
        public bool IsReady { get; private set; }

        private readonly InventoryConfig inventoryConfig;
        private readonly LifetimeScope scope;
        private readonly LifetimeScope gameScope;
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
               [Key("GameScope")] LifetimeScope gameScope,
               AmmoStorage ammoStorage,
               [Key("ParentTransformForWeapon")] Transform parentTransform
            )
        {
            this.inventoryConfig = inventoryConfig;
            this.scope = scope;
            this.gameScope = gameScope;
            this.ammoStorage = ammoStorage;
            this.parentTransform = parentTransform;

            var primarySlot = new InventorySlot();
            Slots.Add(primarySlot);

            var secondarySlot = new InventorySlot();
            Slots.Add(secondarySlot);

            // SlotsCreated?.Invoke(Slots);
        }
        
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            try
            {
                await ammoStorage.Ready.AttachExternalCancellation(cancellation);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return;
            }
            if (disposed)
                return;
            
            CreateWeapon(WeaponRole.Primary, inventoryConfig.TestWeaponConfig1.WeaponPref);
            CreateWeapon(WeaponRole.Secondary, inventoryConfig.TestWeaponConfig2.WeaponPref);
            
            SelectWeapon(WeaponRole.Primary);
            IsReady = true;
        }

        public void CreateWeapon(WeaponRole role, WeaponLifetimeScope weaponPrefab)
        {
            var weaponScope = scope.CreateChildFromPrefab(weaponPrefab);
            
            var weaponInstance = weaponScope.Instance;
            var weaponTrans = weaponScope.transform;
            weaponTrans.SetParent(parentTransform, false);

            var slot = GetSlot(role);

            slot.SetItem(weaponInstance);
            slot.SetAmmo(GetAmmo(weaponScope.Config.AmmoConfig));
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
            if (CurrentSlot?.Item is not Weapon.Weapon activeWeapon)
                return;
            var slot = GetSlot(activeWeapon.Config.Role);
            if (slot.Item != null)
            {
                var itemObjTransform = slot.Item.GameObject.transform;
                var dropItemPrefab = slot.Item.GetDropPrefab();
                CreateDroppedItem(dropItemPrefab, itemObjTransform);
                Object.Destroy(itemObjTransform.gameObject);
                slot.SetItem(null);
            }
        }

        public void ClearSlots()
        {
            IsReady = false;
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
                CreateDroppedItem(dropItemPrefab, itemObjTransform);
                Object.Destroy(itemObjTransform.gameObject);
            }
            CreateWeapon(weaponConfig.Role, weaponConfig.WeaponPref);
            // SelectWeapon(weaponConfig.Role);
        }
        
        public void SelectWeapon(WeaponRole role)
        {
            if (!HasWeapon(role))
                return;
            if (CurrentSlot is null)
            {
                CurrentSlot = GetSlot(role);
                CurrentSlot.Activate();
                CurrentAmmo = CurrentSlot.Ammo;
                
                SlotChanged?.Invoke(null, CurrentSlot);
                
                return;
            }
            
            if (CurrentSlot.Item is Weapon.Weapon currentWeapon && currentWeapon.IsShooting)
            {
                return;
            }

            var was = CurrentSlot;
            if (CurrentSlot.Item != null)
                CurrentSlot.Disable();

            CurrentSlot = GetSlot(role);
            CurrentSlot.Activate();

            ChangeCurrentAmmo(((Weapon.Weapon)CurrentSlot.Item).Config.AmmoConfig);
            SlotChanged?.Invoke(was, CurrentSlot);
        }

        public bool HasWeapon(WeaponRole role) => GetSlot(role).Item is Weapon.Weapon;

        private void CreateDroppedItem(LifetimeScope prefab, Transform origin)
        {
            var droppedItem = gameScope.CreateChildFromPrefab(prefab);
            droppedItem.transform.SetParent(null);
            droppedItem.transform.SetPositionAndRotation(origin.position, origin.rotation);
        }

        private Ammo.Ammo GetAmmo(AmmoConfig config)
        {
            if (!characterAmmo.TryGetValue(config.ID, out var ammo))
            {
                ammo = new Ammo.Ammo(ammoStorage.GetById(config.ID));
                characterAmmo.Add(config.ID, ammo);
            }
            return ammo;
        }

        public void Dispose()
        {
            disposed = true;
            IsReady = false;
        }

        private void ChangeCurrentAmmo(AmmoConfig ammoConfig)
        {
            var was = CurrentAmmo;
            CurrentAmmo = GetAmmo(ammoConfig);
            AmmoChanged?.Invoke(was, CurrentAmmo);
        }
    }
}
