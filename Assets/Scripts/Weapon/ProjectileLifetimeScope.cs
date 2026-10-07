using Scopes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Weapon
{
    public sealed class ProjectileLifetimeScope : EntityLifetimeScope
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private Collider projectileCollider;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ProjectileCollisionRelay collisionRelay;
        [SerializeField, Min(0f)] private float _timeToDeath = 2f;
        [SerializeField, Min(0f)] private float distanceToUseShotPointRotation = 0.5f;
        [SerializeField, Min(0f)] private float trailTime = 0.2f;

        public Projectile Instance { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(body);
            builder.RegisterInstance(projectileCollider);
            builder.RegisterInstance(trail);
            builder.RegisterInstance(collisionRelay);
            builder.RegisterInstance(new ProjectileSettings(_timeToDeath, distanceToUseShotPointRotation, trailTime));
            builder.RegisterEntryPoint<Projectile>().AsSelf();
            builder.RegisterBuildCallback(container => Instance = container.Resolve<Projectile>());
        }
    }

    public sealed class ProjectileSettings
    {
        public float Lifetime { get; }
        public float NormalDistance { get; }
        public float TrailTime { get; }

        public ProjectileSettings(float lifetime, float normalDistance, float trailTime)
        {
            Lifetime = lifetime;
            NormalDistance = normalDistance;
            TrailTime = trailTime;
        }
    }
}
