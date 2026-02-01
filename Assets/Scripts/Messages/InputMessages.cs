using UnityEngine;
using Weapon.Settings;

namespace Messages
{
    public readonly struct PlayerMoveMessage
    {
        public readonly Vector2 Direction;

        public PlayerMoveMessage(Vector2 direction)
        {
            Direction = direction;
        }
    }
    
    public readonly struct LookDeltaMessage
    {
        public readonly Vector2 Delta;

        public LookDeltaMessage(Vector2 delta)
        {
            Delta = delta;
        }
    }

    public readonly struct ChangeSprintStateMessage
    {
        public readonly bool State;
        
        public ChangeSprintStateMessage(bool state)
        {
            State = state;
        }
    }

    public readonly struct ChangeCrouchingStateMessage
    {
        public readonly bool State;
        
        public ChangeCrouchingStateMessage(bool state)
        {
            State = state;
        }
    }

    public readonly struct ClickMessage
    {
        public readonly bool State;

        public ClickMessage(bool state)
        {
            State = state;
        }
    }
    
    public readonly struct RightClickMessage { }       
    public readonly struct JumpMessage { }

    public readonly struct SwitchWeaponMessage
    {
        public readonly WeaponRole Role;

        public SwitchWeaponMessage(WeaponRole role)
        {
            Role = role;
        }
    }
    public readonly struct ReloadingMessage { }
    public readonly struct SwitchFireMode { }
    public readonly struct InteractableMessage { }
}