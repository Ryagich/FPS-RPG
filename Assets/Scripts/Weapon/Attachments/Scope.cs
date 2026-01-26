using UnityEngine;

namespace Weapon.Attachments
{
    public class Scope : MonoBehaviour, IAttachment
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; set; } = null!;
        [field: SerializeField] public UnityEngine.Camera ScopeCamera { get; private set; } = null!;
        [field: SerializeField] public Transform CenterTransform { get; private set; } = null!;
    }
}