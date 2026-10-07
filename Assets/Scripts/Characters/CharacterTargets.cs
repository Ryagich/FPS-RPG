using System;
using Dependencies;
using Player.Stats;
using UnityEngine;
using VContainer.Unity;

namespace Characters
{
    public sealed class CharacterTargets : IStartable, IDisposable
    {
        private readonly EntityTargets targets;
        private readonly StatsController stats;
        private readonly Collider[] colliders;

        public CharacterTargets(EntityTargets targets, StatsController stats,
            BodyColliders collidersReference)
        {
            var colliders = collidersReference.Value;
            this.targets = targets;
            this.stats = stats;
            this.colliders = colliders;
        }

        public void Start()
        {
            foreach (var collider in colliders)
                targets.RegisterDamage(collider, stats);
        }

        public void Dispose()
        {
            foreach (var collider in colliders)
                targets.UnregisterDamage(collider);
        }
    }
}
