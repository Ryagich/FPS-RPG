using UnityEngine;
using Weapon.Settings;

namespace Messages
{
    // Intent, in character-local space. Brokers belong to the character scope.
    public readonly struct MoveCommand
    {
        public Vector2 Direction { get; }
        public MoveCommand(Vector2 direction) => Direction = Vector2.ClampMagnitude(direction, 1f);
    }
    public readonly struct LookCommand
    {
        // Yaw and pitch input in degrees, independent of the input device.
        public Vector2 Delta { get; }
        public LookCommand(Vector2 delta) => Delta = delta;
    }
    public readonly struct SprintCommand
    {
        public bool State { get; }
        public SprintCommand(bool state) => State = state;
    }
    public readonly struct CrouchCommand
    {
        public bool State { get; }
        public CrouchCommand(bool state) => State = state;
    }
    public readonly struct FireCommand
    {
        public bool State { get; }
        public FireCommand(bool state) => State = state;
    }
    public readonly struct AimCommand
    {
        public bool State { get; }
        public AimCommand(bool state) => State = state;
    }
    public readonly struct JumpCommand { }
    public readonly struct ReloadCommand { }
    public readonly struct SwitchFireModeCommand { }
    public readonly struct InteractCommand { }
    public readonly struct SwitchWeaponCommand
    {
        public WeaponRole Role { get; }
        public SwitchWeaponCommand(WeaponRole role) => Role = role;
    }
}
