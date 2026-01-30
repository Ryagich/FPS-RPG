using UnityEngine;
using StateMachine.Graph.Model;

namespace StateMachine.Behaviours
{
    [CreateAssetMenu(fileName = "MoveToObjective", menuName = "configs/StateMachine/Behaviours/MoveToObjective")]
    public class MoveToObjective : BaseBehaviour
    {
        public override void Enter(StateMachineContext context)
        {
            Debug.Log("Entered");
            context.agent.isStopped = false;
            context.agent.destination = context.goal.position;
        }

        public override void Exit(StateMachineContext context)
        {
            Debug.Log("Exited");
            context.agent.isStopped = true;
        }
    }
}