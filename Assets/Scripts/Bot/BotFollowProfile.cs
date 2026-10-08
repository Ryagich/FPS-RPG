using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    [CreateAssetMenu(fileName = "BotFollowProfile", menuName = "configs/Bot/Follow Control")]
    public sealed class BotFollowProfile : BotControlProfile
    {
        public override void Register(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BotFollowControlSource>().AsSelf();
        }
    }
}
