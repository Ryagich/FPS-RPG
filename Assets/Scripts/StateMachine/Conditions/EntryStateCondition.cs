using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Conditions
{
    [CreateAssetMenu(fileName = "EntryStateCondition", menuName = "configs/StateMachine/Conditions/EnemyInSight")]
    public class EntryStateCondition : BaseCondition
    {
        public override bool IsCondition(StateMachineContext context)
        {
            Debug.Log("Can transition");
            Debug.Log(context.agent != null && context.goal != null);
            return context.agent != null && context.goal != null;
        }
    }
}