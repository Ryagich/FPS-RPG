using System;
using Characters;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace CameraScripts
{
    public sealed class CameraFovController : ITickable, IDisposable
    {
        private readonly CameraFovConfig config;
        private readonly Camera camera;
        private readonly float defaultFov;
        private readonly IDisposable subscription;
        private float targetFov;

        public CameraFovController(CameraFovConfig config, CharacterState state, Camera camera)
        {
            this.config = config;
            this.camera = camera;
            defaultFov = camera.fieldOfView;
            subscription = state.IsSprinting.Subscribe(sprinting => targetFov = sprinting ? config.RunFov : defaultFov);
        }

        public void Tick() => camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFov,
            Time.deltaTime * config.ChangeToRunSpeed);
        public void Dispose() => subscription.Dispose();
    }
}