using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InteractableScripts
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class InteractableFounder : ITickable
    {
        private readonly InteractableConfig interactableConfig;
        private readonly Transform cameraParentTransform;
        private readonly LifetimeScope lifetimeScope;

        private Interactable lastIntractable;

        public InteractableFounder
            (
                InteractableConfig interactableConfig,
                [Key("CameraParentTransform")] Transform cameraParentTransform,
                LifetimeScope lifetimeScope,
                ISubscriber<InteractableMessage> InteractableMessageSubscriber
            )
        {
            this.interactableConfig = interactableConfig;
            this.cameraParentTransform = cameraParentTransform;
            this.lifetimeScope = lifetimeScope;

            InteractableMessageSubscriber.Subscribe(Interact);
        }
        
        public void Tick()
        {
            var ray = new Ray(cameraParentTransform.position, cameraParentTransform.forward);
            if (Physics.Raycast(ray, out var hit, interactableConfig.Distance, interactableConfig.InteractableMask))
            {
                var interactable = hit.transform.GetComponentInParent<Interactable>();
                // Debug.Log($"lastIntractable {interactable.name}");
                if (interactable != lastIntractable)
                {
                    if (lastIntractable)
                        lastIntractable.OutHighlight(lifetimeScope);
                    lastIntractable = interactable;
                    lastIntractable.Highlight(lifetimeScope);
                }
            }
            else
            {
                if (lastIntractable)
                    lastIntractable.OutHighlight(lifetimeScope);
                lastIntractable = null;
            }
        }
        
        private void Interact(InteractableMessage msg)
        {
            if (lastIntractable != null)
            { 
                lastIntractable.Interact(lifetimeScope);
            }
        }
    }
}