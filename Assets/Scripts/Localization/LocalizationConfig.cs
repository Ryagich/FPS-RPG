using UnityEngine;
using UnityEngine.Localization;

namespace Localization
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "configs/Localization/Localization Config")]
    public class LocalizationConfig : ScriptableObject
    {
        [field: Header("Pages")]
        [field: SerializeField] public LocalizedString PriceWord { get; private set; }
        [field: SerializeField] public LocalizedString InInventory { get; private set; }
        [field: Header("Popups")]
        [field: SerializeField] public LocalizedString ProductWord { get; private set; }
        [field: SerializeField] public LocalizedString DisableWord { get; private set; }
        [field: SerializeField] public LocalizedString ActivateWord { get; private set; }
        [field: SerializeField] public LocalizedString GrowStage { get; private set; }
        [field: SerializeField] public LocalizedString GrownWord { get; private set; }
        [field: SerializeField] public LocalizedString FruitsWord { get; private set; }
        [field: SerializeField] public LocalizedString ReadyWord { get; private set; }
        [field: SerializeField] public LocalizedString BuyersWord { get; private set; }
        [field: SerializeField] public LocalizedString GoToShop { get; private set; }

        [field: Header("Helps")]
        [field: SerializeField] public LocalizedString RKMHelpForChoseUI { get; private set; }
        [field: SerializeField] public LocalizedString HelpForPlaceBuilding { get; private set; }
        [field: SerializeField] public LocalizedString CancelBuilding { get; private set; }
        [field: SerializeField] public LocalizedString RotateRight { get; private set; }
        [field: SerializeField] public LocalizedString RotateLeft { get; private set; }
    }
}