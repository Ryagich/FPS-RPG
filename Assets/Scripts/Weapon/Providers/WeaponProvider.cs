using CameraScripts;
using Inventory;
using UnityEngine;
using VContainer;
using Weapon.Animations;
using Weapon.Attachments;
using Weapon.Settings;

namespace Weapon.Providers
{
    /// <summary>
    /// Управляет активным предметом. Огнестрельные возможности изолированы от
    /// общего контракта IPrimaryActionItem и недоступны холодному оружию.
    /// </summary>
    public sealed class WeaponProvider
    {
        private readonly Inventory.Inventory inventory;

        private IItem activeItem = null!;
        private Weapon firearm = null!;
        private WeaponLifetimeScope firearmScope = null!;
        private WeaponBobbing bobbing = null!;
        private WeaponRunBobbing runBobbing = null!;
        private WeaponLowering lowering = null!;
        private WeaponReloading reloading = null!;
        private AttachmentsController attachmentsController = null!;
        private EquipmentSlotId firearmSlotToChange;
        private bool haveShootRequest;

        public WeaponProvider(Inventory.Inventory inventory)
        {
            this.inventory = inventory;
            inventory.SlotChanged += OnSlotChanged;
        }

        public void TakeNewWeapon(WeaponConfig weaponConfig)
        {
            if (IsShooting() || IsReloading())
                return;

            inventory.ReplaceFirearm(weaponConfig);
            if (inventory.CurrentSlot.Item is Weapon currentFirearm)
                SetFirearm(currentFirearm);
        }

        public void ChangeWeapon(EquipmentSlotId slot)
        {
            if (activeItem is IPrimaryActionItem { IsPrimaryActionInProgress: true })
                return;

            if (firearm != null && IsReloading())
                StopReloading();

            // Kinemation melee animation owns its own equip/unequip transition.
            // Firearm-to-firearm keeps the existing lowering flow.
            if (slot == EquipmentSlotId.Melee || activeItem is not Weapon)
            {
                inventory.SelectSlot(slot);
                return;
            }

            if (inventory.CurrentSlot.Id == slot)
            {
                if (lowering.isLowered)
                {
                    lowering.Lowered -= Lower;
                    lowering.Raise();
                }
                return;
            }

            firearmSlotToChange = slot;
            lowering.Lower();
            lowering.Lowered += Lower;
        }

        public void ChangeWeapon(WeaponRole role) => ChangeWeapon(role switch
        {
            WeaponRole.Primary => EquipmentSlotId.Primary,
            WeaponRole.Secondary => EquipmentSlotId.Secondary,
            _ => throw new System.ArgumentOutOfRangeException(nameof(role), role, null)
        });

        public void StartShooting()
        {
            if (activeItem is not IPrimaryActionItem primaryAction)
                return;

            if (firearm == null)
            {
                primaryAction.TryStartPrimaryAction();
                StopSprint();
                return;
            }

            if (lowering.isLowered || lowering.isRaising)
            {
                haveShootRequest = true;
                lowering.Raised += OnRaising;
                return;
            }
            if (IsReloading())
                return;

            primaryAction.TryStartPrimaryAction();
            StopSprint();
        }

        public void StopShoot()
        {
            if (haveShootRequest && lowering != null)
            {
                haveShootRequest = false;
                lowering.Raised -= OnRaising;
            }
            (activeItem as IPrimaryActionItem)?.StopPrimaryAction();
        }

        public bool TryReload()
        {
            if (firearm == null || IsReloading() || IsShooting() || lowering.isLowered || lowering.isRaising)
                return false;
            if (inventory.CurrentAmmo == null || inventory.CurrentAmmo.Value <= 0 || firearm.NeedAmmo() <= 0)
                return false;

            reloading.StartReloading();
            return true;
        }

        public void StartSprint()
        {
            if (runBobbing != null)
                runBobbing.StartRun();
        }

        public void StopSprint()
        {
            if (runBobbing != null)
                runBobbing.StopRun();
        }

        public void SetMovementSpeed(Vector3 motion)
        {
            firearm?.SetMovementSpeed(motion);
        }

        public bool IsShooting() => activeItem is IPrimaryActionItem { IsPrimaryActionInProgress: true };
        public bool IsAiming() => bobbing != null && bobbing.isAim;
        public bool IsSprint() => runBobbing != null && runBobbing.isRunning;
        public bool IsReloading() => reloading != null && reloading.IsReloading;
        public bool TrySwitchShootingMode() => firearm != null && firearm.TrySwitchShootingMode();

        private void OnSlotChanged(InventorySlot was, InventorySlot now)
        {
            activeItem = now.Item;
            if (now.Item is Weapon newFirearm)
                SetFirearm(newFirearm);
            else
                ClearFirearmState();
        }

        private void SetFirearm(Weapon newFirearm)
        {
            var newScope = newFirearm.GameObject.GetComponent<WeaponLifetimeScope>();
            var newBobbing = newScope.Container.Resolve<WeaponBobbing>();
            var newRunBobbing = newScope.Container.Resolve<WeaponRunBobbing>();
            var newAttachments = newScope.Container.Resolve<AttachmentsController>();

            if (firearm != null)
                reloading.EndedReloading -= OnEndReloading;

            firearmScope = newScope;
            newAttachments.UpdateAttachments();
            newBobbing.scopeSettings = newFirearm.Config.GetActiveScope().ScopesSettings;
            lowering = firearmScope.Container.Resolve<WeaponLowering>();
            reloading = firearmScope.Container.Resolve<WeaponReloading>();
            bobbing = newBobbing;
            runBobbing = newRunBobbing;
            attachmentsController = newAttachments;
            firearm = newFirearm;
            activeItem = newFirearm;
            lowering.ResetLowering();
            reloading.EndedReloading += OnEndReloading;
        }

        private void ClearFirearmState()
        {
            if (reloading != null)
                reloading.EndedReloading -= OnEndReloading;
            firearm = null!;
            firearmScope = null!;
            bobbing = null!;
            runBobbing = null!;
            lowering = null!;
            reloading = null!;
            attachmentsController = null!;
            haveShootRequest = false;
        }

        private void Lower()
        {
            lowering.Lowered -= Lower;
            inventory.SelectSlot(firearmSlotToChange);
        }

        private void OnRaising()
        {
            if (!haveShootRequest)
                return;
            haveShootRequest = false;
            StartShooting();
        }

        private void StopReloading() => reloading.StopReloading();

        private void OnEndReloading()
        {
            if (firearm == null || inventory.CurrentAmmo == null)
                return;
            var amount = Mathf.Min(firearm.NeedAmmo(), inventory.CurrentAmmo.Value);
            inventory.CurrentAmmo.AddValue(-amount);
            firearm.TryChangeValue((int)amount);
        }
    }
}
