using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Behaviours
{
    [CreateAssetMenu(fileName = "EnemyInSightBehaviour", menuName = "configs/StateMachine/Behaviours/EnemyInSight")]
    public class EnemyInSightBehaviour : BaseBehaviour
    {
        public override void Logic(StateMachineContext context)
        {
            if(context.aggroTarget == null)
            {
                return;
            }
            Vector3 target = context.aggroTarget.Value.target.bounds.center;

            Vector3 lookPos = target - context.self.position;
            lookPos.y = 0;

            Quaternion rotation = Quaternion.LookRotation(lookPos);
            context.self.rotation = Quaternion.Slerp(context.self.rotation, rotation, context.DeltaTime * context.rotationDamping);
        }
    }
}