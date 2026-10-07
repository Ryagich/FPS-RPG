using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Weapon.Attachments
{
    public sealed class MuzzleLifetimeScope : AttachmentLifetimeScope
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; private set; }
        [field: SerializeField] public Transform ShotPoint { get; private set; }
        [field: FormerlySerializedAs("<particles>k__BackingField")]
        [field: SerializeField] public ParticleSystem Particles { get; private set; }
        [field: SerializeField] private Light flashLight;
        [field: SerializeField] private int flashParticlesCount = 5;
        [field: SerializeField] private float flashLightDuration;

        protected override void Configure(IContainerBuilder builder)
        {
            var muzzle = new Muzzle(transform, AttachmentBaseInfo, ShotPoint, Particles,
                flashLight, flashParticlesCount, flashLightDuration);
            Instance = muzzle;
            builder.RegisterEntryPoint<Muzzle>(_ => muzzle, Lifetime.Scoped).AsSelf();
        }
    }
}
