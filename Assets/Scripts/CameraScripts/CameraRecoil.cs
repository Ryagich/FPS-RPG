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
            ConfigureForItem(inventory.CurrentSlot?.Item);
            
            recoilMessageSubscriber.Subscribe(OnRequestRecoil);
        }
        
        private void OnRequestRecoil(RecoilMessage msg)
        {
            totalRecoil += new Vector2(Random.Range(-msg.Recoil.x, msg.Recoil.x), msg.Recoil.y);
        }
        
        private void OnSlotChanged(InventorySlot was, InventorySlot now)
        {
            ConfigureForItem(now.Item);
        }

        private void ConfigureForItem(IItem item)
        {
            settings = item is Weapon.Weapon weapon ? weapon.Config.ShakeSettings : null;
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
