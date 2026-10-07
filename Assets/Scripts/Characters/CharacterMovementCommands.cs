using System;
using MessagePipe;
using Messages;

namespace Characters
{
    public sealed class CharacterMovementCommands : IDisposable
    {
        private readonly IDisposable subscriptions;
        private readonly CharacterState state;

        public CharacterMovementCommands(CharacterState state, ISubscriber<MoveCommand> move,
            ISubscriber<SprintCommand> sprint, ISubscriber<CrouchCommand> crouch,
            ISubscriber<JumpCommand> jump)
        {
            this.state = state;
            var bag = DisposableBag.CreateBuilder();
            move.Subscribe(command => state.MoveRequest = command.Direction).AddTo(bag);
            sprint.Subscribe(command => state.SprintRequested = command.State).AddTo(bag);
            crouch.Subscribe(command => state.CrouchRequested = command.State).AddTo(bag);
            jump.Subscribe(_ => state.JumpRequested = true).AddTo(bag);
            subscriptions = bag.Build();
        }

        public bool ConsumeJump()
        {
            var requested = state.JumpRequested;
            state.JumpRequested = false;
            return requested;
        }

        public void Dispose() => subscriptions.Dispose();
    }
}
