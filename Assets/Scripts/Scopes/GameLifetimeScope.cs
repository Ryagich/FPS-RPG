using Cysharp.Threading.Tasks;
using Inventory.Ammo;
using Inventory.Pools;
using Inventory.Pools.Impact;
using Localization;
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

            // === MessagePipe ===
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlaySoundMessage>(options);
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
            builder.RegisterMessageBroker<InteractableMessage>(options);
            builder.RegisterMessageBroker<AimChangedMessage>(options);
        
            //pools
            var pools = new GameObject("Pools");
            builder.RegisterInstance(pools.transform).Keyed("PoolsParent");
            builder.Register<ProjectilesPool>(Lifetime.Singleton).AsSelf();
            builder.RegisterEntryPoint<ImpactPools>().AsSelf();
            builder.RegisterEntryPoint<CasingPool>().AsSelf();
        
            //TODO: подкастылю создание игрока, братья, после того как загрузятся ресы из проэкта.
            builder.RegisterBuildCallback(container =>
                                          {
                                              GlobalMessagePipe.SetProvider(container.AsServiceProvider());

                                              playerScope = CreateChildFromPrefab(PlayerPrefab);
                                              container.Resolve<SoundsManager>().PlayerTransform = playerScope.transform;
                                              // var ammoStorage = container.Resolve<AmmoStorage>();
                                              //
                                              // UniTask.Void(async () =>
                                              //              {
                                              //                  await ammoStorage.WaitUntilReady();
                                              //
                                              //                  playerScope = CreateChildFromPrefab(PlayerPrefab);
                                              //                  container.Resolve<SoundsManager>().PlayerTransform = playerScope.transform;
                                              //              });
                                          });
            builder.RegisterEntryPoint<SoundsManager>().AsSelf();
        }
    }
}
