using Sounds;
using UnityEngine;

namespace Inventory.Pools.Impact
{
    [CreateAssetMenu(fileName = "Impact Config", menuName = "configs/Inventory/Impact")]
    public class ImpactConfig : ScriptableObject
    {
        [field: SerializeField] public string Tag { get; private set; } = "Impact tag";
        [field: SerializeField] public ImpactType Type { get; private set; }
        [field: SerializeField] public float LifeTime { get; private set; } = 2.0f;
        [field: SerializeField] public GameObject Pref { get; private set; }
        [field: SerializeField] public SoundConfig SoundConfig { get; private set; }
    }
    
    public enum ImpactType
    {
        Default,
        Meat,
        Metal,
        Wood,
        Ground
    }
}