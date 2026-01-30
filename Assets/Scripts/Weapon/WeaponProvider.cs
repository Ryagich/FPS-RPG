using System;
using CameraScripts;
using Inventory;
using MessagePipe;
using Messages;
using NaughtyAttributes;
using Player;
using UnityEngine;
using VContainer;
using Weapon.Animations;
using Weapon.Attachments;
using Weapon.Settings;
using UniRx;

namespace Weapon
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponProvider
    {
        public event Action<bool> Aimed;
        public event Action<bool> Sprinted;

        private Vector2 movementDirection;
        
        private readonly Inventory.Inventory inventory;
        private readonly CharacterController characterController;
        private readonly CameraFovConfig cameraFovConfig;
        private readonly PlayerMovement playerMovement;

        private readonly CameraFovController cameraFovController;
        // private readonly HandsTargets handsTargets = null!;

        private Weapon weapon;
        private WeaponBobbing bobbing;
        private WeaponRunBobbing runBobbing;
        private WeaponKickBack kickBack;
        private WeaponLowering lowering;
        private WeaponJumpBobbing jumpBobbing;
        private WeaponSway sway;
        private WeaponReloading reloading;
        private AttachmentsController attachmentsController;
        private WeaponRole roleToChange;
        
        private bool haveShootRequest;

        // [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public WeaponProvider
            (
                Inventory.Inventory inventory,
                CharacterController characterController,
                CameraFovConfig cameraFovConfig,
                PlayerMovement playerMovement,
                // HandsTargets handsTargets,
                CameraFovController cameraFovController,
                ISubscriber<SwitchWeaponMessage> switchWeaponMessageSubscriber,
                ISubscriber<ClickMessage> clickMessageSubscriber,
                ISubscriber<ReloadingMessage> reloadingMessageSubscriber,
                ISubscriber<SwitchFireMode> switchFireModeSubscriber
            )
        {
            this.inventory = inventory;
            this.cameraFovConfig = cameraFovConfig;
            this.playerMovement = playerMovement;
            this.characterController = characterController;
            // this.handsTargets = handsTargets;
            this.cameraFovController = cameraFovController;

            playerMovement.IsSprinting.Subscribe(ChangeRunState);
            inventory.SlotChanged += OnSlotChanged;
            Aimed += OnAimStateChanged;

            switchWeaponMessageSubscriber.Subscribe(ChangeWeapon);
            reloadingMessageSubscriber.Subscribe(TryReload);
            switchFireModeSubscriber.Subscribe(SwitchShootingMode);
            // clickMessageSubscriber.Subscribe();
        }

        private void ChangeRunState(bool value)
        {
            if (value)
                StartSprint();
            else 
                StopSprint();
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

        private void ChangeWeapon(SwitchWeaponMessage msg)
        {
            if (weapon is not null)
            {
                if (IsShooting())
                    return;
                
                if (weapon.Config.Role == msg.Role)
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
                    
                    roleToChange = msg.Role;
                    lowering.Lower();
                    lowering.Lowered += Lower;
                }
            }
            else
            {
                inventory.SelectWeapon(msg.Role);
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
                reloading.UpdateReloadingTime -= OnUpdateReloadingTime;
            }

            newBobbing.scopeSettings = scopeInfo.ScopesSettings;
            if (newAttachmentsC.Scope.ScopeCamera)
                newAttachmentsC.Scope.ScopeCamera.fieldOfView = cameraFovConfig.AimFov / scopeInfo.BaseInfo.ScopeSettings.Zoom;

            kickBack = newScope.Container.Resolve<WeaponKickBack>();
            lowering = newScope.Container.Resolve<WeaponLowering>();
            jumpBobbing = newScope.Container.Resolve<WeaponJumpBobbing>();
            sway = newScope.Container.Resolve<WeaponSway>();
            reloading = newScope.Container.Resolve<WeaponReloading>();

            bobbing = newBobbing;
            runBobbing = newRunBobbing;
            attachmentsController = newAttachmentsC;
            weapon = newWeapon;
            roleToChange = newWeapon.Config.Role;

            jumpBobbing.characterController = characterController;
            attachmentsController.UpdateAttachments();
            lowering.ResetLowering();
            lowering.transitionTime = weapon.Config.WeaponAnimationSettings.loweredTransitionTime;
            lowering.loweredPositionOffset = weapon.Config.WeaponAnimationSettings.loweredPositionOffset;
            lowering.loweredRotationEuler = weapon.Config.WeaponAnimationSettings.loweredRotationEuler;
            {
                jumpBobbing.UseCurrentSettings(IsAiming());
                bobbing.SetCurrentSettings(IsAiming());
                sway.SetCurrentSettings(IsAiming());
                kickBack.SetCurrentSettings(IsAiming());
            }
            if (haveShootRequest)
            {
                lowering.Raised += OnRaising;
            }
            
            //Руки перенесу позже
            // var leftTarget = newAttachmentsC.Grip.LeftHandTarget;
            // var rightTarget = newWeapon.GetComponent<IKPointsInWeapon>().RightTarget;
            // handsTargets.SetTarget(leftTarget, rightTarget);

            reloading.UpdateReloadingTime += OnUpdateReloadingTime;
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

        public event Action EndedReloading;
        public event Action<bool> Reloading;
        public event Action<float, float> UpdateReloadingTime;

        public void TryReload(ReloadingMessage msg)
        {
            Debug.Log($"Reloading");
            TryReload();
        }
        
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
            Reloading?.Invoke(true);

            return false;
        }

        public void StopReloading()
        {
            reloading.StopReloading();
            Reloading?.Invoke(false);
        }

        public void OnEndReloading()
        {
            var value = ((Weapon)inventory.CurrentSlot.Item).NeedAmmo() <= inventory.CurrentAmmo.Value
                            ? ((Weapon)inventory.CurrentSlot.Item).NeedAmmo()
                            : inventory.CurrentAmmo.Value;
            inventory.CurrentAmmo.AddValue(-value);
            ((Weapon)inventory.CurrentSlot.Item).TryChangeValue((int)value);
            Reloading?.Invoke(false);
            EndedReloading?.Invoke();
        }

        public void OnUpdateReloadingTime(float current, float max)
        {
            UpdateReloadingTime?.Invoke(current, max);
        }

#endregion

        [Button]
        public void AimIn()
        {
            if (bobbing is null)
                return;
            bobbing.StartAim();
            //Тут Посылать месседж о прицеливание
            // cameraFovController.SetAimFov();
            Aimed?.Invoke(bobbing.isAim);
            GlobalMessagePipe.GetPublisher<AimChangedMessage>().Publish(new AimChangedMessage(true));
        }

        [Button]
        public void AimOut()
        {
            if (bobbing is null)
                return;
            bobbing.StopAim();
            //Позже реакцию камеры на прицеливание
            //cameraFovController.SetDefaultFov();
            Aimed?.Invoke(bobbing.isAim);
            GlobalMessagePipe.GetPublisher<AimChangedMessage>().Publish(new AimChangedMessage(false));
        }

        private void OnAimStateChanged(bool newState)
        {
            jumpBobbing.UseCurrentSettings(newState);
            bobbing.SetCurrentSettings(newState);
            sway.SetCurrentSettings(newState);
            kickBack.SetCurrentSettings(newState);
        }

        public void UpdateMovementDirection(Vector2 newMovementDirection)
        {
            movementDirection = newMovementDirection;
            if (Vector3.zero.Equals(movementDirection) && runBobbing.isRunning)
                StopSprint();
        }
        
        public void StartSprint()
        {
            if (weapon is null || IsShooting() || IsAiming() || IsReloading())
            {
                Debug.Log($"Cant Sprint");   
                return;
            }
            runBobbing.StartRun();
            // cameraFovController.SetRunFov();
            Sprinted?.Invoke(runBobbing.isRunning);
        }

        public void StopSprint()
        {
            if (runBobbing is null)
                return;
            runBobbing.StopRun();
            // cameraFovController.SetDefaultFov();
            Sprinted?.Invoke(runBobbing.isRunning);
        }

        public void SetMovementSpeed(Vector3 motion)
        {
            weapon.SetMovementSpeed(motion);
        }
        
        public bool IsShooting() => weapon.IsShooting;
        public bool IsAiming() => bobbing.isAim;
        public bool IsSprint() => runBobbing.isRunning;
        public bool IsReloading() => reloading.IsReloading;

        public void SwitchShootingMode(SwitchFireMode msg) => weapon.TrySwitchShootingMode();
        public bool TrySwitchShootingMode() => weapon.TrySwitchShootingMode();
    }
}
