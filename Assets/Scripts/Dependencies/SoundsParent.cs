using UnityEngine;

namespace Dependencies
{
    public readonly struct SoundsParent
    {
        public Transform Value { get; }

        public SoundsParent(Transform value) => Value = value;
    }
}
