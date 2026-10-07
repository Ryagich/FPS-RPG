using UnityEngine;

namespace Dependencies
{
    public readonly struct RagdollBodies
    {
        public Rigidbody[] Value { get; }

        public RagdollBodies(Rigidbody[] value) => Value = value;
    }
}
