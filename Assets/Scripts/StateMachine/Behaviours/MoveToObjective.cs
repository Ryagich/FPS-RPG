using Cysharp.Threading.Tasks;
using StateMachine.Graph.Model;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachine.Behaviours
{
    [CreateAssetMenu(fileName = "MoveToObjective", menuName = "configs/StateMachine/Behaviours/MoveToObjective")]
    public class MoveToObjective : BaseBehaviour
    {
        public override void Enter(StateMachineContext context)
        {
            UniTask.Void(async () =>
            {
                // Wait until agent reports it's on the NavMesh
                while (!context.agent.isOnNavMesh)
                {
                    // Try snapping if close to a NavMesh
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(context.agent.transform.position, out hit, 2f, NavMesh.AllAreas))
                    {
                        context.agent.Warp(hit.position);
                    }
                    await UniTask.NextFrame(); // wait one frame
                }

                context.agent.isStopped = false;
                context.agent.destination = context.goal.position;
            });
        }

        public override void Exit(StateMachineContext context)
        {
            if (context.goal == null)
            {
                return;
            }
            Debug.Log("Exited");
            context.agent.isStopped = true;
        }
    }
}