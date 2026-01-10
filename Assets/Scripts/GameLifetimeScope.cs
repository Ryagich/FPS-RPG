using Gravity;
using Input;
using MessagePipe;
using Messages;
using Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [field: SerializeField] public InputConfig InputConfig { get; private set; } = null!;
    [field: SerializeField] public PlayerMovementConfig PlayerMovementConfig { get; private set; } = null!;
    [field: SerializeField] public GravityConfig GravityConfig { get; private set; } = null!;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(InputConfig).AsSelf();
        builder.RegisterInstance(PlayerMovementConfig).AsSelf();
        builder.RegisterInstance(GravityConfig).AsSelf();

        // === MessagePipe ===
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<PlayerMoveMessage>(options);
        builder.RegisterMessageBroker<LookDeltaMessage>(options);
        builder.RegisterMessageBroker<ClickMessage>(options);
        builder.RegisterMessageBroker<RightClickMessage>(options);
        builder.RegisterMessageBroker<JumpMessage>(options);

        // === InputHandler ===
        builder.Register<InputHandler>(Lifetime.Singleton).AsSelf().As<IStartable>();
    }
}