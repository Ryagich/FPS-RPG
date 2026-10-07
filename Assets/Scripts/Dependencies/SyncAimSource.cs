using UnityEngine;

namespace Dependencies
{
    public readonly struct SyncAimSource
    {
        public Transform Value { get; }

        public SyncAimSource(Transform value) => Value = value;
    }
}
