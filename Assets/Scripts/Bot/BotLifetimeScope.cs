using Characters;
using Dependencies;
using InteractableScripts;
using Scopes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    public sealed class BotLifetimeScope : EntityLifetimeScope
    {
        [SerializeField] private Transform botGoal;
        [SerializeField] private CharacterController controller;
        [SerializeField] private Collider[] bodyColliders;
        [SerializeField] private Rigidbody[] ragdollBodies;
        [SerializeField] private Transform aimOrigin;
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterAnimationConfig animationConfig;
        [SerializeField] private BotNavigationSettings navigationSettings;
        [SerializeField] private BotControlProfile controlProfile;
        [SerializeField] private Transform parentTransformForWeapon;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterCharacter(controller, transform, aimOrigin, parentTransformForWeapon);
            builder.RegisterCharacterTargets(bodyColliders);
            builder.RegisterInstance(new RagdollBodies(ragdollBodies));
            builder.RegisterInstance(new BotGoal(botGoal));
            builder.RegisterInstance(navigationSettings);
            builder.Register<BotNavigation>(Lifetime.Scoped);
            controlProfile.Register(builder);
            builder.RegisterEntryPoint<CharacterMotor>().AsSelf();
            builder.RegisterEntryPoint<BotAimOrigin>().AsSelf();
            builder.RegisterEntryPoint<InteractionController>().AsSelf();
            builder.RegisterCharacterAnimation(animator, animationConfig);
            builder.RegisterEntryPoint<BotDeath>().AsSelf();
        }
    }
}
