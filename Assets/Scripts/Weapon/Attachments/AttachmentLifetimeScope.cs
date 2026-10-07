using Scopes;
using VContainer.Unity;

namespace Weapon.Attachments
{
    public abstract class AttachmentLifetimeScope : EntityLifetimeScope
    {
        public IAttachment Instance { get; protected set; }
    }
}
