using Messages;
using StateMachine.Graph.Model;
using UnityEngine;

namespace StateMachine.Behaviours
{
    [CreateAssetMenu(fileName = "EnemyInSightBehaviour", menuName = "configs/StateMachine/Behaviours/EnemyInSight")]
    public class EnemyInSightBehaviour : BaseBehaviour
    {
        public override void Enter(StateMachineContext context)
        {
            context.LastSpineRotation = context.spine.rotation;
        }
        
        public override void Logic(StateMachineContext context)
        {
            if (context.aggroTarget == null) 
                return;

            var targetPos = context.aggroTarget.Value.target.bounds.center;

            var bodyLookDir = targetPos - context.self.position;
            var bodyTargetRot = Quaternion.LookRotation(bodyLookDir);

            var currentSpineRot = context.spine.rotation;

            var futureWorldRot = Quaternion.Slerp(
                                                  currentSpineRot,
                                                  bodyTargetRot,
                                                  context.DeltaTime * context.rotationDamping
                                                 );

            // --- вычисляем LookDelta ДО применения ---
            var deltaRot = futureWorldRot * Quaternion.Inverse(context.LastSpineRotation);

            deltaRot.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
                angle -= 360f;

            // переводим в yaw / pitch
            var yawDelta = Vector3.Dot(axis, Vector3.up) * angle;
            var pitchDelta = Vector3.Dot(axis, context.spine.right) * angle;

            context.LookDeltaPublisher.Publish(
                                               new LookDeltaMessage(new Vector2(yawDelta, -pitchDelta))
                                              );

            // --- твоя текущая логика ограничений ---
            var futureLocalRot = Quaternion.Inverse(context.spine.parent.rotation) * futureWorldRot;
            var futureRawY = futureLocalRot.eulerAngles.y;
            var futureNormalizedY = (futureRawY > 180) ? futureRawY - 360 : futureRawY;

            if (Mathf.Abs(futureNormalizedY) > 50f)
            {
                context.hips.rotation = deltaRot * context.hips.rotation;
                return;
            }

            context.spine.rotation = futureWorldRot;

            // --- сохраняем состояние ---
            context.LastSpineRotation = futureWorldRot;
        }

    }
}