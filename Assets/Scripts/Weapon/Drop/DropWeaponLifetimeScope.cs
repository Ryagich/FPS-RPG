using InteractableScripts;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Drop
{
    public class DropWeaponLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public WeaponConfig WeaponConfig { get; private set; }
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(WeaponConfig).AsSelf();
            builder.RegisterInstance(gameObject).AsSelf();

            var interactable = gameObject.AddComponent<Interactable>();
            
            builder.RegisterInstance(interactable);
            
            builder.RegisterEntryPoint<WeaponAdderInInventory>().AsSelf();
        }
    }
}