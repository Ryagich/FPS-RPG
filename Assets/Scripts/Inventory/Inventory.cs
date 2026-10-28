using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Ammo;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon;
using Weapon.Melee;
using Weapon.Settings;
using Object = UnityEngine.Object;

namespace Inventory
{
    public sealed class Inventory : IStartable
    {
        public event Action<Ammo.Ammo, Ammo.Ammo> AmmoChanged;
        public event Action<InventorySlot, InventorySlot> SlotChanged;
        public event Action<IEnumerable<InventorySlot>> SlotsCreated;

        private readonly InventoryConfig inventoryConfig;
        private readonly LifetimeScope ownerScope;
        private readonly AmmoStorage ammoStorage;
        private readonly Transform parentTransform;
        private readonly bool hasMeleeSlot;

        public List<InventorySlot> Slots { get; } = new();
        public InventorySlot CurrentSlot { get; private set; } = null!;
        public Ammo.Ammo CurrentAmmo { get; private set; } = null!;
        public EquipmentSlotId LastFirearmSlot { get; private set; } = EquipmentSlotId.Primary;

        public Inventory(InventoryConfig inventoryConfig,
                         [Key("InventoryOwnerScope")] LifetimeScope ownerScope,
                         AmmoStorage ammoStorage,
                         [Key("ParentTransformForWeapon")] Transform parentTransform,
                         [Key("HasMeleeSlot")] bool hasMeleeSlot)
        {
            this.inventoryConfig = inventoryConfig;
            this.ownerScope = ownerScope;
            this.ammoStorage = ammoStorage;
            this.parentTransform = parentTransform;
            this.hasMeleeSlot = hasMeleeSlot;

            Slots.Add(new InventorySlot(EquipmentSlotId.Primary));
            Slots.Add(new InventorySlot(EquipmentSlotId.Secondary));
            Slots.Add(new InventorySlot(EquipmentSlotId.Melee));
        }

        public async void Start()
        {
            await ammoStorage.Ready;
            if (inventoryConfig.TestWeaponConfig1 != null)
                CreateWeapon(EquipmentSlotId.Primary, inventoryConfig.TestWeaponConfig1.WeaponPref);
            if (inventoryConfig.TestWeaponConfig2 != null)
                CreateWeapon(EquipmentSlotId.Secondary, inventoryConfig.TestWeaponConfig2.WeaponPref);
            if (hasMeleeSlot && inventoryConfig.DefaultMeleeWeapon != null)
                CreateMeleeWeapon(inventoryConfig.DefaultMeleeWeapon);

            SelectSlot(hasMeleeSlot ? EquipmentSlotId.Melee : EquipmentSlotId.Primary);
            SlotsCreated?.Invoke(Slots);
        }

        public void CreateWeapon(EquipmentSlotId slotId, WeaponLifetimeScope weaponPrefab)
        {
            if (!IsFirearmSlot(slotId))
                throw new ArgumentOutOfRangeException(nameof(slotId), "Firearms can only occupy Primary or Secondary.");

            var weaponScope = CreateScope(weaponPrefab);
            var weapon = weaponScope.Container.Resolve<Weapon.Weapon>();
            var slot = GetSlot(slotId);
            slot.SetItem(weapon);
            slot.SetAmmo(ammoStorage.Ammo.First(ammo => ammo.AmmoConfig.ID.Equals(weapon.Config.AmmoConfig.ID)));
            slot.Disable();
        }

        public void CreateMeleeWeapon(MeleeWeaponConfig config)
        {
            if (config.WeaponPref == null)
                throw new ArgumentException("Melee weapon config requires a lifetime-scope prefab.", nameof(config));

            var meleeScope = CreateScope(config.WeaponPref);
            var meleeWeapon = meleeScope.Container.Resolve<MeleeWeapon>();
            var slot = GetSlot(EquipmentSlotId.Melee);
            slot.SetItem(meleeWeapon);
            slot.Disable();
        }

        public bool ReplaceFirearm(WeaponConfig weaponConfig)
        {
            var target = CurrentSlot?.Id == EquipmentSlotId.Melee ? LastFirearmSlot : ToSlotId(weaponConfig.Role);
            var slot = GetSlot(target);
            DestroyItem(slot, drop: true);
            CreateWeapon(target, weaponConfig.WeaponPref);
            return true;
        }

        public bool SelectSlot(EquipmentSlotId slotId)
        {
            var next = GetSlot(slotId);
            if (next.Item == null)
                return false;

            if (CurrentSlot == null)
            {
                CurrentSlot = next;
                CurrentSlot.Activate();
                UpdateSelectionState(null!, next);
                return true;
            }

            if (CurrentSlot.Id == slotId)
                return true;
            if (CurrentSlot.Item is IPrimaryActionItem { IsPrimaryActionInProgress: true })
                return false;

            var was = CurrentSlot;
            was.Disable();
            CurrentSlot = next;
            next.Activate();
            UpdateSelectionState(was, next);
            return true;
        }

        public void DropWeapon()
        {
            if (CurrentSlot != null)
                DestroyItem(CurrentSlot, drop: true);
        }

        public void ClearSlots()
        {
            foreach (var slot in Slots)
                DestroyItem(slot, drop: false);
        }

        // Compatibility until every caller is migrated to EquipmentSlotId.
        public void ChangeWeapon(WeaponConfig weaponConfig) => ReplaceFirearm(weaponConfig);
        public void SelectWeapon(WeaponRole role) => SelectSlot(ToSlotId(role));
        public void CreateWeapon(WeaponRole role, WeaponLifetimeScope weaponPrefab) => CreateWeapon(ToSlotId(role), weaponPrefab);

        private LifetimeScope CreateScope(LifetimeScope prefab)
        {
            var itemScope = ownerScope.CreateChildFromPrefab(prefab);
            var itemTransform = itemScope.transform;
            var localPosition = itemTransform.localPosition;
            itemTransform.SetParent(parentTransform);
            itemTransform.localPosition = localPosition;
            return itemScope;
        }

        private void UpdateSelectionState(InventorySlot was, InventorySlot now)
        {
            if (IsFirearmSlot(now.Id))
            {
                LastFirearmSlot = now.Id;
                ChangeCurrentAmmo(now.Ammo);
            }
            else
            {
                var previousAmmo = CurrentAmmo;
                CurrentAmmo = null!;
                AmmoChanged?.Invoke(previousAmmo, CurrentAmmo);
            }

            SlotChanged?.Invoke(was, now);
        }

        private void ChangeCurrentAmmo(Ammo.Ammo ammo)
        {
            var was = CurrentAmmo;
            CurrentAmmo = ammo;
            AmmoChanged?.Invoke(was, CurrentAmmo);
        }

        private void DestroyItem(InventorySlot slot, bool drop)
        {
            if (slot.Item == null)
                return;

            if (drop && slot.Item is IDroppableItem droppable)
            {
                var itemTransform = slot.Item.GameObject.transform;
                Object.Instantiate(droppable.GetDropPrefab(), itemTransform.position, itemTransform.rotation);
            }

            Object.Destroy(slot.Item.GameObject);
            slot.SetItem(null!);
        }

        private InventorySlot GetSlot(EquipmentSlotId id) => Slots.First(slot => slot.Id == id);
        private static bool IsFirearmSlot(EquipmentSlotId id) => id is EquipmentSlotId.Primary or EquipmentSlotId.Secondary;

        private static EquipmentSlotId ToSlotId(WeaponRole role) => role switch
        {
            WeaponRole.Primary => EquipmentSlotId.Primary,
            WeaponRole.Secondary => EquipmentSlotId.Secondary,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}
