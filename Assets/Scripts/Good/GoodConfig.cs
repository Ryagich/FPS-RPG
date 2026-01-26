using Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Good
{
    [CreateAssetMenu(fileName = "GoodConfig", menuName = "configs/Goods/GoodConfig")]
    public class GoodConfig : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public GoodType Type { get; private set; }
        [field: SerializeField] public LocalizedString NameKey { get; private set; }
        [field: SerializeField] public bool State { get; private set; }
     
        public string Name => NameKey.GetLocalizedStringCached();
    }
    
    public enum GoodType
    {
        Weapon,
        Character,
    }
}