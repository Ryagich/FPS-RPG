using UnityEngine;
using UnityEngine.Localization;

namespace Inventory.Ammo
{
    [CreateAssetMenu(fileName = "Ammo Config", menuName = "configs/Weapons/Ammo")]
    public class AmmoConfig : ScriptableObject 
    {
        [field: SerializeField] public string ID { get; private set; } = "Ammo ID";
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public int Max { get; private set; } = 200;
    }
}