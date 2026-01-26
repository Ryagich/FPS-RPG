using Camera;
using Camera.Shake;
using Input;
using MessagePipe;
using Messages;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;

namespace Player
{
    public class PlayerLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public Transform CameraParentTransform { get; private set; } = null!;
        [field: SerializeField] public Transform ParentTransformForWeapon { get; private set; } = null!;
        [field: SerializeField] public SoundConfig MovementSoundConfig { get; private set; } = null!;
        [field: SerializeField] public WeaponConfig TestWeaponConfig1 { get; private set; } = null!;
        [field: SerializeField] public WeaponConfig TestWeaponConfig2 { get; private set; } = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            var characterController = GetComponent<CharacterController>();
            builder.RegisterInstance(characterController).AsSelf();
            builder.RegisterInstance(transform).AsSelf();
            builder.RegisterInstance(CameraParentTransform).Keyed("CameraParentTransform");
            builder.RegisterInstance(ParentTransformForWeapon).Keyed("ParentTransformForWeapon");
            builder.RegisterInstance(MovementSoundConfig).Keyed("MovementSoundConfig");
            builder.RegisterInstance(TestWeaponConfig1).Keyed("TestWeaponConfig1");
            builder.RegisterInstance(TestWeaponConfig2).Keyed("TestWeaponConfig2");
            
            builder.Register<PlayerGravity>(Lifetime.Scoped);
            builder.Register<PlayerJump>(Lifetime.Scoped);
            builder.Register<PlayerMovement>(Lifetime.Scoped);
            // builder.Register<Inventory.Inventory>(Lifetime.Scoped);
            builder.Register<WeaponProvider>(Lifetime.Scoped);
            builder.Register<CameraShakeOnStep>(Lifetime.Scoped).AsSelf();
 
            // === MessagePipe ===
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlayerMoveMessage>(options);
            builder.RegisterMessageBroker<LookDeltaMessage>(options);
            builder.RegisterMessageBroker<ClickMessage>(options);
            builder.RegisterMessageBroker<RightClickMessage>(options);
            builder.RegisterMessageBroker<JumpMessage>(options);
            builder.RegisterMessageBroker<ChangeSprintStateMessage>(options);
            builder.RegisterMessageBroker<ChangeCrouchingStateMessage>(options);
            builder.RegisterMessageBroker<SwitchWeaponMessage>(options);
            builder.RegisterMessageBroker<ReloadingMessage>(options);
            builder.RegisterMessageBroker<SwitchFireMode>(options);
            
            // === InputHandler ===
            // builder.Register<InputHandler>(Lifetime.Singleton).AsSelf().As<IStartable>().As<ITickable>();
            
            builder.RegisterEntryPoint<InputHandler>().AsSelf();
            builder.RegisterEntryPoint<PlayerCamera>().AsSelf();
            builder.RegisterEntryPoint<PlayerMotor>().AsSelf();
            builder.RegisterEntryPoint<MovementSound>().AsSelf();
            builder.RegisterEntryPoint<CameraFovController>().AsSelf();
            builder.RegisterEntryPoint<CameraRecoil>().AsSelf();
            builder.RegisterEntryPoint<CameraShaker>().AsSelf();
            builder.RegisterEntryPoint<CameraShakeOnRecoil>().AsSelf();
            builder.RegisterEntryPoint<CameraStepBobber>().AsSelf();
            
            builder.RegisterEntryPoint<Inventory.Inventory>().AsSelf();
            builder.RegisterEntryPoint<PlayerSetterInSoundsManager>().AsSelf();
        }
    }
}
