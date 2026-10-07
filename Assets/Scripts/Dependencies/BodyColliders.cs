using UnityEngine;

namespace Dependencies
{
    public readonly struct BodyColliders
    {
        public Collider[] Value { get; }

        public BodyColliders(Collider[] value) => Value = value;
    }
}
