using CameraScripts;
using CameraScripts.Shake;
using CanvasScripts;
using Input;
using InteractableScripts;
using MessagePipe;
using Messages;
using Movement;
using Player.Stats;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon.Providers;

namespace Player
{
    public class PlayerLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public Transform CameraParentTransform { get; private set; } = null!;
        [field: SerializeField] public Transform ParentTransformForWeapon { get; private set; } = null!;
        [field: SerializeField] public SoundConfig MovementSoundConfig { get; private set; } = null!;
    
        protected override void Configure(IContainerBuilder builder)
        {
            var characterController = GetComponent<CharacterController>();
            var avatarView = GetComponent<PlayerAvatarView>();
            if (avatarView == null)
                throw new MissingComponentException("Player prefab requires PlayerAvatarView.");
            var cameraSocket = GetComponent<PlayerCameraSocket>();
            if (cameraSocket == null)
                throw new MissingComponentException("Player prefab requires PlayerCameraSocket.");
            builder.RegisterInstance(characterController).AsSelf();
            builder.RegisterInstance(avatarView).AsSelf();
            builder.RegisterInstance(cameraSocket).AsSelf();
            builder.RegisterInstance(transform).AsSelf();
            builder.RegisterInstance<LifetimeScope>(this).Keyed("InventoryOwnerScope");
            builder.RegisterInstance(true).Keyed("HasMeleeSlot");
            builder.RegisterInstance(CameraParentTransform).Keyed("CameraParentTransform");
            builder.RegisterInstance(ParentTransformForWeapon).Keyed("ParentTransformForWeapon");
            builder.RegisterInstance(MovementSoundConfig).Keyed("MovementSoundConfig");
            
            builder.Register<PlayerGravity>(Lifetime.Scoped);
            builder.Register<PlayerJump>(Lifetime.Scoped);
            builder.Register<PlayerMovement>(Lifetime.Scoped);
            builder.Register<MoveStates>(Lifetime.Scoped);
            builder.Register<WeaponProvider>(Lifetime.Scoped);
            builder.Register<CameraShakeOnStep>(Lifetime.Scoped).AsSelf();
            builder.Register<StatsController>(Lifetime.Scoped).AsSelf();
            builder.Register<IMovementDataProvider>(c =>
                                                        new CharacterControllerMovementProvider(
                                                             c.Resolve<CharacterController>(),
                                                             c.Resolve<Transform>()
                                                            ),
                                                    Lifetime.Scoped
                                                   );

            // Character death is local to this player scope; input messages are inherited
            // from the parent GameLifetimeScope.
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<DeathMessage>(options);
            
            builder.RegisterEntryPoint<InputHandler>().AsSelf();
            builder.RegisterEntryPoint<PlayerMotor>().AsSelf();
            builder.RegisterEntryPoint<PlayerPoseController>().AsSelf();
            builder.RegisterEntryPoint<PlayerAnimationDriver>().AsSelf();
            builder.RegisterEntryPoint<MovementSound>().AsSelf();
            builder.RegisterEntryPoint<PlayerCamera>().AsSelf();
            builder.RegisterEntryPoint<KinemationLookInputBridge>().AsSelf();
            builder.RegisterEntryPoint<CameraFovController>().AsSelf();
            builder.RegisterEntryPoint<CameraRecoil>().AsSelf();
            builder.RegisterEntryPoint<CameraShaker>().AsSelf();
            builder.RegisterEntryPoint<CameraShakeOnRecoil>().AsSelf();
            builder.RegisterEntryPoint<CameraStepBobber>().AsSelf();
            
            builder.RegisterEntryPoint<Inventory.Inventory>().AsSelf();
            builder.RegisterEntryPoint<InteractableFounder>().AsSelf();
            builder.RegisterEntryPoint<InputProvider>().AsSelf();
        }
    }
}
