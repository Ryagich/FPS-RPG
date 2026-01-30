using System;
using System.Collections.Generic;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bot
{
    public class BotSight : MonoBehaviour, ITickable
    {
        private IPublisher<BotVisionMessage> botVisionPublisher;
        [SerializeField] private LayerMask obstructionLayers;
        private Transform visionOrigin;


        private List<Collider> targetsInRange = new();
        private long lastVisibilityCheck = 0;
        private Collider selfRigidbody;
        private Dictionary<Collider, float> lastSeenTime = new();
        [SerializeField] private float loseSightDelay = 0.3f; // seconds

        [Inject]
        public void Construct(IPublisher<BotVisionMessage> botVisionPublisher, 
            [Key("visionOrigin")] Transform visionOrigin, [Key("collider")] Collider selfRigidbody) 
        {
            this.botVisionPublisher = botVisionPublisher;
            this.visionOrigin = visionOrigin;
            this.selfRigidbody = selfRigidbody;
        }


        private bool IsObstructed(Collider other)
        {
            Vector3 start = visionOrigin.position;
            Vector3 end = other.bounds.center;

            return Physics.Linecast(start, end, obstructionLayers);
        }

        void OnTriggerEnter(Collider other)
        {
            if(other == selfRigidbody)
            {
                return;
            }
            targetsInRange.Add(other);
            lastSeenTime[other] = Time.time;

            if (!IsObstructed(other))
                botVisionPublisher.Publish(new BotVisionMessage(other, true));
        }

        void OnTriggerExit(Collider other)
        {
            lastSeenTime[other] = Time.time;
        }

        private void CheckVisibilityOfTargets()
        {
            for(int i = 0; i < targetsInRange.Count; i++)
            {
                if(IsObstructed(targetsInRange[i]))
                {
                    botVisionPublisher?.Publish(new BotVisionMessage(targetsInRange[i], false));
                    continue;
                }

                botVisionPublisher?.Publish(new BotVisionMessage(targetsInRange[i], true));
            }
        }

        public void Tick()
        {
            for (int i = targetsInRange.Count - 1; i >= 0; i--)
            {
                var target = targetsInRange[i];

                if (IsObstructed(target))
                {
                    if (Time.time - lastSeenTime[target] > loseSightDelay)
                    {
                        botVisionPublisher.Publish(new BotVisionMessage(target, false));
                        targetsInRange.RemoveAt(i);
                    }
                    continue;
                }

                lastSeenTime[target] = Time.time;
                botVisionPublisher.Publish(new BotVisionMessage(target, true));
            }
        }
    }
}
