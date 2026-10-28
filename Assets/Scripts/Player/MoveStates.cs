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
        /// <summary>Мгновенная цель движения от input, в мировых координатах.</summary>
        public Vector3 DesiredDirection;
        /// <summary>Фактическое движение с учётом ускорения, нормализованное в диапазон 0..1.</summary>
        public Vector3 Direction;
        
        public readonly ReactiveProperty<bool> IsSprinting = new ();
        public readonly ReactiveProperty<bool> IsCrouching = new ();
        public readonly ReactiveProperty<bool> IsProne = new ();
    }
}
