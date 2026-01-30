using Inventory;
using UnityEngine;

namespace Player.Stats
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StatsController
    {
        public bool IsAlive => Hp.Value > .0f;
        public bool IsHpMax => Hp.Max.Equals(Hp.Value);
        private readonly Stat Hp;

        public StatsController(StatsConfig statsConfig)
        {
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
                Debug.LogError($"Cannot subtract health to a negative amount of {amount}");
                return;
            }

            Hp.AddValue(-amount);
            //Костыль для защиты от низких значений float <- они ломают игру
            if (Hp.Value <= 0.4f)
                Hp.AddValue(-1f);
            if (!IsAlive)
            {
                //
            }
        }
    }
}