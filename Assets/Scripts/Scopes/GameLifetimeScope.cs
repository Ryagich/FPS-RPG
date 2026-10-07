using Characters;
using Inventory.Ammo;
using Inventory.Pools;
using Inventory.Pools.Impact;
using MessagePipe;
using Messages;
using Player;
using Sounds;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [field: SerializeField] public PlayerLifetimeScope PlayerPrefab { get; private set; } = null!;

        [SerializeField] private Transform poolsParent;
        [SerializeField] private Transform soundsParent;
    
        private PlayerLifetimeScope playerScope;
        private GameSceneSessionConfiguration sceneSessionConfiguration = new(isGameplayScene: true);

        public void SetSceneSessionConfiguration(GameSceneSessionConfiguration configuration)
        {
            if (Container != null)
            {
                Debug.LogError("Game scene configuration must be assigned before GameLifetimeScope is built.", this);
                return;
            }

            sceneSessionConfiguration = configuration ?? new GameSceneSessionConfiguration(isGameplayScene: true);
        }

        protected override void Awake()
        {
            var projectScope = ProjectLifetimeScope.Instance;
            if (projectScope == null)
            {
                Debug.LogError("ProjectLifetimeScope was not created before GameLifetimeScope.", this);
                return;
            }

            parentReference.Object = projectScope;
            base.Awake();
        }
    
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(sceneSessionConfiguration);
            builder.Register<AmmoStorage>(Lifetime.Singleton).AsSelf();
            builder.Register<EntityTargets>(Lifetime.Singleton);
            builder.RegisterInstance(this).Keyed("GameScope");

            // === MessagePipe ===
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlaySoundMessage>(options);
        
            //pools
            builder.RegisterInstance(poolsParent).Keyed("PoolsParent");
            builder.RegisterInstance(soundsParent).Keyed("SoundsParent");
            builder.Register<ProjectilesPool>(Lifetime.Singleton).AsSelf();
            builder.RegisterEntryPoint<ImpactPools>().AsSelf();
            builder.RegisterEntryPoint<CasingPool>().AsSelf();
        
            builder.RegisterBuildCallback(container =>
            {
                GlobalMessagePipe.SetProvider(container.AsServiceProvider());
                playerScope = CreateChildFromPrefab(PlayerPrefab);
                container.Resolve<SoundsManager>().PlayerTransform = playerScope.transform;
            });
            builder.RegisterEntryPoint<SoundsManager>().AsSelf();
        }
    }
}
