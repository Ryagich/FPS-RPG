using UnityEngine;

namespace Dependencies
{
    public readonly struct WeaponParent
    {
        public Transform Value { get; }

        public WeaponParent(Transform value) => Value = value;
    }
}
