using Inventory;
using UnityEngine;

namespace Player.Stats
{
    [CreateAssetMenu(fileName = "StatsConfig", menuName = "configs/Stats/StatsConfig")]
    public class StatsConfig : ScriptableObject
    {
        [field: SerializeField] public Stat HpStat { get; private set; }
    }
}