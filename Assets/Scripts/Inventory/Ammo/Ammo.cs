namespace Inventory.Ammo
{
    public class Ammo : Stat
    {
        public AmmoConfig AmmoConfig { get; private set; }
        
        public Ammo(AmmoConfig ammoConfig, int value)
            : base(ammoConfig.Max, 0, value)
        {
            AmmoConfig = ammoConfig;
        }

        public Ammo(Ammo ammo)
            : base(ammo.Max, ammo.Min, ammo.Value)
        {
            AmmoConfig = ammo.AmmoConfig;
        }
        
        public Ammo(AmmoConfig ammoConfig)
            : base(ammoConfig.Max, 0, 0)
        {
            AmmoConfig = ammoConfig;
        }
    }
}