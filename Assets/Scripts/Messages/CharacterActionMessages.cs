using UnityEngine;

namespace Messages
{
    // Results of accepted actions, never raw input requests.
    public readonly struct ShotFiredMessage
    {
        public global::Weapon.Weapon Weapon { get; }
        public Quaternion Rotation { get; }
        public ShotFiredMessage(global::Weapon.Weapon weapon, Quaternion rotation)
        {
            Weapon = weapon;
            Rotation = rotation;
        }
    }

    public readonly struct ReloadStartedMessage { }
    public readonly struct ReloadFinishedMessage
    {
        public bool Cancelled { get; }
        public ReloadFinishedMessage(bool cancelled) => Cancelled = cancelled;
    }
    public readonly struct WeaponChangeStartedMessage { }
    public readonly struct WeaponChangeFinishedMessage { }
}
