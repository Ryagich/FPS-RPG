using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    public class BotMovement : IStartable
    {    
        private Transform goal;
        private readonly NavMeshAgent agent;

        [Inject]
        public BotMovement(NavMeshAgent agent, [Key("botGoal")] Transform goal)
        {
            this.agent = agent;
            this.goal = goal;
        }


        void IStartable.Start()
        {
            agent.destination = goal.position;
        }
    }
}
