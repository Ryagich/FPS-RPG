using UnityEngine;

namespace Dependencies
{
    public readonly struct InteractionColliders
    {
        public Collider[] Value { get; }

        public InteractionColliders(Collider[] value) => Value = value;
    }
}
