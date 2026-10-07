using UnityEngine;

namespace Dependencies
{
    public readonly struct BotGoal
    {
        public Transform Value { get; }

        public BotGoal(Transform value) => Value = value;
    }
}
