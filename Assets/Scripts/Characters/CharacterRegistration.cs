using Dependencies;
using MessagePipe;
using Messages;
using Movement;
using Player.Stats;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Weapon;
using Weapon.Providers;

namespace Characters
{
    public static class CharacterRegistration
    {
        public static MessagePipeOptions RegisterCharacter(this IContainerBuilder builder,
            CharacterController controller, Transform body, Transform aim, Transform weaponParent)
        {
            var options = builder.RegisterMessagePipe(configuration => configuration.InstanceLifetime = InstanceLifetime.Scoped);
            builder.RegisterMessageBroker<MoveCommand>(options);
            builder.RegisterMessageBroker<LookCommand>(options);
            builder.RegisterMessageBroker<SprintCommand>(options);
            builder.RegisterMessageBroker<CrouchCommand>(options);
            builder.RegisterMessageBroker<JumpCommand>(options);
            builder.RegisterMessageBroker<FireCommand>(options);
            builder.RegisterMessageBroker<AimCommand>(options);
            builder.RegisterMessageBroker<SwitchWeaponCommand>(options);
            builder.RegisterMessageBroker<ReloadCommand>(options);
            builder.RegisterMessageBroker<SwitchFireModeCommand>(options);
            builder.RegisterMessageBroker<InteractCommand>(options);
            builder.RegisterMessageBroker<AimChangedMessage>(options);
            builder.RegisterMessageBroker<RecoilMessage>(options);
            builder.RegisterMessageBroker<ShotFiredMessage>(options);
            builder.RegisterMessageBroker<ReloadStartedMessage>(options);
            builder.RegisterMessageBroker<ReloadFinishedMessage>(options);
            builder.RegisterMessageBroker<WeaponChangeStartedMessage>(options);
            builder.RegisterMessageBroker<WeaponChangeFinishedMessage>(options);
            builder.RegisterMessageBroker<DeathMessage>(options);

            builder.RegisterInstance(controller);
            builder.RegisterInstance(body);
            builder.RegisterInstance(new AimTransform(aim));
            builder.RegisterInstance(new WeaponParent(weaponParent));
            builder.Register<CharacterState>(Lifetime.Scoped);
            builder.Register<CharacterMovementCommands>(Lifetime.Scoped);
            builder.Register<CharacterLook>(Lifetime.Scoped);
            builder.Register<CharacterMovement>(Lifetime.Scoped);
            builder.Register<WeaponProvider>(Lifetime.Scoped);
            builder.Register<StatsController>(Lifetime.Scoped);
            builder.Register<IMovementDataProvider>(container => new CharacterControllerMovementProvider(
                container.Resolve<CharacterController>(), container.Resolve<Transform>()), Lifetime.Scoped);
            builder.RegisterEntryPoint<Inventory.Inventory>().AsSelf();
            builder.RegisterEntryPoint<CharacterWeaponController>().AsSelf();
            // Register the input source before CharacterMotor in each scope.
            return options;
        }

        public static void RegisterCharacterAnimation(this IContainerBuilder builder, Animator animator)
        {
            builder.RegisterInstance(animator);
            builder.RegisterEntryPoint<CharacterAnimation>().AsSelf();
        }

        public static void RegisterCharacterTargets(this IContainerBuilder builder, Collider[] colliders)
        {
            builder.RegisterInstance(new BodyColliders(colliders));
            builder.RegisterEntryPoint<CharacterTargets>().AsSelf();
        }
    }
}
