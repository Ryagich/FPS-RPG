using MessagePipe;
using Messages;
using StateMachine;
using StateMachine.Graph.Model;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BotController : IStartable, IFixedTickable
    {
        public readonly StateMachineContext context;
        private readonly BotSettings settings;
        public ReactiveProperty<State> CurrentState {get; private set;} = new();
        private bool started;

        public BotController
            (
                BotSettings settings, 
                NavMeshAgent agent, 
                [Key("botGoal")] Transform goal, 
                [Key("self")] Transform self, 
                [Key("visionOrigin")] Transform visionOrigin, 
                [Key("spine")] Transform spine, 
                [Key("hips")] Transform hips,
                IPublisher<LookDeltaMessage> lookDeltaMessageSubscriber
            )
        {
            context = new StateMachineContext();
            context.agent = agent;
            context.goal = goal;
            context.rotationDamping = settings.botRotationDamping;
            context.self = self;
            context.visionOrigin = visionOrigin;
            context.spine = spine;
            context.hips = hips;
            context.LookDeltaPublisher = lookDeltaMessageSubscriber;
            
            this.settings = settings;
        }
        
        public void FixedTick()
        {
            if(!started)
            {
                return;
            }
            if (CurrentState == null)
            {
                return;
            }
            context.DeltaTime = Time.fixedDeltaTime;

            foreach (var behaviour in CurrentState.Value.Behaviours)
            {
                if(behaviour == null)
                {
                    continue;
                }
                behaviour.Logic(context);
            }
            
            foreach (var transition in CurrentState.Value.Transitions)
            {
                if (transition.CanTransition(context))
                {
                    Debug.Log("Can transition to: " + transition.name);
                    foreach (var action in transition.ActionOnTransitions)
                    {
                        action.DoAction(context);
                    }
                    SetState(transition.TargetState);
                    break;
                }
            }
        }
        
        private void SetState(State state)
        {
            if (CurrentState.Value != null)
            {
                foreach (var behaviour in CurrentState.Value.Behaviours)
                {
                    if(behaviour == null)
                    {
                        continue;
                    }
                    behaviour.Exit(context);
                }
            }
            
            
            // ReSharper disable once PossibleNullReferenceException
            CurrentState.Value = state;
            Debug.Log(CurrentState.Value.Behaviours.Count);
            foreach (var behaviour in CurrentState.Value.Behaviours)
            {
                if(behaviour == null)
                {
                    continue;
                }
                behaviour.Enter(context);
            }
        }

        public void Start()
        {
            SetState(settings.StateMachineGraph.GetEntryState());
            started = true;
        }
    }
}