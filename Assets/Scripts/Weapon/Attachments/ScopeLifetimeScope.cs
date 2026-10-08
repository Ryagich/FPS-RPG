using UnityEngine;
using VContainer;

namespace Weapon.Attachments
{
    public sealed class ScopeLifetimeScope : AttachmentLifetimeScope
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; private set; }
        [field: SerializeField] public Camera ScopeCamera { get; private set; }
        [field: SerializeField] public Transform CenterTransform { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(AttachmentBaseInfo);
            var attachment = new Scope(transform, AttachmentBaseInfo, ScopeCamera, CenterTransform);
            Instance = attachment;
            builder.RegisterInstance(attachment);
        }
    }
}
