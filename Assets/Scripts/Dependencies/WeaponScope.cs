using VContainer.Unity;

namespace Dependencies
{
    public readonly struct WeaponScope
    {
        public LifetimeScope Value { get; }

        public WeaponScope(LifetimeScope value) => Value = value;
    }
}
