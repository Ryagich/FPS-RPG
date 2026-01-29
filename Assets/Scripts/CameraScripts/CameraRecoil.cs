using Inventory;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace CameraScripts
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class CameraRecoil : ITickable
    {
        private readonly PlayerCamera playerCamera;
        private Vector2 totalRecoil;
        private ShakeSettings settings;
        
        // [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public CameraRecoil
            (
                Inventory.Inventory inventory, 
                PlayerCamera playerCamera,
                ISubscriber<RecoilMessage> recoilMessageSubscriber
            )
        {
            this.playerCamera = playerCamera;
            
            inventory.SlotChanged += OnSlotChanged;
            if (inventory.CurrentSlot != null!)
            {
                var weapon = (Weapon.Weapon)inventory.CurrentSlot.Item;
                settings = weapon.Config.ShakeSettings;
            }
            
            recoilMessageSubscriber.Subscribe(OnRequestRecoil);
        }
        
        private void OnRequestRecoil(RecoilMessage msg)
        {
            totalRecoil += new Vector2(Random.Range(-msg.Recoil.x, msg.Recoil.x), msg.Recoil.y);
        }
        
        private void OnSlotChanged(InventorySlot was, InventorySlot now)
        {
            settings = ((Weapon.Weapon)now.Item).Config.ShakeSettings;
        }

        public void Tick()
        {
            if (settings == null)
            {
                return;
            }
            var deltaRecoil = totalRecoil * settings.RecoilMultiplier;
            totalRecoil -= deltaRecoil;
            playerCamera.RotateX(deltaRecoil);
        }
    }
}