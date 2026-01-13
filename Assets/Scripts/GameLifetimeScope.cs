using Gravity;
using Input;
using MessagePipe;
using Messages;
using Player;
using Sounds;
using Sounds.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [field: SerializeField] public InputConfig InputConfig { get; private set; } = null!;
    [field: SerializeField] public PlayerMovementConfig PlayerMovementConfig { get; private set; } = null!;
    [field: SerializeField] public GravityConfig GravityConfig { get; private set; } = null!;
    [field: SerializeField] public SoundsConfig SoundsConfig { get; private set; } = null!;
    [field: SerializeField] public MovementSoundConfig MovementSoundConfig { get; private set; } = null!;
    [field: SerializeField] public PlayerLifetimeScope PlayerPrefab { get; private set; } = null!;
    
    private PlayerLifetimeScope playerScope;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(InputConfig).AsSelf();
        builder.RegisterInstance(PlayerMovementConfig).AsSelf();
        builder.RegisterInstance(GravityConfig).AsSelf();
        builder.RegisterInstance(SoundsConfig).AsSelf();
        builder.RegisterInstance(MovementSoundConfig).AsSelf();
        
        // === MessagePipe ===
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<PlayerMoveMessage>(options);
        builder.RegisterMessageBroker<LookDeltaMessage>(options);
        builder.RegisterMessageBroker<ClickMessage>(options);
        builder.RegisterMessageBroker<RightClickMessage>(options);
        builder.RegisterMessageBroker<JumpMessage>(options);
        builder.RegisterMessageBroker<ChangeSprintStateMessage>(options);
        builder.RegisterMessageBroker<ChangeCrouchingStateMessage>(options);
            
        // === InputHandler ===
        builder.Register<InputHandler>(Lifetime.Singleton).AsSelf().As<IStartable>();
        
        //Sounds
        var soundsManager = gameObject.AddComponent<SoundsManager>();
        builder.RegisterComponent(soundsManager).AsSelf();

        builder.RegisterBuildCallback(container =>
                                      {
                                          GlobalMessagePipe.SetProvider(container.AsServiceProvider());
                                          playerScope = CreateChildFromPrefab(PlayerPrefab, childBuilder =>
                                                                              {
                                                                              });
                                          soundsManager.SetPlayer(playerScope);
                                      });
    }
}