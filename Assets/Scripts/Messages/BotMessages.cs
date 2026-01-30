

using UnityEngine;

namespace Messages
{
    public readonly struct BotVisionMessage
    {
        public readonly Collider target;
        public readonly bool isVisible;

        public BotVisionMessage(Collider target, bool isVisible)
        {
            this.target = target;
            this.isVisible = isVisible;
        }
    }
}