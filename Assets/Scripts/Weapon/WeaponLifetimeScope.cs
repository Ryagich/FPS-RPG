using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Animations;
using Weapon.Attachments;
using Weapon.Settings;

namespace Weapon
{
    public class WeaponLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public WeaponConfig Config { get; private set; }
        [field: SerializeField] public Transform CasingSpawnPoint { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(Config).AsSelf();
            builder.RegisterInstance(transform).AsSelf();
            builder.RegisterInstance(gameObject).AsSelf();
            builder.RegisterInstance(CasingSpawnPoint).Keyed($"CasingSpawnPoint").AsSelf();

            builder.RegisterEntryPoint<WeaponLowering>().AsSelf();
            builder.RegisterEntryPoint<WeaponKickBack>().AsSelf();
            builder.RegisterEntryPoint<WeaponSway>().AsSelf();
            builder.RegisterEntryPoint<WeaponBobbing>().AsSelf();
            builder.RegisterEntryPoint<WeaponRunBobbing>().AsSelf();
            builder.RegisterEntryPoint<WeaponJumpBobbing>().AsSelf();
            builder.RegisterEntryPoint<WeaponReloading>().AsSelf();

            // builder.Register<WeaponLowering>(Lifetime.Scoped);
            // builder.Register<WeaponKickBack>(Lifetime.Scoped);
            // builder.Register<WeaponSway>(Lifetime.Scoped);
            // builder.Register<WeaponBobbing>(Lifetime.Scoped);
            // builder.Register<WeaponRunBobbing>(Lifetime.Scoped);
            // builder.Register<WeaponJumpBobbing>(Lifetime.Scoped);
            builder.Register<AttachmentsController>(Lifetime.Scoped);
            
            builder.RegisterEntryPoint<Weapon>().AsSelf();
            builder.RegisterEntryPoint<CasingDropper>().AsSelf();
        }
    }
}