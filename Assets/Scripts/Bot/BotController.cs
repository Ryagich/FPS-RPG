using StateMachine;
using StateMachine.Graph.Model;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    public class BotController : IFixedTickable, IStartable
    {
        public StateMachineContext context;
        public ReactiveProperty<State> CurrentState {get; private set;} = new();
        private BotSettings settings;

        public BotController(BotSettings settings, NavMeshAgent agent, [Key("botGoal")] Transform goal, [Key("self")] Transform self)
        {
            context = new StateMachineContext();
            context.agent = agent;
            context.goal = goal;
            context.rotationDamping = settings.botRotationDamping;
            context.self = self;
            this.settings = settings;
        }


        public void FixedTick()
        {
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
        }
    }
}