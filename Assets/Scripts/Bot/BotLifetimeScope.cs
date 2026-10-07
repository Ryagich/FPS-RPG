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
        [SerializeField] private BotNavigationSettings navigationSettings = new();
        [field: SerializeField] public Transform ParentTransformForWeapon { get; private set; } = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var collider in bodyColliders)
            {
                if (collider != controller && !collider.isTrigger)
                    Physics.IgnoreCollision(controller, collider);
            }
            builder.RegisterCharacter(controller, transform, aimOrigin, ParentTransformForWeapon);
            builder.RegisterCharacterTargets(bodyColliders);
            builder.RegisterInstance(new RagdollBodies(ragdollBodies));
            // A null optional target is represented by the bot itself; demo Return uses its initial position.
            builder.RegisterInstance(new BotGoal(botGoal != null ? botGoal : transform));
            builder.RegisterInstance(navigationSettings);
            builder.Register<BotNavigation>(Lifetime.Scoped);
            builder.RegisterEntryPoint<BotControlSource>().AsSelf();
            builder.RegisterEntryPoint<CharacterMotor>().AsSelf();
            builder.RegisterEntryPoint<BotAimOrigin>().AsSelf();
            builder.RegisterEntryPoint<InteractionController>().AsSelf();
            builder.RegisterCharacterAnimation(animator);
            builder.RegisterEntryPoint<BotDeath>().AsSelf();
        }
    }
}
