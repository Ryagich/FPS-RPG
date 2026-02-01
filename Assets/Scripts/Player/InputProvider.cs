using MessagePipe;
using Messages;
using UniRx;
using VContainer.Unity;
using Weapon.Providers;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InputProvider : IStartable
    {
        private readonly WeaponProvider weaponProvider;
        private readonly MoveStates moveStates;
        
        private bool isClickInput;
        private bool isAimInput;
        
        public InputProvider
            (
                WeaponProvider weaponProvider,
                MoveStates moveStates,
                ISubscriber<SwitchWeaponMessage> switchWeaponMessageSubscriber,
                ISubscriber<ReloadingMessage> reloadingMessageSubscriber,
                ISubscriber<SwitchFireMode> switchFireModeSubscriber,
                ISubscriber<ClickMessage> clickMessageSubscriber,
                ISubscriber<AimChangedMessage> aimChangedMessagePublisher,
                ISubscriber<ChangeSprintStateMessage> changeSprintStateMessageSubscriber,
                ISubscriber<ChangeCrouchingStateMessage> changeCrouchingStateMessageSubscriber,
                ISubscriber<PlayerMoveMessage> playerMoveMessageSubscriber
            )
        {
            this.weaponProvider = weaponProvider;
            this.moveStates = moveStates;
            
            moveStates.IsSprinting.Subscribe(ChangeRunState);

            switchWeaponMessageSubscriber.Subscribe(ChangeWeapon);
            reloadingMessageSubscriber.Subscribe(TryReload);
            switchFireModeSubscriber.Subscribe(SwitchShootingMode);
            clickMessageSubscriber.Subscribe(UseItem); 
            aimChangedMessagePublisher.Subscribe(OnAim);     
            changeSprintStateMessageSubscriber.Subscribe(OnChangeSprintState);
            changeCrouchingStateMessageSubscriber.Subscribe(OnChangeCrouchingState);
            playerMoveMessageSubscriber.Subscribe(OnMove);
        }
        
        private void UseItem(ClickMessage msg)
        {
            isClickInput = msg.State;
            if (isClickInput)
            {
                weaponProvider.StartShooting();

                if (weaponProvider.IsSprint())
                    weaponProvider.StopSprint();
            }
            else
            {
                weaponProvider.StopShoot();
                
                if (moveStates.IsSprinting.Value && CanRun())
                    weaponProvider.StartSprint();
            }
        }
        
        private void OnAim(AimChangedMessage msg)
        {
            isAimInput = msg.State;
            if (isAimInput)
            {
                if (weaponProvider.IsSprint())
                    weaponProvider.StopSprint();
            }
            else
            {
                if (moveStates.IsSprinting.Value && CanRun())
                    weaponProvider.StartSprint();
            }
        }
        
        private void ChangeRunState(bool value)
        {
            if (value)
                weaponProvider.StartSprint();
            else 
                weaponProvider.StopSprint();
        }
        
        private void ChangeWeapon(SwitchWeaponMessage msg)
        {
            weaponProvider.ChangeWeapon(msg.Role);
        }
        
        private void TryReload(ReloadingMessage msg)
        {
            weaponProvider.TryReload();
        }
        
        private void SwitchShootingMode(SwitchFireMode msg)
        {
            weaponProvider.TrySwitchShootingMode();
        }

        private void OnChangeSprintState(ChangeSprintStateMessage msg)
        {
            moveStates.IsSprintingInput = msg.State;
        }
        
        private void OnChangeCrouchingState(ChangeCrouchingStateMessage msg)
        {
            moveStates.IsCrouchingInput = msg.State;
        }
        
        public bool CanRun()
        {
            return !weaponProvider.IsShooting() 
                && !weaponProvider.IsAiming() 
                && !weaponProvider.IsReloading() 
                && !moveStates.IsCrouching.Value
                && moveStates.Direction.sqrMagnitude > 0.001f;
        }
        
        private void OnMove(PlayerMoveMessage msg)
        {
            moveStates.MoveInput = msg.Direction;
        }
        
        public void Start() { }
    }
}