using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    public class BotLifetimeScope : LifetimeScope
    {
        [SerializeField] private BotSight botSight;
        [SerializeField] private Transform botGoal;
        [SerializeField] private Transform visionOrigin;
        [SerializeField] private Transform spine;
        [SerializeField] private Collider visibleCollider;
        [SerializeField] private Transform hips;

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<BotVisionMessage>(options);

            var agent = GetComponent<NavMeshAgent>();

            builder.RegisterInstance(transform).Keyed("self");
            builder.RegisterInstance(agent).AsSelf();
            builder.RegisterInstance(botGoal).Keyed("botGoal");
            builder.RegisterInstance(visibleCollider).Keyed("collider");
            builder.RegisterInstance(visionOrigin).Keyed("visionOrigin");
            builder.RegisterInstance(spine).Keyed("spine");
            builder.RegisterInstance(hips).Keyed("hips");
            builder.RegisterEntryPoint<BotController>().AsSelf();
            builder.RegisterComponent(botSight).AsImplementedInterfaces();
            builder.RegisterEntryPoint<BotAggro>().AsSelf();
        }
    }

}
