using CameraScripts;
using CanvasScripts;
using Characters;
using Gravity;
using Input;
using InteractableScripts;
using Inventory;
using Localization;
using Player.Stats;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        public static ProjectLifetimeScope Instance { get; private set; }

        [field: SerializeField] public InputConfig InputConfig { get; private set; } = null!;
        [field: FormerlySerializedAs("<PlayerMovementConfig>k__BackingField")]
        [field: SerializeField] public CharacterMovementConfig CharacterMovementConfig { get; private set; } = null!;
        [field: SerializeField] public GravityConfig GravityConfig { get; private set; } = null!;
        [field: SerializeField] public SoundsConfig SoundsConfig { get; private set; } = null!;
        [field: SerializeField] public MovementSoundConfig MovementSoundConfig { get; private set; } = null!;
        [field: SerializeField] public CameraFovConfig CameraFovConfig { get; private set; } = null!;
        [field: SerializeField] public InventoryConfig InventoryConfig { get; private set; } = null!;
        [field: SerializeField] public CanvasConfig CanvasConfig { get; private set; } = null!;
        [field: SerializeField] public InteractableConfig InteractableConfig { get; private set; } = null!;
        [field: SerializeField] public StatsConfig StatsConfig { get; private set; } = null!;

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            base.OnDestroy();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(InputConfig).AsSelf();
            builder.RegisterInstance(CharacterMovementConfig).AsSelf();
            builder.RegisterInstance(GravityConfig).AsSelf();
            builder.RegisterInstance(SoundsConfig).AsSelf();
            builder.RegisterInstance(MovementSoundConfig).AsSelf();
            builder.RegisterInstance(CameraFovConfig).AsSelf();
            builder.RegisterInstance(InventoryConfig).AsSelf();
            builder.RegisterInstance(InventoryConfig.CasingPref).Keyed("CasingPrefab");
            builder.RegisterInstance(CanvasConfig).AsSelf();
            builder.RegisterInstance(InteractableConfig).AsSelf();
            builder.RegisterInstance(StatsConfig).AsSelf();
            
            builder.Register<BootCompletion>(Lifetime.Singleton).AsSelf();
            
            builder.RegisterEntryPoint<Bootloader>().AsSelf();
        }
    }
}
