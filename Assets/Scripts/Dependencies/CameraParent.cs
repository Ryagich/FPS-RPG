using UnityEngine;

namespace Dependencies
{
    public readonly struct CameraParent
    {
        public Transform Value { get; }

        public CameraParent(Transform value) => Value = value;
    }
}
