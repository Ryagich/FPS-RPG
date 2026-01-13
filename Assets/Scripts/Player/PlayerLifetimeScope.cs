using Camera;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Player
{
    public class PlayerLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public Transform CameraParentTransform { get; private set; } = null!;
        [field: SerializeField] public SoundConfig MovementSoundConfig { get; private set; } = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            var characterController = GetComponent<CharacterController>();
            builder.RegisterInstance(characterController).AsSelf();
            builder.RegisterInstance(transform).AsSelf();
            builder.RegisterInstance(CameraParentTransform).Keyed("CameraParentTransform");
            builder.RegisterInstance(MovementSoundConfig).Keyed("MovementSoundConfig");

            builder.Register<PlayerGravity>(Lifetime.Scoped);
            builder.Register<PlayerJump>(Lifetime.Scoped);
            builder.Register<PlayerMovement>(Lifetime.Scoped);
            
            // builder.RegisterEntryPoint<PlayerGravity>().AsSelf();
            // builder.RegisterEntryPoint<PlayerJump>().AsSelf();
            // builder.RegisterEntryPoint<PlayerMovement>().AsSelf();
            builder.RegisterEntryPoint<PlayerCamera>().AsSelf();
            builder.RegisterEntryPoint<PlayerMotor>().AsSelf();
            builder.RegisterEntryPoint<MovementSound>().AsSelf();
        }
    }
}
