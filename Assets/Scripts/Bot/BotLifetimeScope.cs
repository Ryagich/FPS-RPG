using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace Bot
{
    public class BotLifetimeScope : LifetimeScope
    {
        [SerializeField] private BotSight botSight;
        [SerializeField] private Transform botGoal;
        [SerializeField] private Transform visionOrigin;
        [SerializeField] private Transform spine;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(transform).Keyed("self");
            builder.RegisterComponentInHierarchy<NavMeshAgent>();
            builder.RegisterInstance(botGoal).Keyed("botGoal");
            builder.RegisterInstance(visionOrigin).Keyed("visionOrigin");
            builder.RegisterInstance(spine).Keyed("spine");
            builder.RegisterEntryPoint<BotController>().AsSelf();
            builder.RegisterComponent(botSight).AsImplementedInterfaces();
            builder.RegisterEntryPoint<BotAggro>().AsSelf();
        }
    }

}
