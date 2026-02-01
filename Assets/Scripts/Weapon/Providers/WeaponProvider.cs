using CameraScripts;
using Inventory;
using UnityEngine;
using VContainer;
using Weapon.Animations;
using Weapon.Attachments;
using Weapon.Settings;

namespace Weapon.Providers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponProvider
    {
        private Vector2 movementDirection;
        
        private readonly Inventory.Inventory inventory;

        // private readonly HandsTargets handsTargets = null!;

        private Weapon weapon;
        private WeaponLifetimeScope weaponScope;

        private WeaponBobbing bobbing;
        private WeaponRunBobbing runBobbing;
        private WeaponLowering lowering;
        private WeaponReloading reloading;
        private AttachmentsController attachmentsController;
        private WeaponRole roleToChange;
        
        private bool haveShootRequest;

        public WeaponProvider
            (
                Inventory.Inventory inventory
                // HandsTargets handsTargets,
            )
        {
            this.inventory = inventory;
            // this.handsTargets = handsTargets;

            inventory.SlotChanged += OnSlotChanged;
        }

        public void TakeNewWeapon(WeaponConfig weaponConfig)
        {
            if (weapon is not null)
            {
                if (IsShooting())
                    return;
                if (IsReloading())
                    StopReloading();
                if (lowering.isLowered)
                    lowering.Lowered -= Lower;
                
                inventory.ChangeWeapon(weaponConfig);
                if (weaponConfig.Role == weapon.Config.Role)
                {
                    inventory.SelectWeapon(weaponConfig.Role);
                    SetWeapon((Weapon)inventory.CurrentSlot.Item);
                }
            }
            else
            {
                inventory.ChangeWeapon(weaponConfig);
                inventory.SelectWeapon(weaponConfig.Role);
                SetWeapon((Weapon)inventory.CurrentSlot.Item);
            }
        }

        public void ChangeWeapon(WeaponRole role)
        {
            if (weapon is not null)
            {
                if (IsShooting())
                    return;
                
                if (weapon.Config.Role == role)
                {
                    if (lowering.isLowered)
                    {
                        lowering.Lowered -= Lower;
                        lowering.Raise();
                    }
                }
                else
                {
                    if (IsReloading())
                        StopReloading();
                    
                    roleToChange = role;
                    lowering.Lower();
                    lowering.Lowered += Lower;
                }
            }
            else
            {
                inventory.SelectWeapon(role);
                SetWeapon((Weapon)inventory.CurrentSlot.Item);
            }
        }

        private void Lower()
        {
            //Руки позже
            // handsTargets.SetTarget(null!, null!);
            lowering.Lowered -= Lower;
            inventory.SelectWeapon(roleToChange);
        }

        private void OnSlotChanged(InventorySlot was, InventorySlot now)
        {
            SetWeapon((Weapon)now.Item);
        }

        private void SetWeapon(Weapon newWeapon)
        {
            var newScope = newWeapon.GameObject.GetComponent<WeaponLifetimeScope>();
            var newBobbing = newScope.Container.Resolve<WeaponBobbing>();
            var newRunBobbing = newScope.Container.Resolve<WeaponRunBobbing>();
            var scopeInfo = newWeapon.Config.GetActiveScope();

            var newAttachmentsC = newScope.Container.Resolve<AttachmentsController>();
            newAttachmentsC.UpdateAttachments();

            if (weapon != null)
            {
                newBobbing.isAim = bobbing.isAim;
                newRunBobbing.isRunning = runBobbing.isRunning;
                reloading.EndedReloading -= OnEndReloading;
            }
            weaponScope = newScope;

            newBobbing.scopeSettings = scopeInfo.ScopesSettings;
            //TODO: Когда будут прицелы с зумом - разобраться с их камерами
            // if (newAttachmentsC.Scope.ScopeCamera)
            //     newAttachmentsC.Scope.ScopeCamera.fieldOfView = cameraFovConfig.AimFov / scopeInfo.BaseInfo.ScopeSettings.Zoom;

            lowering = weaponScope.Container.Resolve<WeaponLowering>();
            reloading = newScope.Container.Resolve<WeaponReloading>();

            bobbing = newBobbing;
            runBobbing = newRunBobbing;
            attachmentsController = newAttachmentsC;
            weapon = newWeapon;
            roleToChange = newWeapon.Config.Role;

            attachmentsController.UpdateAttachments();
            lowering.ResetLowering();
            
            if (haveShootRequest)
            {
                lowering.Raised += OnRaising;
            }
            
            //TODO: Руки перенесу позже
            // var leftTarget = newAttachmentsC.Grip.LeftHandTarget;
            // var rightTarget = newWeapon.GetComponent<IKPointsInWeapon>().RightTarget;
            // handsTargets.SetTarget(leftTarget, rightTarget);

            reloading.EndedReloading += OnEndReloading;
        }

        private void OnRaising()
        {
            if (haveShootRequest)
            {
                haveShootRequest = false;
                StartShooting();
            }
        }

        public void StartShooting()
        {
            if (lowering.isLowered)
            {
                haveShootRequest = true;
                return;
            }
            if (lowering.isRaising)
            {
                haveShootRequest = true;
                lowering.Raised += OnRaising;
                return;
            }
            if (IsReloading())
            {
                return;
            }
            weapon.StartShoot();
            if (runBobbing.isRunning)
                StopSprint();
        }

        public void StopShoot()
        {
            if (haveShootRequest)
            {
                haveShootRequest = false;
                lowering.Raised -= OnRaising;
            }
            weapon.StopShoot();
        }

#region Reloading

        public bool TryReload()
        {
            if (IsReloading() || IsShooting() || lowering.isLowered || lowering.isRaising)
            {
                return false;
            }
            if (inventory.CurrentAmmo.Value <= 0 || weapon.NeedAmmo() <= 0)
            {
                return false;
            }

            reloading.StartReloading();

            return false;
        }

        private void StopReloading()
        {
            reloading.StopReloading();
        }

        private void OnEndReloading()
        {
            var value = ((Weapon)inventory.CurrentSlot.Item).NeedAmmo() <= inventory.CurrentAmmo.Value
                            ? ((Weapon)inventory.CurrentSlot.Item).NeedAmmo()
                            : inventory.CurrentAmmo.Value;
            inventory.CurrentAmmo.AddValue(-value);
            ((Weapon)inventory.CurrentSlot.Item).TryChangeValue((int)value);
        }

#endregion
        
        public void StartSprint()
        {
            runBobbing.StartRun();
            // cameraFovController.SetRunFov();
        }

        public void StopSprint()
        {
            if (runBobbing != null)
                runBobbing.StopRun();
            // cameraFovController.SetDefaultFov();
        }

        public void SetMovementSpeed(Vector3 motion)
        {
            weapon.SetMovementSpeed(motion);
        }
        
        public bool IsShooting() => weapon.IsShooting;
        public bool IsAiming() => bobbing.isAim;
        public bool IsSprint() => runBobbing.isRunning;
        public bool IsReloading() => reloading.IsReloading;

        public bool TrySwitchShootingMode() => weapon.TrySwitchShootingMode();
    }
}
