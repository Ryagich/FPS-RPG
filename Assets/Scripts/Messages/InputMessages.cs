using UnityEngine;
using Inventory;

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

    /// <summary>Запрос переключить положение лёжа.</summary>
    public readonly struct ChangeProneStateMessage { }

    /// <summary>Направление наклона: -1 — влево, 0 — нейтральное положение, 1 — вправо.</summary>
    public readonly struct ChangeLeanStateMessage
    {
        public readonly float Direction;

        public ChangeLeanStateMessage(float direction)
        {
            Direction = direction;
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
        public readonly EquipmentSlotId Slot;

        public SwitchWeaponMessage(EquipmentSlotId slot)
        {
            Slot = slot;
        }
    }
    public readonly struct ReloadingMessage { }
    public readonly struct SwitchFireMode { }
    public readonly struct InteractableMessage { }
}
