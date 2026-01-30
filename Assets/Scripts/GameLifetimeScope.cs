using Camera;
using Cysharp.Threading.Tasks;
﻿using Bot;
using Gravity;
using Input;
using Inventory;
using Inventory.Ammo;
using Inventory.Pools;
using Inventory.Pools.Impact;
using Localization;
using MessagePipe;
using Messages;
using Player;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [field: SerializeField] public InputConfig InputConfig { get; private set; } = null!;
    [field: SerializeField] public PlayerMovementConfig PlayerMovementConfig { get; private set; } = null!;
    [field: SerializeField] public GravityConfig GravityConfig { get; private set; } = null!;
    [field: SerializeField] public SoundsConfig SoundsConfig { get; private set; } = null!;
    [field: SerializeField] public MovementSoundConfig MovementSoundConfig { get; private set; } = null!;
    [field: SerializeField] public CameraFovConfig CameraFovConfig { get; private set; } = null!;
    [field: SerializeField] public InventoryConfig InventoryConfig { get; private set; } = null!;
    [field: SerializeField] public PlayerLifetimeScope PlayerPrefab { get; private set; } = null!;
     
    private PlayerLifetimeScope playerScope;

    [field: SerializeField] public BotSettings settings {get; private set;} = null!;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(InputConfig).AsSelf();
        builder.RegisterInstance(PlayerMovementConfig).AsSelf();
        builder.RegisterInstance(GravityConfig).AsSelf();
        builder.RegisterInstance(SoundsConfig).AsSelf();
        builder.RegisterInstance(MovementSoundConfig).AsSelf();
        builder.RegisterInstance(CameraFovConfig).AsSelf();
        builder.RegisterInstance(InventoryConfig).AsSelf();
       
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<PlaySoundMessage>(options);
        builder.RegisterInstance(settings).AsSelf();

        // === MessagePipe ===
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<PlayerMoveMessage>(options);
        builder.RegisterMessageBroker<LookDeltaMessage>(options);
        builder.RegisterMessageBroker<ClickMessage>(options);
        builder.RegisterMessageBroker<RightClickMessage>(options);
        builder.RegisterMessageBroker<BotVisionMessage>(options);
        
        builder.Register<AmmoStorage>(Lifetime.Singleton).AsSelf();
        
        //pools
        var pools = new GameObject("Pools");
        builder.RegisterInstance(pools.transform).Keyed("PoolsParent");
        builder.Register<ProjectilesPool>(Lifetime.Singleton).AsSelf();
        // builder.Register<ImpactPools>(Lifetime.Singleton).AsSelf();
        builder.RegisterEntryPoint<ImpactPools>().AsSelf();
        builder.RegisterEntryPoint<CasingPool>().AsSelf();
        // builder.Register<>(Lifetime.Singleton).AsSelf();
        
        //Sounds
        // var soundsManager = gameObject.AddComponent<SoundsManager>();
        // builder.RegisterComponent(soundsManager).AsSelf();
        
        //TODO: подкастылю создание игрока, братья, после того как загрузятся ресы из проэкта.
        builder.RegisterBuildCallback(container =>
                                      {
                                          GlobalMessagePipe.SetProvider(container.AsServiceProvider());

                                          var ammoStorage = container.Resolve<AmmoStorage>();

                                          UniTask.Void(async () =>
                                                       {
                                                           await ammoStorage.WaitUntilReady();

                                                           playerScope = CreateChildFromPrefab(PlayerPrefab);
                                                           // soundsManager.SetPlayer(playerScope);
                                                       });
                                      });
        // builder.RegisterEntryPoint<SoundsManager>().AsSelf();
        builder.RegisterEntryPoint<Bootloader>()
               .AsSelf()
               .As<IStartable>();
        builder.RegisterEntryPoint<SoundsManager>().AsSelf();
    }
}