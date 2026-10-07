using VContainer.Unity;

namespace Scopes
{
    // Prefab factories set the parent before Awake. Scene composition assigns it before Build.
    public abstract class EntityLifetimeScope : LifetimeScope
    {
        protected override void Awake()
        {
            if (parentReference.Object != null)
                base.Awake();
        }
    }
}
