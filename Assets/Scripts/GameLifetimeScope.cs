using CameraScripts;
using CanvasScripts;
using Cysharp.Threading.Tasks;
using Gravity;
using Input;
using InteractableScripts;
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
    [field: SerializeField] public CanvasConfig CanvasConfig { get; private set; } = null!;
    [field: SerializeField] public InteractableConfig InteractableConfig { get; private set; } = null!;
    [field: SerializeField] public PlayerLifetimeScope PlayerPrefab { get; private set; } = null!;
    
    private PlayerLifetimeScope playerScope;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(InputConfig).AsSelf();
        builder.RegisterInstance(PlayerMovementConfig).AsSelf();
        builder.RegisterInstance(GravityConfig).AsSelf();
        builder.RegisterInstance(SoundsConfig).AsSelf();
        builder.RegisterInstance(MovementSoundConfig).AsSelf();
        builder.RegisterInstance(CameraFovConfig).AsSelf();
        builder.RegisterInstance(InventoryConfig).AsSelf();
        builder.RegisterInstance(CanvasConfig).AsSelf();
        builder.RegisterInstance(InteractableConfig).AsSelf();
        
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<PlaySoundMessage>(options);
        
        builder.Register<AmmoStorage>(Lifetime.Singleton).AsSelf();
        
        //pools
        var pools = new GameObject("Pools");
        builder.RegisterInstance(pools.transform).Keyed("PoolsParent");
        builder.Register<ProjectilesPool>(Lifetime.Singleton).AsSelf();
        // builder.Register<ImpactPools>(Lifetime.Singleton).AsSelf();
        // builder.Register<CasingPool>(Lifetime.Singleton).AsSelf();
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
                                                           container.Resolve<SoundsManager>().PlayerTransform = playerScope.transform;
                                                       });
                                      });
        builder.RegisterEntryPoint<SoundsManager>().AsSelf();
        builder.RegisterEntryPoint<Bootloader>()
               .AsSelf()
               .As<IStartable>();
    }
}