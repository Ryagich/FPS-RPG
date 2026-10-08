using System;
using Dependencies;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace Bot
{
    public sealed class BotFollowControlSource : ITickable, IDisposable
    {
        private readonly Transform transform;
        private readonly Transform goal;
        private readonly BotNavigation navigation;
        private readonly IPublisher<MoveCommand> move;
        private readonly IPublisher<LookCommand> look;
        private readonly IPublisher<JumpCommand> jump;
        private bool disposed;

        public BotFollowControlSource(Transform transform, BotGoal goal, BotNavigation navigation,
            IPublisher<MoveCommand> move, IPublisher<LookCommand> look, IPublisher<JumpCommand> jump)
        {
            this.transform = transform;
            this.goal = goal.Value;
            this.navigation = navigation;
            this.move = move;
            this.look = look;
            this.jump = jump;
        }

        public void Tick()
        {
            if (disposed || Time.deltaTime <= 0f)
                return;
            // A self-target is authored on the prefab; scene instances may supply another target.
            if (goal == transform)
            {
                move.Publish(new MoveCommand(Vector2.zero));
                look.Publish(new LookCommand(Vector2.zero));
                return;
            }
            var yaw = navigation.GetYawDelta(goal.position, Time.deltaTime);
            var input = navigation.GetMove(goal.position, false, false, yaw, out var shouldJump);
            look.Publish(new LookCommand(new Vector2(yaw, 0f)));
            move.Publish(new MoveCommand(input));
            if (shouldJump)
                jump.Publish(new JumpCommand());
        }

        public void Dispose() => disposed = true;
    }
}
