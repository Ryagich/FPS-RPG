using UnityEngine;

namespace Dependencies
{
    public readonly struct CasingPrefab
    {
        public Rigidbody Value { get; }

        public CasingPrefab(Rigidbody value) => Value = value;
    }
}
