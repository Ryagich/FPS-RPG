using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Weapon.Melee
{
    public sealed class MeleeWeaponLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public MeleeWeaponConfig Config { get; private set; } = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(Config).AsSelf();
            builder.RegisterInstance(gameObject).AsSelf();
            builder.Register<MeleeWeapon>(Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
        }
    }
}
