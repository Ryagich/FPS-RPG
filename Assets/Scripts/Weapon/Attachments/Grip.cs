using UnityEngine;

namespace Weapon.Attachments
{
    public sealed class Grip : IAttachment
    {
        public AttachmentBaseInfo AttachmentBaseInfo { get; set; }
        public Transform Transform { get; }
        public GameObject GameObject => Transform.gameObject;
        public Transform LeftHandTarget { get; }

        public Grip(Transform transform, AttachmentBaseInfo info, Transform leftHandTarget)
        {
            Transform = transform;
            AttachmentBaseInfo = info;
            LeftHandTarget = leftHandTarget;
        }
    }
}