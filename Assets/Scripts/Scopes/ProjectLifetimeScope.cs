using Bot;
using CameraScripts;
using CanvasScripts;
using Gravity;
using Input;
using InteractableScripts;
using Inventory;
using Inventory.Ammo;
using Player;
using Player.Stats;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class ProjectLifetimeScope : LifetimeScope
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
        [field: SerializeField] public StatsConfig StatsConfig { get; private set; } = null!;
        [field: SerializeField] public BotSettings BotSettings {get; private set;} = null!;
        
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
            builder.RegisterInstance(StatsConfig).AsSelf();
            builder.RegisterInstance(BotSettings).AsSelf();
            
            builder.Register<AmmoStorage>(Lifetime.Singleton).AsSelf();
            
            builder.RegisterEntryPoint<ProjectInitializer>().AsSelf();
        }
    }
}