using InteractableScripts;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Providers;
using Weapon.Settings;

namespace Weapon.Drop
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponAdderInInventory : IStartable
    {
        private readonly WeaponConfig weaponConfig;
        private readonly GameObject gameObject;

        public WeaponAdderInInventory
            (
                WeaponConfig weaponConfig,
                Interactable interactable,
                GameObject gameObject
            )
        {
            this.weaponConfig = weaponConfig;
            this.gameObject = gameObject;
            interactable.Interacted += AddWeaponInInventory;
        }

        private void AddWeaponInInventory(LifetimeScope scope)
        {
            var weaponProvider = scope.Container.Resolve<WeaponProvider>();

            weaponProvider.TakeNewWeapon(weaponConfig);
            Object.Destroy(gameObject);
        }

        public void Start() { }
    }
}