using Camera;
using Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Player
{
    public class PlayerLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public Transform CameraParentTransform { get; private set; } = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            var characterController = GetComponent<CharacterController>();
            builder.RegisterInstance(characterController).AsSelf();
            builder.RegisterInstance(transform).AsSelf();
            builder.RegisterInstance(CameraParentTransform).Keyed("CameraParentTransform");

            builder.RegisterEntryPoint<PlayerMovement>().AsSelf();
            builder.RegisterEntryPoint<PlayerCamera>().AsSelf();
            builder.RegisterEntryPoint<PlayerGravity>().AsSelf();
        }
    }
}
