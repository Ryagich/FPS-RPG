using VContainer.Unity;

namespace Dependencies
{
    public readonly struct GameScope
    {
        public LifetimeScope Value { get; }

        public GameScope(LifetimeScope value) => Value = value;
    }
}
