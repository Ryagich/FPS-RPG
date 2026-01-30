using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Conditions
{
    [CreateAssetMenu(fileName = "EnemyDestroyedCondition", menuName = "configs/StateMachine/Conditions/EnemyDestroyed")]
    public class EnemyDestroyedCondition : BaseCondition
    {
        public override bool IsCondition(StateMachineContext context)
        {
            return false;
        }
    }
}