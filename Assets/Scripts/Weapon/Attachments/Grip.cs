using UnityEngine;

namespace Weapon.Attachments
{
    public class Grip : MonoBehaviour, IAttachment
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; set; } = null!;
        [field: SerializeField] public Transform LeftHandTarget { get; private set; } = null!;
    }
}