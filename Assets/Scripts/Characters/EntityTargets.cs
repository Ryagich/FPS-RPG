using System.Collections.Generic;
using InteractableScripts;
using Player.Stats;
using UnityEngine;

namespace Characters
{
    // Scene-owned lookup populated by entity scopes; physics hits never search a hierarchy.
    public sealed class EntityTargets
    {
        private readonly Dictionary<Collider, StatsController> damageTargets = new();
        private readonly Dictionary<Collider, Interactable> interactables = new();

        public void RegisterDamage(Collider collider, StatsController target) => damageTargets.Add(collider, target);
        public void UnregisterDamage(Collider collider) => damageTargets.Remove(collider);
        public bool TryGetDamage(Collider collider, out StatsController target) => damageTargets.TryGetValue(collider, out target);
        public void RegisterInteraction(Collider collider, Interactable target) => interactables.Add(collider, target);
        public void UnregisterInteraction(Collider collider) => interactables.Remove(collider);
        public bool TryGetInteraction(Collider collider, out Interactable target) => interactables.TryGetValue(collider, out target);
    }
}
