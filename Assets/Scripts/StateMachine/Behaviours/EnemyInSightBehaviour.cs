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

            Vector3 lookPos = target - context.visionOrigin.position;

            Quaternion rotation = Quaternion.LookRotation(lookPos);
            context.visionOrigin.rotation = Quaternion.Slerp(context.visionOrigin.rotation, rotation, context.DeltaTime * context.rotationDamping);


            context.spine.rotation = Quaternion.Slerp(context.spine.rotation, rotation, context.DeltaTime * context.rotationDamping);
        }
    }
}