using Scopes;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon
{
    public sealed class ProjectileLifetimeScope : EntityLifetimeScope
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private Collider projectileCollider;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ProjectileCollisionRelay collisionRelay;
        [SerializeField] private ProjectileSettings settings;

        public Projectile Instance { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(body);
            builder.RegisterInstance(projectileCollider);
            builder.RegisterInstance(trail);
            builder.RegisterInstance(collisionRelay);
            builder.RegisterInstance(settings);
            builder.RegisterEntryPoint<Projectile>().AsSelf();
            builder.RegisterBuildCallback(container => Instance = container.Resolve<Projectile>());
        }
    }
}
