using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameSceneBootstrapper : LifetimeScope
    {
        [SerializeField] private GameLifetimeScope gameScopePrefab;
        [SerializeField] private bool isGameplayScene = true;
        [Tooltip("Scopes placed in this scene; their parent is assigned explicitly after game scope boot.")]
        [SerializeField] private EntityLifetimeScope[] sceneEntities = System.Array.Empty<EntityLifetimeScope>();

        protected override void Awake()
        {
            parentReference.Object = ProjectLifetimeScope.Instance;
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new GameSceneBootstrapSettings(gameScopePrefab, gameObject.scene,
                isGameplayScene, sceneEntities));
            builder.RegisterEntryPoint<GameSceneBootstrap>().AsSelf();
        }
    }
}
