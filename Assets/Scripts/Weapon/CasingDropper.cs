using Inventory.Pools;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CasingDropper : IStartable
    {
        private readonly WeaponConfig weaponConfig;
        private readonly CasingPool casingPool;
        private readonly Transform casingSpawnPoint;

        public CasingDropper
            (
                WeaponConfig weaponConfig, 
                Weapon weapon,
                CasingPool casingPool,
                [Key("CasingSpawnPoint")] Transform casingSpawnPoint
            )
        {
            this.weaponConfig = weaponConfig;
            this.casingPool = casingPool;
            this.casingSpawnPoint = casingSpawnPoint;
            
            weapon.Shooted += DropCasing;
        }
        
        // Тип оружия	            forceRange (м/с)	ejectTorque (Н·м)	coneAngle (°)	lifeTime (с)
        // Пистолет	                    (1.5, 2.5)	        0.5 – 1.0         12 – 15	        3 – 5
        // Штурмовая винтовка	        (2.0, 3.5)	        1.0 – 2.0	      15 – 20	        4 – 6
        // Снайперская винтовка	        (2.5, 4.0)	        1.5 – 2.5	      18 – 25	        5 – 7
        // Дробовик	                    (3.0, 5.0)	        2.0 – 3.0         20 – 30	        4 – 6
        private void DropCasing()
        {
            casingPool.GetCasing(casingSpawnPoint.position, casingSpawnPoint.rotation,
                                 weaponConfig.CasingProperties.ForceRange,
                                 weaponConfig.CasingProperties.EjectTorque,
                                 weaponConfig.CasingProperties.ConeAngle);
        }

        public void Start() { }
    }
}