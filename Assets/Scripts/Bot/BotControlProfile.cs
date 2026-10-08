using UnityEngine;
using VContainer;

namespace Bot
{
    // The entity scope delegates command-source registration to the selected composition asset.
    public abstract class BotControlProfile : ScriptableObject
    {
        public abstract void Register(IContainerBuilder builder);
    }
}
