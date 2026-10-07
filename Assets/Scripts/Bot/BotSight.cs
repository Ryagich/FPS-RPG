using System;
using System.Collections.Generic;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;

namespace Bot
{
    // An optional sensor service. A physics callback adapter can feed Register/UnregisterTarget.
    public sealed class BotSight : ITickable, IDisposable
    {
        private readonly Transform origin;
        private readonly LayerMask obstructionLayers;
        private readonly IPublisher<BotVisionMessage> publisher;
        private readonly Dictionary<Collider, bool> targets = new();
        private readonly List<Collider> snapshot = new();
        private float nextCheck;
        
        public BotSight(Transform origin, LayerMask obstructionLayers, IPublisher<BotVisionMessage> publisher)
        {
            this.origin = origin;
            this.obstructionLayers = obstructionLayers;
            this.publisher = publisher;
        }

        public void RegisterTarget(Collider collider)
        {
            if (targets.ContainsKey(collider))
                return;
            var visible = IsVisible(collider);
            targets.Add(collider, visible);
            publisher.Publish(new BotVisionMessage(collider, visible));
        }

        public void UnregisterTarget(Collider collider)
        {
            if (targets.Remove(collider))
                publisher.Publish(new BotVisionMessage(collider, false));
        }

        private bool IsVisible(Collider collider) => !Physics.Linecast(origin.position,
            collider.bounds.center, obstructionLayers, QueryTriggerInteraction.Ignore);

        public void Tick()
        {
            if (Time.time < nextCheck)
                return;
            nextCheck = Time.time + 0.1f;
            snapshot.Clear();
            snapshot.AddRange(targets.Keys);
            foreach (var collider in snapshot)
            {
                if (collider == null)
                {
                    targets.Remove(collider);
                    continue;
                }
                var visible = IsVisible(collider);
                if (targets[collider] == visible)
                    continue;
                targets[collider] = visible;
                publisher.Publish(new BotVisionMessage(collider, visible));
            }
        }

        public void Dispose()
        {
            targets.Clear();
            snapshot.Clear();
        }
    }
}