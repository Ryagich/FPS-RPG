using UnityEngine;

namespace Weapon.Attachments
{
    public sealed class Scope : IAttachment
    {
        public AttachmentBaseInfo AttachmentBaseInfo { get; set; }
        public Transform Transform { get; }
        public GameObject GameObject => Transform.gameObject;
        public Camera ScopeCamera { get; }
        public Transform CenterTransform { get; }

        public Scope(Transform transform, AttachmentBaseInfo info, Camera scopeCamera, Transform centerTransform)
        {
            Transform = transform;
            AttachmentBaseInfo = info;
            ScopeCamera = scopeCamera;
            CenterTransform = centerTransform;
        }
    }
}