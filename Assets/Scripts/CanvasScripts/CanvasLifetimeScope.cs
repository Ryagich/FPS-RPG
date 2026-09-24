using VContainer;
using VContainer.Unity;

namespace CanvasScripts
{
    public class CanvasLifetimeScope : LifetimeScope
    {
        [UnityEngine.SerializeField] private UnityEngine.Canvas canvas;

        protected override void Configure(IContainerBuilder builder)
        {
            canvas ??= GetComponent<UnityEngine.Canvas>();
            if (canvas == null)
            {
                UnityEngine.Debug.LogError("CanvasLifetimeScope requires a Canvas component.", this);
                return;
            }

            builder.RegisterComponent(canvas).As<UnityEngine.Canvas>();
        }
    }
}
