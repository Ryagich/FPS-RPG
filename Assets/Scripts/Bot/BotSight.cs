using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    public class BotSight : MonoBehaviour, ITickable
    {
        [SerializeField] private LayerMask obstructionLayers;
        private Transform visionOrigin;
        
        private IPublisher<BotVisionMessage> botVisionPublisher;

        private readonly List<Collider> targetsInRange = new();
        private readonly long lastVisibilityCheck = 0;

        [Inject]
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Construct
            (
                [Key("visionOrigin")] Transform visionOrigin,
                IPublisher<BotVisionMessage> botVisionPublisher
            )
        {
            this.visionOrigin = visionOrigin;
            this.botVisionPublisher = botVisionPublisher;
        }


        private bool IsObstructed(Collider other)
        {
            var start = visionOrigin.position;
            var end = other.bounds.center;

            return Physics.Linecast(start, end, obstructionLayers);
        }

        void OnTriggerEnter(Collider other)
        {
            targetsInRange.Add(other);
            if (IsObstructed(other))
            {
                Debug.Log("Target is obstructed");
                return;
            }
            botVisionPublisher?.Publish(new BotVisionMessage(other, true));
        }

        void OnTriggerExit(Collider other)
        {
            targetsInRange.Remove(other);
            botVisionPublisher?.Publish(new BotVisionMessage(other, false));
        }

        private void CheckVisibilityOfTargets()
        {
            for (var i = 0; i < targetsInRange.Count; i++)
            {
                if (IsObstructed(targetsInRange[i]))
                {
                    botVisionPublisher?.Publish(new BotVisionMessage(targetsInRange[i], false));
                    continue;
                }

                botVisionPublisher?.Publish(new BotVisionMessage(targetsInRange[i], true));
            }
        }

        public void Tick()
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (now - lastVisibilityCheck < 100)
            {
                return;
            }

            CheckVisibilityOfTargets();
        }
    }
}