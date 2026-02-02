using MessagePipe;
using Messages;
using Movement;
using Player.Stats;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;
using Weapon.Providers;

namespace Bot
{
    public class BotLifetimeScope : LifetimeScope
    {
        [SerializeField] private BotSight botSight;
        [SerializeField] private Transform botGoal;
        [SerializeField] private Transform visionOrigin;
        [SerializeField] private Transform spine;
        [SerializeField] private Transform hips;
        [field: SerializeField] public Transform ParentTransformForWeapon { get; private set; } = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            // === MessagePipe ===
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<BotVisionMessage>(options);
            builder.RegisterMessageBroker<LookDeltaMessage>(options);
            builder.RegisterMessageBroker<DeathMessage>(options);
            
            var agent = GetComponent<NavMeshAgent>();
            builder.RegisterInstance(agent).AsSelf();

            builder.Register<IMovementDataProvider>(c =>
                                                        new NavMeshAgentMovementProvider(
                                                             c.Resolve<NavMeshAgent>(),
                                                             c.Resolve<Transform>()
                                                            ),
                                                    Lifetime.Scoped
                                                   );
            builder.Register<WeaponProvider>(Lifetime.Scoped);
            builder.Register<StatsController>(Lifetime.Scoped).AsSelf();

            builder.RegisterInstance(transform).Keyed("self");
            builder.RegisterInstance(botGoal).Keyed("botGoal");
            builder.RegisterInstance(visionOrigin).Keyed("visionOrigin");
            builder.RegisterInstance(spine).Keyed("spine");
            builder.RegisterInstance(hips).Keyed("hips");
            builder.RegisterInstance(ParentTransformForWeapon).Keyed("ParentTransformForWeapon");

            builder.RegisterComponent(botSight).AsImplementedInterfaces();
            
            builder.RegisterEntryPoint<BotController>().AsSelf();
            builder.RegisterEntryPoint<BotAggro>().AsSelf();

            builder.RegisterEntryPoint<Inventory.Inventory>().AsSelf();
            builder.RegisterEntryPoint<BotDeath>().AsSelf();
        }
    }
}
