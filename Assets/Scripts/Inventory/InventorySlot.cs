using System;

namespace Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InventorySlot
    {
        //was | now
        public event Action<IItem, IItem> ItemChanged = null!;
        public event Action<Ammo.Ammo, Ammo.Ammo> AmmoChanged = null!;
        public event Action Activated = null!;
        public event Action Disabled = null!;

        public IItem Item { get; private set; } = null!;
        public Ammo.Ammo Ammo { get; private set; } = null!;
        
        public InventorySlot() { }
        
        public InventorySlot(IItem item, Ammo.Ammo ammo)
        {
            SetItem(item);
            SetAmmo(ammo);
        }

        public void Activate()
        {
            Item.Activate();
            Activated?.Invoke();
        }

        public void Disable()
        {
            Item.Disable();
            Disabled?.Invoke();
        }

        public void SetItem(IItem item)
        {
            if (item is null)
                return;
            var was = Item;
            Item = item;
            ItemChanged?.Invoke(was, Item);
        }
        
        public void SetAmmo(Ammo.Ammo ammo)
        {
            if (ammo is null)
                return;
            var was = Ammo;
            Ammo = ammo;
            AmmoChanged?.Invoke(was, Ammo);
        }
    }
}