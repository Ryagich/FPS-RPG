using Dependencies;
using UnityEngine;
using VContainer.Unity;

namespace Bot
{
    // Keep aiming independent of animated bones and follow the physical crouch height.
    public sealed class BotAimOrigin : ITickable
    {
        private readonly CharacterController controller;
        private readonly Transform aim;
        private readonly float standingHeight;
        private readonly float standingY;

        public BotAimOrigin(CharacterController controller, AimTransform aimReference)
        {
            var aim = aimReference.Value;
            this.controller = controller;
            this.aim = aim;
            standingHeight = controller.height;
            standingY = aim.localPosition.y;
        }

        public void Tick()
        {
            var position = aim.localPosition;
            position.y = standingY + controller.height - standingHeight;
            aim.localPosition = position;
        }
    }
}
