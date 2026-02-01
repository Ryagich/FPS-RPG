
using UnityEngine;
using Weapon.Settings;

namespace Messages
{
    public readonly struct AimChangedMessage
    {
        public readonly bool State;
        public AimChangedMessage(bool state)
        {
            State = state;
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