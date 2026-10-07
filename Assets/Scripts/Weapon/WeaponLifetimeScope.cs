using Dependencies;
using Scopes;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Animations;
using Weapon.Attachments;
using Weapon.Settings;

namespace Weapon
{
    public class WeaponLifetimeScope : EntityLifetimeScope
    {
        [field: SerializeField] public WeaponConfig Config { get; private set; }
        [field: SerializeField] public Transform CasingSpawnPoint { get; private set; }
        public Weapon Instance { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(Config).AsSelf();
            builder.RegisterInstance(transform).AsSelf();
            builder.RegisterInstance(gameObject).AsSelf();
            builder.RegisterInstance(new WeaponScope(this));
            builder.RegisterInstance(new CasingSpawnPoint(CasingSpawnPoint));

            builder.Register<AttachmentsController>(Lifetime.Scoped);
            builder.Register<WeaponPresentation>(Lifetime.Scoped);

            builder.RegisterEntryPoint<Weapon>().AsSelf();
            builder.RegisterEntryPoint<WeaponLowering>().AsSelf();
            builder.RegisterEntryPoint<WeaponKickBack>().AsSelf();
            builder.RegisterEntryPoint<WeaponSway>().AsSelf();
            builder.RegisterEntryPoint<WeaponBobbing>().AsSelf();
            builder.RegisterEntryPoint<WeaponRunBobbing>().AsSelf();
            builder.RegisterEntryPoint<WeaponJumpBobbing>().AsSelf();
            builder.RegisterEntryPoint<WeaponReloading>().AsSelf();
            
            builder.RegisterEntryPoint<CasingDropper>().AsSelf();
            builder.RegisterBuildCallback(container => Instance = container.Resolve<Weapon>());
        }
    }
}