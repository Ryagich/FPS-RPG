using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Inventory;

namespace Input
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InputHandler : IStartable, ITickable
    {
        private readonly InputConfig inputConfig;
        private readonly IPublisher<PlayerMoveMessage> playerMovePublisher;
        private readonly IPublisher<LookDeltaMessage> lookDeltaMessagePublisher;
        private readonly IPublisher<JumpMessage> jumpMessagePublisher;
        private readonly IPublisher<ChangeSprintStateMessage> changeSprintStateMessagePublisher;
        private readonly IPublisher<ChangeCrouchingStateMessage> changeCrouchingStateMessagePublisher;
        private readonly IPublisher<ChangeProneStateMessage> changeProneStateMessagePublisher;
        private readonly IPublisher<ChangeLeanStateMessage> changeLeanStateMessagePublisher;
        private readonly IPublisher<ClickMessage> clickMessagePublisher;
        private readonly IPublisher<SwitchWeaponMessage> switchWeaponMessagePublisher;
        private readonly IPublisher<ReloadingMessage> reloadingMessagePublisher;
        private readonly IPublisher<SwitchFireMode> switchFireModePublisher;
        private readonly IPublisher<AimChangedMessage> aimChangedMessagePublisher;
        private readonly IPublisher<InteractableMessage> interactableMessagePublisher;
        private bool isLeaningLeft;
        private bool isLeaningRight;

        private InputHandler
            (
                InputConfig inputConfig,
                IPublisher<PlayerMoveMessage> playerMovePublisher,
                IPublisher<LookDeltaMessage> lookDeltaMessagePublisher,
                IPublisher<JumpMessage> jumpMessagePublisher,
                IPublisher<ChangeSprintStateMessage> changeSprintStateMessagePublisher,
                IPublisher<ChangeCrouchingStateMessage> changeCrouchingStateMessagePublisher,
                IPublisher<ChangeProneStateMessage> changeProneStateMessagePublisher,
                IPublisher<ChangeLeanStateMessage> changeLeanStateMessagePublisher,
                IPublisher<ClickMessage> clickMessagePublisher,
                IPublisher<SwitchWeaponMessage> switchWeaponMessagePublisher,
                IPublisher<ReloadingMessage> reloadingMessagePublisher,
                IPublisher<SwitchFireMode> switchFireModePublisher,
                IPublisher<AimChangedMessage> aimChangedMessagePublisher,
                IPublisher<InteractableMessage> interactableMessagePublisher 
            )
        {
            this.inputConfig = inputConfig;
            this.playerMovePublisher = playerMovePublisher;
            this.lookDeltaMessagePublisher = lookDeltaMessagePublisher;
            this.jumpMessagePublisher = jumpMessagePublisher;
            this.changeSprintStateMessagePublisher = changeSprintStateMessagePublisher;
            this.changeCrouchingStateMessagePublisher = changeCrouchingStateMessagePublisher;
            this.changeProneStateMessagePublisher = changeProneStateMessagePublisher;
            this.changeLeanStateMessagePublisher = changeLeanStateMessagePublisher;
            this.clickMessagePublisher = clickMessagePublisher;
            this.switchWeaponMessagePublisher = switchWeaponMessagePublisher;
            this.reloadingMessagePublisher = reloadingMessagePublisher;
            this.switchFireModePublisher = switchFireModePublisher;
            this.aimChangedMessagePublisher = aimChangedMessagePublisher;
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
            
            inputConfig.JumpInput.action.started += OnJump;

            inputConfig.SprintInput.action.started += OnStartSprint;
            inputConfig.SprintInput.action.canceled += OnCancelSprint;
            
            inputConfig.CrouchInput.action.started += OnStartCrouching;
            inputConfig.CrouchInput.action.canceled += OnCancelCrouching;
            inputConfig.ProneInput.action.started += OnToggleProne;
            
            inputConfig.FirstWeapon.action.started += SwitchToFirstWeapon;
            inputConfig.SecondWeapon.action.started += SwitchToSecondWeapon;
            inputConfig.ThirdWeapon.action.started += SwitchToMeleeWeapon;

            inputConfig.LeanLeft.action.started += StartLeanLeft;
            inputConfig.LeanLeft.action.canceled += StopLeanLeft;
            inputConfig.LeanRight.action.started += StartLeanRight;
            inputConfig.LeanRight.action.canceled += StopLeanRight;
            
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
            clickMessagePublisher.Publish(new ClickMessage(true));
        }
        
        private void StopUseActiveItem(InputAction.CallbackContext context)
        {
            clickMessagePublisher.Publish(new ClickMessage(false));
        }

        private void AimIn(InputAction.CallbackContext context)
        {
            aimChangedMessagePublisher.Publish(new AimChangedMessage(true));
        }
        
        private void AimOut(InputAction.CallbackContext context)
        {
            aimChangedMessagePublisher.Publish(new AimChangedMessage(false));
        }
        
        private void SwitchToFirstWeapon(InputAction.CallbackContext context)
        {
            switchWeaponMessagePublisher.Publish(new SwitchWeaponMessage(EquipmentSlotId.Primary));
        }

        private void SwitchToSecondWeapon(InputAction.CallbackContext context)
        {
            switchWeaponMessagePublisher.Publish(new SwitchWeaponMessage(EquipmentSlotId.Secondary));
        }

        private void SwitchToMeleeWeapon(InputAction.CallbackContext context)
        {
            switchWeaponMessagePublisher.Publish(new SwitchWeaponMessage(EquipmentSlotId.Melee));
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

        private void OnToggleProne(InputAction.CallbackContext context)
        {
            changeProneStateMessagePublisher.Publish(new ChangeProneStateMessage());
        }

        private void StartLeanLeft(InputAction.CallbackContext context)
        {
            isLeaningLeft = true;
            PublishLeanDirection();
        }

        private void StopLeanLeft(InputAction.CallbackContext context)
        {
            isLeaningLeft = false;
            PublishLeanDirection();
        }

        private void StartLeanRight(InputAction.CallbackContext context)
        {
            isLeaningRight = true;
            PublishLeanDirection();
        }

        private void StopLeanRight(InputAction.CallbackContext context)
        {
            isLeaningRight = false;
            PublishLeanDirection();
        }

        private void PublishLeanDirection()
        {
            // Kinemation interprets a positive LeanInput as a left lean.
            var direction = (isLeaningLeft ? 1f : 0f) - (isLeaningRight ? 1f : 0f);
            changeLeanStateMessagePublisher.Publish(new ChangeLeanStateMessage(direction));
        }
    }
}
