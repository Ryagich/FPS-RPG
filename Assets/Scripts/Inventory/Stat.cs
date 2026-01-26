using System;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class Stat
    {
        //was | now
        public event Action<float, float> ValueChanged;
        [SerializeField] private float max;
        [SerializeField] private float min;
        [SerializeField] private float value;

        public float Max => max;
        public float Min => min;
        public float Value => value;

        public bool IsMax => Value.Equals(Max);
        public bool IsEmpty => Value.Equals(.0f);

        public Stat(float max, float min, float value)
        {
            this.max = max;
            this.min = min;
            this.value = value;
        }

        public Stat(Stat stat)
        {
            max = stat.max;
            min = stat.min;
            value = stat.value;
        }

        public void AddValue(float newValue)
        {
            var old = Value;
            value = Mathf.Clamp(Value + newValue, Min, Max);
            if (!old.Equals(Value))
            {
                ValueChanged?.Invoke(old, Value);
            }
        }

        public void ChangeValue(float newValue)
        {
            var old = Value;
            value = Mathf.Clamp(newValue, Min, Max);
            if (!old.Equals(Value))
            {
                ValueChanged?.Invoke(old, Value);
            }
        }

        public void ChangeMax(float newMax)
        {
            max = newMax;
        }
    }
}