using CameraScripts;
using CameraScripts.Shake;
using CanvasScripts;
using Characters;
using Dependencies;
using Input;
using InteractableScripts;
using Scopes;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Player
{
    public class PlayerLifetimeScope : EntityLifetimeScope
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private Collider[] bodyColliders;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Animator animator;
        [field: SerializeField] public Transform CameraParentTransform { get; private set; } = null!;
        [field: SerializeField] public Transform ParentTransformForWeapon { get; private set; } = null!;
        [field: SerializeField] public SoundConfig MovementSoundConfig { get; private set; } = null!;
    
        private CanvasLifetimeScope canvasScope;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterCharacter(controller, transform,
                CameraParentTransform, ParentTransformForWeapon);
            builder.RegisterCharacterTargets(bodyColliders);
            builder.RegisterInstance(playerCamera);
            builder.RegisterInstance(new CameraParent(CameraParentTransform));
            builder.RegisterInstance(new MovementSoundSettings(MovementSoundConfig));
            builder.Register<CameraShakeOnStep>(Lifetime.Scoped);
            builder.Register<PlayerCamera>(Lifetime.Scoped);
            if (animator != null)
            {
                builder.RegisterCharacterAnimation(animator);
            }
            
            builder.RegisterBuildCallback(container =>
            {
                var canvasConfig = container.Resolve<CanvasConfig>();
                canvasScope = CreateChildFromPrefab(canvasConfig.CanvasPrefab);
                canvasScope.transform.SetParent(null);
                canvasScope.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            });

            builder.RegisterEntryPoint<PlayerInputSource>().AsSelf();
            builder.RegisterEntryPoint<CharacterMotor>().AsSelf();
            builder.RegisterEntryPoint<MovementSound>().AsSelf();
            builder.RegisterEntryPoint<CameraCrouch>().AsSelf();
            builder.RegisterEntryPoint<CameraFovController>().AsSelf();
            builder.RegisterEntryPoint<CameraRecoil>().AsSelf();
            builder.RegisterEntryPoint<CameraShaker>().AsSelf();
            builder.RegisterEntryPoint<CameraShakeOnRecoil>().AsSelf();
            builder.RegisterEntryPoint<CameraStepBobber>().AsSelf();
            
            builder.RegisterEntryPoint<InteractionController>().AsSelf();
        }
    }
}
