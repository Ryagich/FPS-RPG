using Bot;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Demo.Bot
{
    [CreateAssetMenu(fileName = "BotDemoProfile", menuName = "configs/Demo/Bot Action Loop")]
    public sealed class BotDemoProfile : BotControlProfile
    {
        [field: SerializeField] public bool RefillAmmo { get; private set; } = true;
        [field: SerializeField, Min(0.1f)] public float MoveDistance { get; private set; } = 3f;
        [field: SerializeField, Min(0.5f)] public float StepDuration { get; private set; } = 3f;

        public override void Register(IContainerBuilder builder)
        {
            builder.RegisterInstance(this);
            builder.RegisterEntryPoint<BotDemoControlSource>().AsSelf();
        }
    }
}
