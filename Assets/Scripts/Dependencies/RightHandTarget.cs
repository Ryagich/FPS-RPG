using UnityEngine;

namespace Dependencies
{
    public readonly struct RightHandTarget
    {
        public Transform Value { get; }

        public RightHandTarget(Transform value) => Value = value;
    }
}
