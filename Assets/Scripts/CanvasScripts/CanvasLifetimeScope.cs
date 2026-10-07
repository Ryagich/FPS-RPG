using Scopes;
using VContainer;
using VContainer.Unity;

namespace CanvasScripts
{
    public class CanvasLifetimeScope : EntityLifetimeScope
    {
        [UnityEngine.SerializeField] private UnityEngine.Canvas canvas;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(canvas).As<UnityEngine.Canvas>();
        }
    }
}
