using Inventory;
using MessagePipe;
using Messages;
using UnityEngine;

namespace Player.Stats
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StatsController
    {
        private readonly IPublisher<DeathMessage> deathMessagePublisher;
        public bool IsAlive => Hp.Value > .0f;
        public bool IsHpMax => Hp.Max.Equals(Hp.Value);
        private readonly Stat Hp;

        public StatsController
            (
                StatsConfig statsConfig,
                IPublisher<DeathMessage> deathMessagePublisher
            )
        {
            this.deathMessagePublisher = deathMessagePublisher;
            Hp = new Stat(statsConfig.HpStat);
        }
        
        public void AddHealth(float amount)
        {
            if (amount < 0)
            {
                Debug.LogError($"Cannot add health to a negative amount of {amount}");
                return;
            }

            if (Hp.Value >= Hp.Max)
            {
                return;
            }

            Hp.AddValue(amount);

            if (Hp.Value >= Hp.Max)
            {
                //
            }
        }
        
        public bool TryAddHealth(float amount)
        {
            if (!IsAlive || Hp.Value <= .0f)
            {
                return false;
            }
            
            Hp.AddValue(amount);
            return true;
        }
        
        //Влзможно передавать нанесшего урон, например для отображения убийцы в UI - хз
        public void TakeDamage(float amount)
        {
            if (amount < 0)
            {
                Debug.Log($"HP {Hp.Value}");
                Debug.LogError($"Cannot subtract health to a negative amount of {amount}");
                return;
            }

            Hp.AddValue(-amount);
            //Костыль для защиты от низких значений float <- они ломают игру
            if (Hp.Value <= .4f)
                Hp.AddValue(-1.0f);
            if (!IsAlive)
            {
                deathMessagePublisher.Publish(new DeathMessage());
            }
            Debug.Log($"HP {Hp.Value}");
        }
    }
}