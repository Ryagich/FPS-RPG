using System;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace CameraScripts.Shake
{
    public sealed class CameraShakeOnRecoil : IStartable, IDisposable
    {
        private readonly IDisposable subscription;

        public CameraShakeOnRecoil(CameraShaker shaker, ISubscriber<RecoilMessage> recoil)
        {
            subscription = recoil.Subscribe(message =>
            {
                var settings = message.ShakeSettings;
                if (settings == null)
                    return;
                var power = Mathf.Clamp(message.Recoil.magnitude, 0f, settings.maxShakePower);
                shaker.AddNoiseShake(settings.duration, settings.amplitude * power,
                    settings.frequency, settings.falloffCurve);
            });
        }
        
        public void Start() { }
        public void Dispose() => subscription.Dispose();
    }
}