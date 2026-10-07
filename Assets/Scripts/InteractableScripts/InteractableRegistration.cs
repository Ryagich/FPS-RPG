using VContainer;
using System;
using Characters;
using UnityEngine;
using VContainer.Unity;

namespace InteractableScripts
{
    public sealed class InteractableRegistration : IStartable, IDisposable
    {
        private readonly EntityTargets targets;
        private readonly Interactable interactable;
        private readonly Collider[] colliders;

        public InteractableRegistration(EntityTargets targets, Interactable interactable,
            [Key("InteractionColliders")] Collider[] colliders)
        {
            this.targets = targets;
            this.interactable = interactable;
            this.colliders = colliders;
        }

        public void Start()
        {
            foreach (var collider in colliders)
                targets.RegisterInteraction(collider, interactable);
        }

        public void Dispose()
        {
            foreach (var collider in colliders)
                targets.UnregisterInteraction(collider);
        }
    }
}
