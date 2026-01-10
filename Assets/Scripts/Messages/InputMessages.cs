using UnityEngine;

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
    
    public readonly struct ClickMessage { }
    public readonly struct RightClickMessage { }       
    public readonly struct JumpMessage { }         
}