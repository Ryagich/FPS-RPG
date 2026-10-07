using UnityEngine;

namespace Dependencies
{
    public readonly struct LeftHandTarget
    {
        public Transform Value { get; }

        public LeftHandTarget(Transform value) => Value = value;
    }
}
