using UnityEngine;

namespace Dependencies
{
    public readonly struct SyncRootSource
    {
        public Transform Value { get; }

        public SyncRootSource(Transform value) => Value = value;
    }
}
