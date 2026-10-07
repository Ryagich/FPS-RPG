using UnityEngine;

namespace Dependencies
{
    public readonly struct SyncAimTarget
    {
        public Transform Value { get; }

        public SyncAimTarget(Transform value) => Value = value;
    }
}
