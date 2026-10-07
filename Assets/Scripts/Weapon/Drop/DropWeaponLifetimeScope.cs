using InteractableScripts;
using Scopes;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Drop
{
    public class DropWeaponLifetimeScope : EntityLifetimeScope
    {
        [field: SerializeField] public WeaponConfig WeaponConfig { get; private set; }
        
        [SerializeField] private Collider[] interactionColliders;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(WeaponConfig).AsSelf();
            builder.RegisterInstance(gameObject).AsSelf();

            builder.Register<Interactable>(Lifetime.Scoped);
            builder.RegisterInstance(interactionColliders).Keyed("InteractionColliders");
            builder.RegisterEntryPoint<InteractableRegistration>().AsSelf();
            
            builder.RegisterEntryPoint<WeaponAdderInInventory>().AsSelf();
        }
    }
}