using System;
using Characters;
using Dependencies;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Providers;

namespace InteractableScripts
{
    public sealed class InteractionController : ITickable, IDisposable
    {
        private readonly InteractableConfig config;
        private readonly Transform aim;
        private readonly WeaponProvider actor;
        private readonly EntityTargets targets;
        private readonly IDisposable subscription;
        private Interactable current;

        public InteractionController(InteractableConfig config, AimTransform aimReference,
            WeaponProvider actor, EntityTargets targets, ISubscriber<InteractCommand> interact)
        {
            var aim = aimReference.Value;
            this.config = config;
            this.aim = aim;
            this.actor = actor;
            this.targets = targets;
            subscription = interact.Subscribe(_ =>
            {
                UpdateTarget();
                if (current != null)
                    current.Interact(actor);
            });
        }

        public void Tick() => UpdateTarget();

        private void UpdateTarget()
        {
            Interactable next = null;
            if (Physics.Raycast(aim.position, aim.forward, out var hit, config.Distance,
                    config.InteractableMask, QueryTriggerInteraction.Ignore))
                targets.TryGetInteraction(hit.collider, out next);
            if (next == current)
                return;
            if (current != null)
                current.OutHighlight(actor);
            current = next;
            if (current != null)
                current.Highlight(actor);
        }

        public void Dispose()
        {
            subscription.Dispose();
            if (current != null)
                current.OutHighlight(actor);
        }
    }
}
