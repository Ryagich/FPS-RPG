using System.Threading;
using Cysharp.Threading.Tasks;
using Localization;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Scopes
{
    public sealed class GameSceneBootstrap : IAsyncStartable
    {
        private readonly LifetimeScope owner;
        private readonly BootCompletion boot;
        private readonly GameSceneBootstrapSettings settings;

        public GameSceneBootstrap(LifetimeScope owner, BootCompletion boot, GameSceneBootstrapSettings settings)
        {
            this.owner = owner;
            this.boot = boot;
            this.settings = settings;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await boot.WaitAsync().AttachExternalCancellation(cancellation);
            cancellation.ThrowIfCancellationRequested();
            var gameScope = owner.CreateChildFromPrefab(settings.Prefab);
            gameScope.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(gameScope.gameObject, settings.Scene);
            gameScope.SetSceneSessionConfiguration(new GameSceneSessionConfiguration(settings.IsGameplayScene));
            gameScope.Build();
            foreach (var entity in settings.Entities)
            {
                entity.parentReference.Object = gameScope;
                entity.Build();
            }
        }
    }

    public sealed class GameSceneBootstrapSettings
    {
        public GameLifetimeScope Prefab { get; }
        public Scene Scene { get; }
        public bool IsGameplayScene { get; }
        public EntityLifetimeScope[] Entities { get; }

        public GameSceneBootstrapSettings(GameLifetimeScope prefab, Scene scene, bool isGameplayScene,
            EntityLifetimeScope[] entities)
        {
            Prefab = prefab;
            Scene = scene;
            IsGameplayScene = isGameplayScene;
            Entities = entities;
        }
    }
}
