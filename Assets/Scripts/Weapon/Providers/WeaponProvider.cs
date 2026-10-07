using System;
using Characters;
using Inventory;
using MessagePipe;
using Messages;
using UnityEngine;
using Weapon.Animations;
using Weapon.Settings;

namespace Weapon.Providers
{
    public sealed class WeaponProvider : IDisposable
    {
        private readonly Inventory.Inventory inventory;
        private readonly CharacterState state;
        private readonly IPublisher<AimChangedMessage> aimChanged;
        private readonly IPublisher<ReloadStartedMessage> reloadStarted;
        private readonly IPublisher<ReloadFinishedMessage> reloadFinished;
        private readonly IPublisher<WeaponChangeStartedMessage> changeStarted;
        private readonly IPublisher<WeaponChangeFinishedMessage> changeFinished;
        private Weapon weapon;
        private WeaponRunBobbing runBobbing;
        private WeaponLowering lowering;
        private WeaponReloading reloading;
        private WeaponRole requestedRole;
        private bool fireRequested;
        private bool aimRequested;
        private bool changingWeapon;
        
        public bool IsChangingWeapon => changingWeapon || (IsReady && lowering != null && lowering.IsRaising);
        public bool CanReload => IsReady && weapon.NeedAmmo() > 0 && inventory.CurrentAmmo != null && inventory.CurrentAmmo.Value > 0;
        public bool IsReady => weapon != null && weapon.GameObject != null;

        public WeaponProvider(Inventory.Inventory inventory, CharacterState state,
            IPublisher<AimChangedMessage> aimChanged, IPublisher<ReloadStartedMessage> reloadStarted,
            IPublisher<ReloadFinishedMessage> reloadFinished,
            IPublisher<WeaponChangeStartedMessage> changeStarted,
            IPublisher<WeaponChangeFinishedMessage> changeFinished)
        {
            this.inventory = inventory;
            this.state = state;
            this.aimChanged = aimChanged;
            this.reloadStarted = reloadStarted;
            this.reloadFinished = reloadFinished;
            this.changeStarted = changeStarted;
            this.changeFinished = changeFinished;
            inventory.SlotChanged += OnSlotChanged;
        }

        public void TakeNewWeapon(WeaponConfig config)
        {
            if (!inventory.IsReady || IsShooting())
                return;
            StopReloading();
            var select = !IsReady || weapon.Config.Role == config.Role;
            inventory.ChangeWeapon(config);
            if (select)
                inventory.SelectWeapon(config.Role);
        }

        public void ChangeWeapon(WeaponRole role)
        {
            if (!IsReady || IsShooting() || IsChangingWeapon || weapon.Config.Role == role
                || !inventory.HasWeapon(role))
                return;
            StopReloading();
            requestedRole = role;
            changingWeapon = true;
            StopSprint();
            SetActualAim(false);
            changeStarted.Publish(new WeaponChangeStartedMessage());
            lowering.Lowered += OnLowered;
            lowering.Lower();
        }

        private void OnLowered()
        {
            lowering.Lowered -= OnLowered;
            inventory.SelectWeapon(requestedRole);
        }

        private void OnSlotChanged(InventorySlot previous, InventorySlot current)
        {
            DetachWeapon();
            weapon = current?.Item as Weapon;
            if (!IsReady)
                return;
            var presentation = weapon.Presentation;
            var attachments = presentation.Attachments;
            attachments.UpdateAttachments();
            var bobbing = presentation.Bobbing;
            bobbing.ScopeSettings = weapon.Config.GetActiveScope()?.ScopesSettings;
            runBobbing = presentation.RunBobbing;
            lowering = presentation.Lowering;
            reloading = presentation.Reloading;
            reloading.EndedReloading += OnReloadFinished;
            lowering.Raised += OnRaised;
            lowering.ResetLowering();
            SetActualAim(aimRequested && !changingWeapon, true);
            if (state.IsSprinting.Value)
                StartSprint();
        }

        private void OnRaised()
        {
            lowering.Raised -= OnRaised;
            if (changingWeapon)
            {
                changingWeapon = false;
                changeFinished.Publish(new WeaponChangeFinishedMessage());
            }
            SetAim(aimRequested);
            if (fireRequested)
                StartShooting();
        }

        public void StartShooting()
        {
            fireRequested = true;
            if (!IsReady || IsReloading() || changingWeapon || lowering.IsLowered || lowering.IsRaising)
                return;
            StopSprint();
            weapon.StartShoot();
        }

        public void StopShooting()
        {
            fireRequested = false;
            if (IsReady)
                weapon.StopShoot();
        }

        public void SetAim(bool aiming)
        {
            aimRequested = aiming;
            SetActualAim(aiming && IsReady && !IsReloading() && !IsChangingWeapon);
            if (state.IsAiming)
                StopSprint();
        }

        private void SetActualAim(bool aiming, bool force = false)
        {
            if (!force && state.IsAiming == aiming)
                return;
            state.IsAiming = aiming;
            // Republish after a slot change so newly created weapon views receive the state.
            aimChanged.Publish(new AimChangedMessage(aiming));
        }

        public bool TryReload()
        {
            if (!IsReady || IsReloading() || IsShooting() || changingWeapon
                || lowering.IsLowered || lowering.IsRaising || inventory.CurrentAmmo == null
                || inventory.CurrentAmmo.Value <= 0 || weapon.NeedAmmo() <= 0)
                return false;
            StopSprint();
            SetActualAim(false);
            reloading.StartReloading();
            reloadStarted.Publish(new ReloadStartedMessage());
            return true;
        }

        private void StopReloading()
        {
            if (!IsReloading())
                return;
            reloading.StopReloading();
            reloadFinished.Publish(new ReloadFinishedMessage(true));
        }

        private void OnReloadFinished()
        {
            if (!IsReady || inventory.CurrentAmmo == null)
                return;
            var amount = Mathf.Min(weapon.NeedAmmo(), (int)inventory.CurrentAmmo.Value);
            inventory.CurrentAmmo.AddValue(-amount);
            weapon.TryChangeValue(amount);
            reloadFinished.Publish(new ReloadFinishedMessage(false));
            SetAim(aimRequested);
            if (fireRequested)
                StartShooting();
        }
        
        public void StartSprint()
        {
            if (IsReady && !IsShooting() && !IsAiming() && !IsReloading() && !changingWeapon)
                runBobbing.StartRun();
        }

        public void StopSprint() => runBobbing?.StopRun();
        public void SetMovementSpeed(Vector3 motion)
        {
            if (IsReady)
                weapon.SetMovementSpeed(motion);
        }

        public bool IsShooting() => IsReady && weapon.IsShooting;
        public bool IsAiming() => state.IsAiming;
        public bool IsSprinting() => state.IsSprinting.Value;
        public bool IsReloading() => IsReady && reloading != null && reloading.IsReloading;
        public bool TrySwitchShootingMode() => IsReady && !IsReloading() && !IsChangingWeapon
            && weapon.TrySwitchShootingMode();

        private void DetachWeapon()
        {
            if (lowering != null)
            {
                lowering.Lowered -= OnLowered;
                lowering.Raised -= OnRaised;
            }
            if (reloading != null)
                reloading.EndedReloading -= OnReloadFinished;
        }
        
        public void Dispose()
        {
            inventory.SlotChanged -= OnSlotChanged;
            DetachWeapon();
        }
    }
}
