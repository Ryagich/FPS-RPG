using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Conditions
{
    [CreateAssetMenu(fileName = "EnemyLostCondition", menuName = "configs/StateMachine/Conditions/EnemyLost")]
    public class EnemyLostCondition : BaseCondition
    {
        public override bool IsCondition(StateMachineContext context)
        {
            return context.aggroTarget == null;
        }
    }
}