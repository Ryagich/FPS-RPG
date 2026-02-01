using UniRx;
using UnityEngine;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MoveStates
    {
        public bool IsCrouchingInput;
        public bool IsSprintingInput;
        public Vector2 MoveInput;
        public Vector3 Direction;
        
        public readonly ReactiveProperty<bool> IsSprinting = new ();
        public readonly ReactiveProperty<bool> IsCrouching = new ();
    }
}