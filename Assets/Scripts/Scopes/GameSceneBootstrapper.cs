using Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Scopes
{
    /// <summary>
    /// Builds the scene session only after project-level boot has synchronized Yandex and Unity Localization.
    /// Put one on every gameplay or development scene and choose its role in the Inspector.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameSceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameLifetimeScope gameScopePrefab;
        [Header("Scene session")]
        [SerializeField] private bool isGameplayScene = true;

        private async void Awake()
        {
            var projectScope = ProjectLifetimeScope.Instance;
            if (projectScope == null)
            {
                Debug.LogError("ProjectLifetimeScope not found.", this);
                return;
            }

            var bootCompletion = projectScope.Container.Resolve<BootCompletion>();
            await bootCompletion.WaitAsync();

            if (gameScopePrefab == null)
            {
                Debug.LogError("GameLifetimeScope prefab is not assigned.", this);
                return;
            }

            var gameLifetimeScope = Instantiate(gameScopePrefab);
            gameLifetimeScope.gameObject.SetActive(false);

            gameLifetimeScope.parentReference.Object = projectScope;
            SceneManager.MoveGameObjectToScene(gameLifetimeScope.gameObject, gameObject.scene);
            gameLifetimeScope.SetSceneSessionConfiguration(new GameSceneSessionConfiguration(isGameplayScene));
            gameLifetimeScope.gameObject.SetActive(true);
            gameLifetimeScope.Build();
        }
    }
}
