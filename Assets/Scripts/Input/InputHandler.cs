using MessagePipe;
using Messages;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;

namespace Input
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InputHandler : IStartable, ITickable
    {
        private readonly InputConfig inputConfig;
        private readonly WeaponProvider weaponProvider;
        private readonly PlayerMovement playerMovement;
        private readonly IPublisher<PlayerMoveMessage> playerMovePublisher;
        private readonly IPublisher<LookDeltaMessage> lookDeltaMessagePublisher;
        private readonly IPublisher<JumpMessage> jumpMessagePublisher;
        private readonly IPublisher<ChangeSprintStateMessage> changeSprintStateMessagePublisher;
        private readonly IPublisher<ChangeCrouchingStateMessage> changeCrouchingStateMessagePublisher;
        private readonly IPublisher<ClickMessage> clickMessagePublisher;
        private readonly IPublisher<SwitchWeaponMessage> switchWeaponMessagePublisher;
        private readonly IPublisher<ReloadingMessage> reloadingMessagePublisher;
        private readonly IPublisher<SwitchFireMode> switchFireModePublisher;
        private readonly IPublisher<InteractableMessage> interactableMessagePublisher;

        private InputHandler
            (
                InputConfig inputConfig,
                WeaponProvider weaponProvider,
                PlayerMovement playerMovement,
                IPublisher<PlayerMoveMessage> playerMovePublisher,
                IPublisher<LookDeltaMessage> lookDeltaMessagePublisher,
                IPublisher<JumpMessage> jumpMessagePublisher,
                IPublisher<ChangeSprintStateMessage> changeSprintStateMessagePublisher,
                IPublisher<ChangeCrouchingStateMessage> changeCrouchingStateMessagePublisher,
                IPublisher<ClickMessage> clickMessagePublisher,
                IPublisher<SwitchWeaponMessage> switchWeaponMessagePublisher,
                IPublisher<ReloadingMessage> reloadingMessagePublisher,
                IPublisher<SwitchFireMode> switchFireModePublisher,
                IPublisher<InteractableMessage> interactableMessagePublisher 
            )
        {
            this.inputConfig = inputConfig;
            this.weaponProvider = weaponProvider;
            this.playerMovement = playerMovement;
            this.playerMovePublisher = playerMovePublisher;
            this.lookDeltaMessagePublisher = lookDeltaMessagePublisher;
            this.jumpMessagePublisher = jumpMessagePublisher;
            this.changeSprintStateMessagePublisher = changeSprintStateMessagePublisher;
            this.changeCrouchingStateMessagePublisher = changeCrouchingStateMessagePublisher;
            this.clickMessagePublisher = clickMessagePublisher;
            this.switchWeaponMessagePublisher = switchWeaponMessagePublisher;
            this.reloadingMessagePublisher = reloadingMessagePublisher;
            this.switchFireModePublisher = switchFireModePublisher;
            this.interactableMessagePublisher = interactableMessagePublisher;
        }

        public void Start()
        {
            inputConfig.Click.action.started += UseActiveItem;
            inputConfig.Click.action.canceled += StopUseActiveItem;

            inputConfig.RightClick.action.started += AimIn;
            inputConfig.RightClick.action.canceled += AimOut;

            inputConfig.MoveInput.action.performed += OnMove;
            inputConfig.MoveInput.action.canceled += OnMove;
           
            inputConfig.LookInput.action.performed += OnLookDelta;

            // inputConfig.LookInput.action.started += OnLookDelta;
            // inputConfig.LookInput.action.canceled += OnLookDelta;
            
            inputConfig.JumpInput.action.started += OnJump;

            inputConfig.SprintInput.action.started += OnStartSprint;
            inputConfig.SprintInput.action.canceled += OnCancelSprint;
            
            inputConfig.CrouchInput.action.started += OnStartCrouching;
            inputConfig.CrouchInput.action.canceled += OnCancelCrouching;
            
            // inputConfig.CrouchInput.action.started += OnStartCrouching;
            // inputConfig.CrouchInput.action.canceled += OnCancelCrouching;
          
            inputConfig.FirstWeapon.action.started += SwitchToFirstWeapon;
            inputConfig.SecondWeapon.action.started += SwitchToSecondWeapon;
            
            inputConfig.Reloading.action.started += Reloading;
            inputConfig.FireMode.action.started += SwitchShootingMod;
            
            inputConfig.Interactable.action.started += Interact;
        }

        public void Tick()
        {
            lookDeltaMessagePublisher.Publish(new LookDeltaMessage(inputConfig.LookInput.action.ReadValue<Vector2>()));
        }
        
        private void Interact(InputAction.CallbackContext context)
        {
            interactableMessagePublisher.Publish(new InteractableMessage());
        }
        
        private void SwitchShootingMod(InputAction.CallbackContext context)
        {
            switchFireModePublisher.Publish(new SwitchFireMode());
        }
        
        private void Reloading(InputAction.CallbackContext context)
        {
            reloadingMessagePublisher.Publish(new ReloadingMessage());
        }
        
        //Нужно зарефакторить. Хендлер всегда шлет сообщения. А уже сами классы решают что делать с инфой.
        private void UseActiveItem(InputAction.CallbackContext context)
        {
            if (inputConfig.SprintInput.action.IsPressed() && weaponProvider.IsSprint())
                OnCancelSprint();
            weaponProvider.StartShooting();
        }
        
        private void StopUseActiveItem(InputAction.CallbackContext context)
        {
            weaponProvider.StopShoot();
            if (inputConfig.SprintInput.action.IsPressed() && CanRun())
                OnStartSprint();
        }
        
        private void AimIn(InputAction.CallbackContext context)
        {
            if (inputConfig.SprintInput.action.IsPressed() && weaponProvider.IsSprint())
                OnCancelSprint();
            weaponProvider.AimIn();
        }
        
        private void AimOut(InputAction.CallbackContext context)
        {
            weaponProvider.AimOut();
            if (inputConfig.SprintInput.action.IsPressed() && CanRun())
                OnStartSprint();
        }
        
        private bool CanRun()
        {
            return !weaponProvider.IsShooting() 
                && !weaponProvider.IsAiming() 
                && !weaponProvider.IsReloading() 
                && !playerMovement.IsCrouching.Value;
        }
        
        private void SwitchToFirstWeapon(InputAction.CallbackContext context)
        {
            switchWeaponMessagePublisher.Publish(new SwitchWeaponMessage(WeaponRole.Primary));
        }

        private void SwitchToSecondWeapon(InputAction.CallbackContext context)
        {
            switchWeaponMessagePublisher.Publish(new SwitchWeaponMessage(WeaponRole.Secondary));
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();
            playerMovePublisher.Publish(new PlayerMoveMessage(dir));
        }

        private void OnLookDelta(InputAction.CallbackContext context)
        {
            var delta = context.ReadValue<Vector2>();
            lookDeltaMessagePublisher.Publish(new LookDeltaMessage(delta));
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            jumpMessagePublisher.Publish(new JumpMessage());
        }
        
        private void OnStartSprint(InputAction.CallbackContext context = default)
        {
            changeSprintStateMessagePublisher.Publish(new ChangeSprintStateMessage(true));
        }
        
        private void OnCancelSprint(InputAction.CallbackContext context = default)
        {
            changeSprintStateMessagePublisher.Publish(new ChangeSprintStateMessage(false));
        }
        
        private void OnStartCrouching(InputAction.CallbackContext context)
        {
            changeCrouchingStateMessagePublisher.Publish(new ChangeCrouchingStateMessage(true));
        }
        
        private void OnCancelCrouching(InputAction.CallbackContext context)
        {
            changeCrouchingStateMessagePublisher.Publish(new ChangeCrouchingStateMessage(false));
        }
    }
}