using UnityEngine;

namespace Dependencies
{
    public readonly struct SyncRootTarget
    {
        public Transform Value { get; }

        public SyncRootTarget(Transform value) => Value = value;
    }
}
