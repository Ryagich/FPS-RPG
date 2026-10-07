using UnityEngine;

namespace Scopes
{
    public static class ProjectBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // защита от двойного создания
            if (ProjectLifetimeScope.Instance != null)
                return;
            var prefab = Resources.Load<ProjectLifetimeScope>("Project/ProjectLifetimeScope");

            if (prefab == null)
            {
                Debug.LogError("ProjectLifetimeScope prefab not found in Resources!");
                return;
            }

            Object.Instantiate(prefab);
        }
    }
}