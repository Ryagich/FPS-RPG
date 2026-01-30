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

        [Inject]
        public void Construct(IPublisher<BotVisionMessage> botVisionPublisher, [Key("visionOrigin")] Transform visionOrigin)
        {
            this.botVisionPublisher = botVisionPublisher;
            this.visionOrigin = visionOrigin;
        }


        private bool IsObstructed(Collider other)
        {
            Vector3 start = visionOrigin.position;
            Vector3 end = other.bounds.center;

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
            for (int i = 0; i < targetsInRange.Count; i++)
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
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (now - lastVisibilityCheck < 100)
            {
                return;
            }

            CheckVisibilityOfTargets();
        }
    }
}