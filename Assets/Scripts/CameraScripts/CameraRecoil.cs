using System;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace CameraScripts
{
    internal sealed class CameraRecoil : ITickable, IDisposable
    {
        private readonly PlayerCamera camera;
        private readonly IDisposable subscription;
        private Vector2 recoil;
        private ShakeSettings settings;
        
        public CameraRecoil(PlayerCamera camera, ISubscriber<RecoilMessage> recoilSubscriber)
        {
            this.camera = camera;
            subscription = recoilSubscriber.Subscribe(message =>
            {
                settings = message.ShakeSettings;
                recoil += new Vector2(UnityEngine.Random.Range(-message.Recoil.x, message.Recoil.x), message.Recoil.y);
            });
        }

        public void Tick()
        {
            if (settings == null)
                return;
            var fraction = 1f - Mathf.Pow(1f - Mathf.Clamp01(settings.RecoilMultiplier), Time.deltaTime * 60f);
            var delta = recoil * fraction;
            recoil -= delta;
            camera.AddRotation(delta);
        }

        public void Dispose() => subscription.Dispose();
    }
}