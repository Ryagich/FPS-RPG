using Weapon.Animations;
using Weapon.Attachments;

namespace Weapon
{
    public sealed class WeaponPresentation
    {
        public AttachmentsController Attachments { get; }
        public WeaponBobbing Bobbing { get; }
        public WeaponRunBobbing RunBobbing { get; }
        public WeaponLowering Lowering { get; }
        public WeaponReloading Reloading { get; }

        public WeaponPresentation(AttachmentsController attachments, WeaponBobbing bobbing,
            WeaponRunBobbing runBobbing, WeaponLowering lowering, WeaponReloading reloading)
        {
            Attachments = attachments;
            Bobbing = bobbing;
            RunBobbing = runBobbing;
            Lowering = lowering;
            Reloading = reloading;
        }
    }
}
