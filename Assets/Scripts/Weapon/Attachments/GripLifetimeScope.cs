using UnityEngine;
using VContainer;

namespace Weapon.Attachments
{
    public sealed class GripLifetimeScope : AttachmentLifetimeScope
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; private set; }
        [field: SerializeField] public Transform LeftHandTarget { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            var attachment = new Grip(transform, AttachmentBaseInfo, LeftHandTarget);
            Instance = attachment;
            builder.RegisterInstance(attachment);
        }
    }
}
