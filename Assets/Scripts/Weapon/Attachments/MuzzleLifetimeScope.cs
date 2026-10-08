using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Weapon.Attachments
{
    public sealed class MuzzleLifetimeScope : AttachmentLifetimeScope
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; private set; }
        [field: SerializeField] public Transform ShotPoint { get; private set; }
        [field: SerializeField] public ParticleSystem Particles { get; private set; }
        [field: SerializeField] private Light flashLight;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(AttachmentBaseInfo);
            var muzzle = new Muzzle(transform, AttachmentBaseInfo, ShotPoint, Particles,
                flashLight);
            Instance = muzzle;
            builder.RegisterEntryPoint<Muzzle>(_ => muzzle, Lifetime.Scoped).AsSelf();
        }
    }
}
