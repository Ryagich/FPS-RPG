


using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Conditions
{
    [CreateAssetMenu(fileName = "EnemyInSight Condition", menuName = "configs/StateMachine/Conditions/EnemyInSight")]
    public class EnemyInSightCondition : BaseCondition
    {
        public override bool IsCondition(StateMachineContext context)
        {
            return context.aggroTarget != null;
        }
    }
}