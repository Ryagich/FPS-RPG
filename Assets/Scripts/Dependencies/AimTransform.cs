using UnityEngine;

namespace Dependencies
{
    public readonly struct AimTransform
    {
        public Transform Value { get; }

        public AimTransform(Transform value) => Value = value;
    }
}
