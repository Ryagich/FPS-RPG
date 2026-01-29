using Inventory;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace CameraScripts.Shake
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CameraShakeOnRecoil : IStartable
    {
        private readonly CameraShaker cameraShaker;
        private ShakeSettings settings;

        public CameraShakeOnRecoil(
                Inventory.Inventory inventory,
                CameraShaker cameraShaker,
                ISubscriber<RecoilMessage> recoilSubscriber
            )
        {
            this.cameraShaker = cameraShaker;

            inventory.SlotChanged += OnSlotChanged;
            if (inventory.CurrentSlot != null)
                settings = ((Weapon.Weapon)inventory.CurrentSlot.Item).Config.ShakeSettings;

            recoilSubscriber.Subscribe(OnRecoil);
        }

        private void OnSlotChanged(InventorySlot was, InventorySlot now)
        {
            settings = ((Weapon.Weapon)now.Item).Config.ShakeSettings;
        }

        private void OnRecoil(RecoilMessage msg)
        {
            if (settings == null)
                return;

            var power = Mathf.Clamp(msg.Recoil.magnitude, 0f, settings.maxShakePower);

            cameraShaker.AddNoiseShake(
                                       duration: settings.duration,
                                       amplitude: settings.amplitude * power,
                                       frequency: settings.frequency,
                                       falloff: settings.falloffCurve
                                      );
        }
        
        public void Start() { }
    }
}