using System;
using MessagePipe;
using Messages;
using UniRx;
using VContainer.Unity;
using Weapon.Providers;

namespace Characters
{
    public sealed class CharacterWeaponController : IStartable, IDisposable
    {
        private readonly IDisposable subscriptions;

        public CharacterWeaponController(WeaponProvider weapon, CharacterState state,
            ISubscriber<FireCommand> fire, ISubscriber<AimCommand> aim,
            ISubscriber<ReloadCommand> reload, ISubscriber<SwitchWeaponCommand> switchWeapon,
            ISubscriber<SwitchFireModeCommand> switchFireMode)
        {
            var bag = DisposableBag.CreateBuilder();
            fire.Subscribe(command =>
            {
                if (command.State)
                    weapon.StartShooting();
                else
                    weapon.StopShooting();
            }).AddTo(bag);
            aim.Subscribe(command => weapon.SetAim(command.State)).AddTo(bag);
            reload.Subscribe(_ => weapon.TryReload()).AddTo(bag);
            switchWeapon.Subscribe(command => weapon.ChangeWeapon(command.Role)).AddTo(bag);
            switchFireMode.Subscribe(_ => weapon.TrySwitchShootingMode()).AddTo(bag);
            state.IsSprinting.Subscribe(sprinting =>
            {
                if (sprinting)
                    weapon.StartSprint();
                else
                    weapon.StopSprint();
            }).AddTo(bag);
            subscriptions = bag.Build();
        }

        public void Start() { }
        public void Dispose() => subscriptions.Dispose();
    }
}
