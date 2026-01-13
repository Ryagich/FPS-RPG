using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Conditions
{
    [CreateAssetMenu(fileName = "ReachedObjectiveCondition", menuName = "configs/StateMachine/Conditions/ReachedObjective")]
    public class ReachedObjectiveCondition : BaseCondition
    {
        public override bool IsCondition(StateMachineContext context)
        {
            return false;
        }
    }
}