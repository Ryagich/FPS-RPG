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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(transform).Keyed("self");
            builder.RegisterComponentInHierarchy<NavMeshAgent>();
            builder.RegisterInstance(botGoal).Keyed("botGoal");
            builder.RegisterEntryPoint<BotController>().AsSelf();
            builder.RegisterComponent(botSight).AsImplementedInterfaces();
            builder.RegisterEntryPoint<BotAggro>().AsSelf();
        }
    }

}
