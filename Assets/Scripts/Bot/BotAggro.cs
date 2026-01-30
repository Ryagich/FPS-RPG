using System;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Bot
{
    public enum AggroTargetType
    {
        Enemy
    }

    public struct AggroTarget
    {
        public readonly Collider target;
        public readonly AggroTargetType targetType;
        
        public AggroTarget(Collider target, AggroTargetType targetType)
        {
            this.target = target;
            this.targetType = targetType;
        }
    }

    public class BotAggro : IStartable
    {
        private BotController controller;

        [Inject]
        public BotAggro(ISubscriber<BotVisionMessage> botVisionSubscriber, BotController controller)
        {
            this.controller = controller;
            botVisionSubscriber.Subscribe(OnVisionEntered);
        }

        public void Start()
        {
            
        }

        // TODO: Логика определения корректного объекта агрессии
        private bool IsValidAggroTarget(BotVisionMessage message)
        {
            return true;
        }

        //TODO: Логика переопределения объекта агрессии и выбора из нескольких
        private void OnVisionEntered(BotVisionMessage message)
        {
            if(!message.isVisible && controller.context.aggroTarget != null 
                && controller.context.aggroTarget.Value.target == message.target)
            {
                Debug.Log("TARGET LOST");
                controller.context.aggroTarget = null;
                return;
            }
            if(message.isVisible && IsValidAggroTarget(message) && controller.context.aggroTarget == null)
            {
                controller.context.aggroTarget = new AggroTarget(message.target, AggroTargetType.Enemy);
                Debug.Log("Aggro target is set to: " + controller.context.aggroTarget.Value.target.name);
            }
        }
    }
}
