using VContainer;
using Characters;
using UnityEngine;

namespace CameraScripts
{
    public sealed class PlayerCamera
    {
        private readonly CharacterLook look;
        public Transform CameraParentTransform { get; }
        
        public PlayerCamera(CharacterLook look, [Key("CameraParentTransform")] Transform cameraParentTransform)
        {
            this.look = look;
            CameraParentTransform = cameraParentTransform;
        }

        public void AddRotation(Vector2 delta) => look.Rotate(delta);
    }
}