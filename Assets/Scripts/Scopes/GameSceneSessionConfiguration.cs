namespace Scopes
{
    /// <summary>
    /// Immutable policy supplied by a scene before its GameLifetimeScope is built.
    /// Development scenes receive false and must not change gameplay-persistent state.
    /// </summary>
    public sealed class GameSceneSessionConfiguration
    {
        public GameSceneSessionConfiguration(bool isGameplayScene)
        {
            IsGameplayScene = isGameplayScene;
        }

        public bool IsGameplayScene { get; }
    }
}
