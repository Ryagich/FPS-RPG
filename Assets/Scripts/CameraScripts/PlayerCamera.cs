using Characters;
using Dependencies;
using UnityEngine;

namespace CameraScripts
{
    public sealed class PlayerCamera
    {
        private readonly CharacterLook look;
        public Transform CameraParentTransform { get; }
        
        public PlayerCamera(CharacterLook look, CameraParent cameraParentTransformReference)
        {
            var cameraParentTransform = cameraParentTransformReference.Value;
            this.look = look;
            CameraParentTransform = cameraParentTransform;
        }

        public void AddRotation(Vector2 delta) => look.Rotate(delta);
    }
}