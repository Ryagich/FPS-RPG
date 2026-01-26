using Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace Weapon.Attachments
{
    [CreateAssetMenu(fileName = "AttachmentSectionInfo", menuName = "configs/Weapons/Attachments/AttachmentSectionInfo")]
    public class AttachmentSectionInfo : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; } = "AttachmentSectionInfo ID";
        [field: SerializeField] public LocalizedString NameKey { get; private set; } = null!;
        [field: SerializeField] public AttachmentTypes Type { get; private set; }

        public string SectionName => NameKey.GetLocalizedStringCached();
    }
}