using VContainer;
using System;
using Characters;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Providers;
using Weapon.Settings;

namespace Bot
{
    // Temporary, deterministic command source. Disable PlayDemo to follow botGoal.
    public sealed class BotControlSource : ITickable, IDisposable
    {
        private enum Action
        {
            Idle, Move, Sprint, Crouch, Jump, TurnLeft, TurnRight, LookUp, LookDown,
            Aim, AimFire, Fire, Reload, Secondary, Primary, FireMode, Interact, Return
        }

        private readonly struct Step
        {
            public Action Action { get; }
            public Vector2 Offset { get; }
            public Step(Action action, float x = 0f, float z = 0f)
            {
                Action = action;
                Offset = new Vector2(x, z);
            }
        }

        private static readonly Step[] Steps =
        {
            new(Action.Idle),
            new(Action.Move, 0, 1), new(Action.Move, 0, -1),
            new(Action.Move, 1, 0), new(Action.Move, -1, 0),
            new(Action.Move, 1, 1), new(Action.Move, -1, -1),
            new(Action.Move, -1, 1), new(Action.Move, 1, -1),
            new(Action.Sprint, 0, 1), new(Action.Crouch),
            new(Action.Crouch, 0, 1), new(Action.Crouch, 0, -1),
            new(Action.Crouch, 1, 0), new(Action.Crouch, -1, 0),
            new(Action.Jump), new(Action.Jump, 0, 1),
            new(Action.TurnLeft), new(Action.TurnRight),
            new(Action.LookUp), new(Action.LookDown),
            new(Action.Aim), new(Action.AimFire), new(Action.Fire), new(Action.Reload),
            new(Action.Secondary), new(Action.Fire), new(Action.Reload), new(Action.FireMode),
            new(Action.Primary), new(Action.FireMode), new(Action.Interact), new(Action.Return)
        };

        private readonly Transform transform;
        private readonly Transform goal;
        private readonly BotNavigation navigation;
        private readonly BotNavigationSettings settings;
        private readonly CharacterState state;
        private readonly Inventory.Inventory inventory;
        private readonly WeaponProvider weapon;
        private readonly IPublisher<MoveCommand> move;
        private readonly IPublisher<LookCommand> look;
        private readonly IPublisher<SprintCommand> sprint;
        private readonly IPublisher<CrouchCommand> crouch;
        private readonly IPublisher<JumpCommand> jump;
        private readonly IPublisher<FireCommand> fire;
        private readonly IPublisher<AimCommand> aim;
        private readonly IPublisher<ReloadCommand> reload;
        private readonly IPublisher<SwitchWeaponCommand> switchWeapon;
        private readonly IPublisher<SwitchFireModeCommand> fireMode;
        private readonly IPublisher<InteractCommand> interact;
        private Vector3 origin;
        private Vector3 destination;
        private float elapsed;
        private int stepIndex = -1;
        private bool discreteActionSent;
        private bool disposed;

        public BotControlSource(Transform transform, [Key("botGoal")] Transform goal,
            BotNavigation navigation, BotNavigationSettings settings, CharacterState state,
            Inventory.Inventory inventory, WeaponProvider weapon,
            IPublisher<MoveCommand> move, IPublisher<LookCommand> look,
            IPublisher<SprintCommand> sprint, IPublisher<CrouchCommand> crouch,
            IPublisher<JumpCommand> jump, IPublisher<FireCommand> fire,
            IPublisher<AimCommand> aim, IPublisher<ReloadCommand> reload,
            IPublisher<SwitchWeaponCommand> switchWeapon, IPublisher<SwitchFireModeCommand> fireMode,
            IPublisher<InteractCommand> interact)
        {
            this.transform = transform;
            this.goal = goal;
            this.navigation = navigation;
            this.settings = settings;
            this.state = state;
            this.inventory = inventory;
            this.weapon = weapon;
            this.move = move;
            this.look = look;
            this.sprint = sprint;
            this.crouch = crouch;
            this.jump = jump;
            this.fire = fire;
            this.aim = aim;
            this.reload = reload;
            this.switchWeapon = switchWeapon;
            this.fireMode = fireMode;
            this.interact = interact;
        }

        public void Tick()
        {
            if (disposed || !inventory.IsReady || !weapon.IsReady || Time.deltaTime <= 0f)
                return;
            if (!settings.PlayDemo)
            {
                ResetHeldActions();
                if (goal != transform)
                    Navigate(goal.position, false, false, true);
                return;
            }
            if (stepIndex < 0)
            {
                origin = transform.position;
                BeginStep(0);
            }
            elapsed += Time.deltaTime;
            var step = Steps[stepIndex];
            var crouching = step.Action == Action.Crouch;
            var sprinting = step.Action == Action.Sprint;
            crouch.Publish(new CrouchCommand(crouching));
            sprint.Publish(new SprintCommand(sprinting));
            if (step.Offset.sqrMagnitude > 0f || step.Action == Action.Return)
                Navigate(destination, crouching, sprinting, step.Action == Action.Return);
            else
                move.Publish(new MoveCommand(Vector2.zero));

            var lookDelta = step.Action switch
            {
                Action.TurnLeft => new Vector2(-30f, 0f),
                Action.TurnRight => new Vector2(30f, 0f),
                Action.LookUp => new Vector2(0f, 15f),
                Action.LookDown => new Vector2(0f, -15f),
                _ => Vector2.zero
            };
            look.Publish(new LookCommand(lookDelta * Time.deltaTime));
            if (!discreteActionSent)
                SendAction(step.Action);

            var duration = Mathf.Max(0.5f, settings.DemoStepDuration);
            if (elapsed < duration || weapon.IsReloading() || weapon.IsChangingWeapon)
                return;
            BeginStep((stepIndex + 1) % Steps.Length);
        }

        private void Navigate(Vector3 target, bool crouching, bool sprinting, bool turn)
        {
            var yaw = turn ? navigation.GetYawDelta(target, Time.deltaTime) : 0f;
            var input = navigation.GetMove(target, crouching, sprinting, yaw, out var shouldJump);
            look.Publish(new LookCommand(new Vector2(yaw, 0f)));
            move.Publish(new MoveCommand(input));
            if (shouldJump)
                jump.Publish(new JumpCommand());
        }

        private void SendAction(Action action)
        {
            if (weapon.IsChangingWeapon)
                return;
            switch (action)
            {
                case Action.Jump:
                    if (!state.IsGrounded || (Steps[stepIndex].Offset.sqrMagnitude > 0f && elapsed < 0.25f))
                        return;
                    jump.Publish(new JumpCommand());
                    break;
                case Action.Aim:
                    aim.Publish(new AimCommand(true));
                    break;
                case Action.AimFire:
                    aim.Publish(new AimCommand(true));
                    fire.Publish(new FireCommand(true));
                    break;
                case Action.Fire:
                    fire.Publish(new FireCommand(true));
                    break;
                case Action.Reload:
                    reload.Publish(new ReloadCommand());
                    if (!weapon.IsReloading() && weapon.CanReload)
                        return;
                    break;
                case Action.Secondary:
                    switchWeapon.Publish(new SwitchWeaponCommand(WeaponRole.Secondary));
                    break;
                case Action.Primary:
                    switchWeapon.Publish(new SwitchWeaponCommand(WeaponRole.Primary));
                    break;
                case Action.FireMode:
                    fireMode.Publish(new SwitchFireModeCommand());
                    break;
                case Action.Interact:
                    interact.Publish(new InteractCommand());
                    break;
            }
            discreteActionSent = true;
        }

        private void BeginStep(int index)
        {
            ResetHeldActions();
            navigation.Reset();
            stepIndex = index;
            elapsed = 0f;
            discreteActionSent = false;
            var step = Steps[index];
            destination = transform.position + transform.TransformDirection(
                new Vector3(step.Offset.x, 0f, step.Offset.y).normalized * settings.DemoMoveDistance);
            if (step.Action == Action.Return)
                destination = goal != transform ? goal.position : origin;
            if (settings.RefillDemoAmmo && (step.Action == Action.Fire || step.Action == Action.AimFire)
                && inventory.CurrentSlot.Item is Weapon.Weapon activeWeapon)
                activeWeapon.TryChangeValue(activeWeapon.NeedAmmo());
            // Demo-only replenishment keeps the action loop observable indefinitely.
            if (index == 0 && settings.RefillDemoAmmo)
            {
                foreach (var slot in inventory.Slots)
                {
                    slot.Ammo?.ChangeValue(slot.Ammo.Max);
                    if (slot.Item is Weapon.Weapon slotWeapon)
                        slotWeapon.TryChangeValue(slotWeapon.NeedAmmo());
                }
            }
        }

        private void ResetHeldActions()
        {
            move.Publish(new MoveCommand(Vector2.zero));
            sprint.Publish(new SprintCommand(false));
            crouch.Publish(new CrouchCommand(false));
            fire.Publish(new FireCommand(false));
            aim.Publish(new AimCommand(false));
        }

        public void Dispose() => disposed = true;
    }
}
