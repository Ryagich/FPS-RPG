using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Behaviours
{
    [CreateAssetMenu(fileName = "EnemyInSightBehaviour", menuName = "configs/StateMachine/Behaviours/EnemyInSight")]
    public class EnemyInSightBehaviour : BaseBehaviour
    {
        public override void Logic(StateMachineContext context)
        {
            if (context.aggroTarget == null) return;

            Vector3 targetPos = context.aggroTarget.Value.target.bounds.center;

            float distanceToTarget = Vector3.Distance(context.self.position, targetPos);
            Vector3 bodyLookDir = targetPos - context.self.position;
            Quaternion bodyTargetRot = Quaternion.LookRotation(bodyLookDir);

            Quaternion current = context.spine.rotation;
            Quaternion target = bodyTargetRot;

            Quaternion diff = target * Quaternion.Inverse(current);

            Quaternion futureWorldRot = Quaternion.Slerp(context.spine.rotation, bodyTargetRot, context.DeltaTime * context.rotationDamping);


            Quaternion futureLocalRot = Quaternion.Inverse(context.spine.parent.rotation) * futureWorldRot;

            float futureRawY = futureLocalRot.eulerAngles.y;
            float futureNormalizedY = (futureRawY > 180) ? futureRawY - 360 : futureRawY;

            if (Mathf.Abs(futureNormalizedY) > 50f)
            {
                diff.z = 0;
                diff.x = 0;
                context.hips.rotation = diff * context.hips.rotation;
                return;
            }

            context.spine.rotation = futureWorldRot;
        }
    }
}