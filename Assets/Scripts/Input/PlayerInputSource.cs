using System;
using System.Collections.Generic;
using Characters;
using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Weapon.Settings;

namespace Input
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class PlayerInputSource : IStartable, ITickable, IDisposable
    {
        private readonly List<InputAction> enabledActions = new();
        private readonly InputConfig inputConfig;
        private readonly CharacterMovementConfig movementConfig;
        private readonly IPublisher<MoveCommand> movePublisher;
        private readonly IPublisher<LookCommand> lookPublisher;
        private readonly IPublisher<JumpCommand> jumpPublisher;
        private readonly IPublisher<SprintCommand> sprintPublisher;
        private readonly IPublisher<CrouchCommand> crouchPublisher;
        private readonly IPublisher<FireCommand> firePublisher;
        private readonly IPublisher<SwitchWeaponCommand> switchWeaponPublisher;
        private readonly IPublisher<ReloadCommand> reloadPublisher;
        private readonly IPublisher<SwitchFireModeCommand> switchFireModePublisher;
        private readonly IPublisher<AimCommand> aimPublisher;
        private readonly IPublisher<InteractCommand> interactPublisher;

        public PlayerInputSource
            (
                InputConfig inputConfig,
                CharacterMovementConfig movementConfig,
                IPublisher<MoveCommand> movePublisher,
                IPublisher<LookCommand> lookPublisher,
                IPublisher<JumpCommand> jumpPublisher,
                IPublisher<SprintCommand> sprintPublisher,
                IPublisher<CrouchCommand> crouchPublisher,
                IPublisher<FireCommand> firePublisher,
                IPublisher<SwitchWeaponCommand> switchWeaponPublisher,
                IPublisher<ReloadCommand> reloadPublisher,
                IPublisher<SwitchFireModeCommand> switchFireModePublisher,
                IPublisher<AimCommand> aimPublisher,
                IPublisher<InteractCommand> interactPublisher
            )
        {
            this.inputConfig = inputConfig;
            this.movementConfig = movementConfig;
            this.movePublisher = movePublisher;
            this.lookPublisher = lookPublisher;
            this.jumpPublisher = jumpPublisher;
            this.sprintPublisher = sprintPublisher;
            this.crouchPublisher = crouchPublisher;
            this.firePublisher = firePublisher;
            this.switchWeaponPublisher = switchWeaponPublisher;
            this.reloadPublisher = reloadPublisher;
            this.switchFireModePublisher = switchFireModePublisher;
            this.aimPublisher = aimPublisher;
            this.interactPublisher = interactPublisher;
        }

        public void Start()
        {
            inputConfig.Click.action.started += StartFire;
            inputConfig.Click.action.canceled += StopStartFire;

            inputConfig.RightClick.action.started += AimIn;
            inputConfig.RightClick.action.canceled += AimOut;

            inputConfig.MoveInput.action.performed += OnMove;
            inputConfig.MoveInput.action.canceled += OnMove;

            inputConfig.JumpInput.action.started += OnJump;

            inputConfig.SprintInput.action.started += StartSprint;
            inputConfig.SprintInput.action.canceled += StopSprint;

            inputConfig.CrouchInput.action.started += StartCrouch;
            inputConfig.CrouchInput.action.canceled += StopCrouch;

            inputConfig.FirstWeapon.action.started += SwitchToFirstWeapon;
            inputConfig.SecondWeapon.action.started += SwitchToSecondWeapon;

            inputConfig.Reloading.action.started += Reload;
            inputConfig.FireMode.action.started += SwitchFireMode;

            inputConfig.Interactable.action.started += Interact;
            foreach (var reference in new[]
            {
                inputConfig.Click, inputConfig.RightClick, inputConfig.MoveInput, inputConfig.LookInput,
                inputConfig.JumpInput, inputConfig.SprintInput, inputConfig.CrouchInput,
                inputConfig.FirstWeapon, inputConfig.SecondWeapon, inputConfig.Reloading,
                inputConfig.FireMode, inputConfig.Interactable
            })
            {
                if (!reference.action.enabled)
                {
                    reference.action.Enable();
                    enabledActions.Add(reference.action);
                }
            }
        }

        public void Dispose()
        {
            inputConfig.Click.action.started -= StartFire;
            inputConfig.Click.action.canceled -= StopStartFire;
            inputConfig.RightClick.action.started -= AimIn;
            inputConfig.RightClick.action.canceled -= AimOut;
            inputConfig.MoveInput.action.performed -= OnMove;
            inputConfig.MoveInput.action.canceled -= OnMove;
            inputConfig.JumpInput.action.started -= OnJump;
            inputConfig.SprintInput.action.started -= StartSprint;
            inputConfig.SprintInput.action.canceled -= StopSprint;
            inputConfig.CrouchInput.action.started -= StartCrouch;
            inputConfig.CrouchInput.action.canceled -= StopCrouch;
            inputConfig.FirstWeapon.action.started -= SwitchToFirstWeapon;
            inputConfig.SecondWeapon.action.started -= SwitchToSecondWeapon;
            inputConfig.Reloading.action.started -= Reload;
            inputConfig.FireMode.action.started -= SwitchFireMode;
            inputConfig.Interactable.action.started -= Interact;
            foreach (var action in enabledActions)
                action.Disable();
            enabledActions.Clear();
        }

        public void Tick()
        {
            lookPublisher.Publish(new LookCommand(inputConfig.LookInput.action.ReadValue<Vector2>() * movementConfig.Sensitivity));
        }

        private void Interact(InputAction.CallbackContext context)
        {
            interactPublisher.Publish(new InteractCommand());
        }

        private void SwitchFireMode(InputAction.CallbackContext context)
        {
            switchFireModePublisher.Publish(new SwitchFireModeCommand());
        }

        private void Reload(InputAction.CallbackContext context)
        {
            reloadPublisher.Publish(new ReloadCommand());
        }

        private void StartFire(InputAction.CallbackContext context)
        {
            firePublisher.Publish(new FireCommand(true));
        }

        private void StopStartFire(InputAction.CallbackContext context)
        {
            firePublisher.Publish(new FireCommand(false));
        }

        private void AimIn(InputAction.CallbackContext context)
        {
            aimPublisher.Publish(new AimCommand(true));
        }

        private void AimOut(InputAction.CallbackContext context)
        {
            aimPublisher.Publish(new AimCommand(false));
        }

        private void SwitchToFirstWeapon(InputAction.CallbackContext context)
        {
            switchWeaponPublisher.Publish(new SwitchWeaponCommand(WeaponRole.Primary));
        }

        private void SwitchToSecondWeapon(InputAction.CallbackContext context)
        {
            switchWeaponPublisher.Publish(new SwitchWeaponCommand(WeaponRole.Secondary));
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();
            movePublisher.Publish(new MoveCommand(dir));
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            jumpPublisher.Publish(new JumpCommand());
        }

        private void StartSprint(InputAction.CallbackContext context = default)
        {
            sprintPublisher.Publish(new SprintCommand(true));
        }

        private void StopSprint(InputAction.CallbackContext context = default)
        {
            sprintPublisher.Publish(new SprintCommand(false));
        }

        private void StartCrouch(InputAction.CallbackContext context)
        {
            crouchPublisher.Publish(new CrouchCommand(true));
        }

        private void StopCrouch(InputAction.CallbackContext context)
        {
            crouchPublisher.Publish(new CrouchCommand(false));
        }
    }
}
