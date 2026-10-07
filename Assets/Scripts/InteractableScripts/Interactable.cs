using System;
using Weapon.Providers;

namespace InteractableScripts
{
    public sealed class Interactable
    {
        public event Action<WeaponProvider> Interacted;
        public event Action<WeaponProvider> Highlighted;
        public event Action<WeaponProvider> HighlightOuted;

        public void Interact(WeaponProvider actor) => Interacted?.Invoke(actor);
        public void Highlight(WeaponProvider actor) => Highlighted?.Invoke(actor);
        public void OutHighlight(WeaponProvider actor) => HighlightOuted?.Invoke(actor);
    }
}