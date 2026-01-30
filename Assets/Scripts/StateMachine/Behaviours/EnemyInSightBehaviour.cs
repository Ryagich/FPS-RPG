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

            Vector3 bodyLookDir = targetPos - context.self.position;
            bodyLookDir.y = 0;
            Quaternion bodyTargetRot = Quaternion.LookRotation(bodyLookDir);

            Vector3 spineLookDir = targetPos - context.visionOrigin.position;
            Quaternion spineTargetWorldRot = Quaternion.LookRotation(spineLookDir);

            float angleToTarget = Vector3.SignedAngle(context.self.forward, bodyLookDir, Vector3.up);
            if (Mathf.Abs(angleToTarget) > 50f)
            {
                context.self.rotation = Quaternion.Slerp(context.self.rotation, bodyTargetRot, context.DeltaTime * context.rotationDamping);
            }

            context.spine.rotation = Quaternion.Slerp(context.spine.rotation, spineTargetWorldRot, context.DeltaTime * context.rotationDamping);

            Vector3 localEuler = context.spine.localEulerAngles;

            float localY = Mathf.DeltaAngle(0, localEuler.y);
            float clampedY = Mathf.Clamp(localY, -50f, 50f);

            context.spine.localRotation = Quaternion.Euler(localEuler.x, clampedY, localEuler.z);
        }
    }
}