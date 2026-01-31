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
    
        protected override void Configure(IContainerBuilder builder)
        {
            // === MessagePipe ===
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<PlaySoundMessage>(options);
            builder.RegisterMessageBroker<PlayerMoveMessage>(options);
            builder.RegisterMessageBroker<LookDeltaMessage>(options);
            builder.RegisterMessageBroker<ClickMessage>(options);
            builder.RegisterMessageBroker<RightClickMessage>(options);
        
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
            builder.RegisterEntryPoint<Bootloader>().AsSelf().As<IStartable>();
        }
    }
}