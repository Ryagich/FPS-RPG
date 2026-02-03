using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BotDeath : IStartable
    {
        private readonly LifetimeScope lifetimeScope;
        private readonly NavMeshAgent navMeshAgent;
        private readonly Animator animator;
        private readonly Inventory.Inventory inventory;
        private readonly Transform transform;

        public BotDeath
            (
                LifetimeScope lifetimeScope,
                NavMeshAgent navMeshAgent,
                Animator animator,
                Inventory.Inventory inventory,
                [Key("self")] Transform transform,
                ISubscriber<DeathMessage> deathMessageSubscribe
            )
        {
            this.lifetimeScope = lifetimeScope;
            this.navMeshAgent = navMeshAgent;
            this.animator = animator;
            this.inventory = inventory;
            this.transform = transform;
            deathMessageSubscribe.Subscribe(OnDeath);
        }

        private void OnDeath(DeathMessage msg)
        {
            inventory.DropWeapon();
            inventory.ClearSlots();
            lifetimeScope.DisposeCore();
            Object.Destroy(lifetimeScope);
            Object.Destroy(navMeshAgent);
            Object.Destroy(animator);

            RemoveAllForces();
            // navMeshAgent.enabled = false;
        }
        
        private void RemoveAllForces()
        {
            foreach (var member in transform.GetComponentsInChildren<Rigidbody>())
            {
                member.isKinematic = false;
                member.velocity = Vector3.zero;
            }
        }
        
        public void Start() { }
    }
}