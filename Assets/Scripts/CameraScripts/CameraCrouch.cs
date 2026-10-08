using Characters;
using Dependencies;
using UnityEngine;
using VContainer.Unity;

namespace CameraScripts
{
    public sealed class CameraCrouch : ITickable
    {
        private readonly CharacterState state;
        private readonly CharacterMovementConfig config;
        private readonly Transform camera;
        private readonly float standingY;
        private float appliedY;

        public CameraCrouch(CharacterState state, CharacterMovementConfig config,
            CameraParent cameraReference)
        {
            var camera = cameraReference.Value;
            this.state = state;
            this.config = config;
            this.camera = camera;
            standingY = camera.localPosition.y;
        }

        public void Tick()
        {
            var offset = (config.CameraPositionInCrouching - standingY) * state.CrouchProgress;
            camera.localPosition += Vector3.up * (offset - appliedY);
            appliedY = offset;
        }
    }
}
