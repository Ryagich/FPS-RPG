using UnityEngine;

namespace Dependencies
{
    public readonly struct PoolsParent
    {
        public Transform Value { get; }

        public PoolsParent(Transform value) => Value = value;
    }
}
