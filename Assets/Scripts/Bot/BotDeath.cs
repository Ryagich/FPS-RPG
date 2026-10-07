using VContainer;
﻿using System;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace Bot
{
    public sealed class BotDeath : IStartable, IDisposable
    {
        private readonly LifetimeScope scope;
        private readonly Inventory.Inventory inventory;
        private readonly CharacterController controller;
        private readonly Animator animator;
        private readonly Rigidbody[] ragdollBodies;
        private readonly IDisposable subscription;
        private bool dead;

        public BotDeath(LifetimeScope scope, Inventory.Inventory inventory, CharacterController controller,
            Animator animator, [Key("RagdollBodies")] Rigidbody[] ragdollBodies,
            ISubscriber<DeathMessage> death)
        {
            this.scope = scope;
            this.inventory = inventory;
            this.controller = controller;
            this.animator = animator;
            this.ragdollBodies = ragdollBodies;
            subscription = death.Subscribe(OnDeath);
        }

        private void OnDeath(DeathMessage message)
        {
            if (dead)
                return;
            dead = true;
            controller.enabled = false;
            inventory.DropWeapon();
            inventory.ClearSlots();
            if (animator != null)
            {
                animator.enabled = false;
                UnityEngine.Object.Destroy(animator);
            }
            UnityEngine.Object.Destroy(controller);
            scope.DisposeCore();
            UnityEngine.Object.Destroy(scope);
            foreach (var body in ragdollBodies)
            {
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
        
        public void Start() { }
        public void Dispose() => subscription.Dispose();
    }
}