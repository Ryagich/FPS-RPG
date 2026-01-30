using System;
using UnityEngine;
using VContainer.Unity;

namespace InteractableScripts
{
    public class Interactable : MonoBehaviour
    {
        public event Action<LifetimeScope> Interacted; 
        public event Action<LifetimeScope> Highlighted; 
        public event Action<LifetimeScope> HighlightOuted; 

        [field: SerializeField] public bool CanInteractable { get; private set; } = true;
        [field: SerializeField] public bool ManyInteract { get; private set; }
        
        public void Interact(LifetimeScope interacting)
        {
            if (!CanInteractable)
                return;
            Interacted?.Invoke(interacting);
        }
    
        public void Highlight(LifetimeScope interacting)
        {
            if (!CanInteractable)
                return;
            Highlighted?.Invoke(interacting);
        }
    
        public void OutHighlight(LifetimeScope interacting)
        {
            if (!CanInteractable)
                return;
            HighlightOuted?.Invoke(interacting);
        }
    }
}