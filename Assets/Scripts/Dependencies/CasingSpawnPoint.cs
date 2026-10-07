using UnityEngine;

namespace Dependencies
{
    public readonly struct CasingSpawnPoint
    {
        public Transform Value { get; }

        public CasingSpawnPoint(Transform value) => Value = value;
    }
}
