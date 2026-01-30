using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.ActionOnTransition
{
    [CreateAssetMenu(fileName = "MoveToObjective", menuName = "configs/StateMachine/ActionsOnTransition/EntrypointAction")]
    public class EntrypointAction : ActionOnTransitionBase
    {
        public override void DoAction(StateMachineContext context)
        {
            Debug.Log("Start");
            context.agent.destination = context.goal.position;
        }
    }
}