
using UnityEngine;
using Weapon.Settings;

namespace Messages
{
    public readonly struct AimChangedMessage
    {
        public bool IsAimed { get; }
        public AimChangedMessage(bool isAimed)
        {
            IsAimed = isAimed;
        }
    }

    public readonly struct RecoilMessage
    {
        public Vector2 Recoil { get; }
        public ShakeSettings ShakeSettings { get; }
        public RecoilMessage(Vector2 recoil, ShakeSettings shakeSettings)
        {
            Recoil = recoil;
            ShakeSettings = shakeSettings;
        }
    }
}