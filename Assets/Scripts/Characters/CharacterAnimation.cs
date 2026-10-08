using System;
using System.Collections.Generic;
using Inventory;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;

namespace Characters
{
    public sealed class CharacterAnimation : ILateTickable, IDisposable
    {
        private readonly Animator animator;
        private readonly CharacterState state;
        private readonly Inventory.Inventory inventory;
        private readonly Dictionary<int, AnimatorControllerParameterType> parameters = new();
        private readonly IDisposable subscriptions;
        private readonly float sprintSpeed;
        private readonly int rifleLayer;
        private readonly int pistolLayer;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int SpeedX = Animator.StringToHash("SpeedX");
        private static readonly int SpeedZ = Animator.StringToHash("SpeedZ");
        private static readonly int Crouching = Animator.StringToHash("IsCrouching");
        private static readonly int Grounded = Animator.StringToHash("IsGrounded");
        private static readonly int Sprinting = Animator.StringToHash("IsSprinting");
        private static readonly int RotationSpeed = Animator.StringToHash("RotationSpeed");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int VerticalAim = Animator.StringToHash("VerticalAim");
        private static readonly int Aiming = Animator.StringToHash("IsAiming");
        private static readonly int Reloading = Animator.StringToHash("IsReloading");

        public CharacterAnimation(Animator animator, CharacterState state, Inventory.Inventory inventory,
            CharacterMovementConfig movementConfig,
            ISubscriber<ShotFiredMessage> shot, ISubscriber<ReloadStartedMessage> reload,
            ISubscriber<ReloadFinishedMessage> reloadFinished,
            ISubscriber<WeaponChangeStartedMessage> changeStarted,
            ISubscriber<WeaponChangeFinishedMessage> changeFinished)
        {
            this.animator = animator;
            this.state = state;
            this.inventory = inventory;
            sprintSpeed = Mathf.Max(0.001f, movementConfig.SprintSpeed);
            if (animator.runtimeAnimatorController != null)
            {
                foreach (var parameter in animator.parameters)
                    parameters[parameter.nameHash] = parameter.type;
            }
            else
            {
                Debug.LogWarning("Character animation requires an Animator Controller. Physical control remains available.", animator);
            }
            rifleLayer = animator.runtimeAnimatorController != null ? animator.GetLayerIndex("RifleAim") : -1;
            pistolLayer = animator.runtimeAnimatorController != null ? animator.GetLayerIndex("PistolAim") : -1;
            var bag = DisposableBag.CreateBuilder();
            shot.Subscribe(_ => Trigger("Shoot")).AddTo(bag);
            reload.Subscribe(_ =>
            {
                SetState(Reloading, true);
                Trigger("Reload");
            }).AddTo(bag);
            reloadFinished.Subscribe(_ => SetState(Reloading, false)).AddTo(bag);
            changeStarted.Subscribe(_ => Trigger("StartChangeWeapon")).AddTo(bag);
            changeFinished.Subscribe(_ => Trigger("StopChangeWeapon")).AddTo(bag);
            subscriptions = bag.Build();
            inventory.SlotChanged += OnSlotChanged;
            OnSlotChanged(null, inventory.CurrentSlot);
        }

        private bool CanAnimate => animator != null && animator.isActiveAndEnabled
            && animator.runtimeAnimatorController != null;

        public void LateTick()
        {
            if (!CanAnimate)
                return;
            // FPS_Win's blend trees expect speed / sprint speed and a unit local direction.
            SetFloat(Speed, Mathf.Clamp01(state.HorizontalSpeed / sprintSpeed));
            var direction = new Vector2(state.LocalVelocity.x, state.LocalVelocity.z);
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
            SetFloat(SpeedX, direction.x);
            SetFloat(SpeedZ, direction.y);
            SetFloat(VerticalSpeed, state.Velocity.y);
            SetFloat(RotationSpeed, state.YawRate * Time.deltaTime);
            SetFloat(VerticalAim, -state.Pitch / 90f);
            SetFloat(Crouching, state.CrouchProgress);
            SetState(Grounded, state.IsGrounded);
            SetState(Sprinting, state.IsSprinting.Value);
            SetState(Aiming, state.IsAiming);
        }

        private void OnSlotChanged(InventorySlot previous, InventorySlot current)
        {
            if (!CanAnimate || current?.Item is not Weapon.Weapon weapon)
                return;
            var pistol = weapon.Config.Type == WeaponType.Pistol;
            if (rifleLayer >= 0)
                animator.SetLayerWeight(rifleLayer, pistol ? 0f : 1f);
            if (pistolLayer >= 0)
                animator.SetLayerWeight(pistolLayer, pistol ? 1f : 0f);
        }

        private void Trigger(string name)
        {
            var hash = Animator.StringToHash(name);
            if (CanAnimate && parameters.TryGetValue(hash, out var type) && type == AnimatorControllerParameterType.Trigger)
                animator.SetTrigger(hash);
        }

        private void SetFloat(int hash, float value)
        {
            if (parameters.TryGetValue(hash, out var type) && type == AnimatorControllerParameterType.Float)
                animator.SetFloat(hash, value);
        }

        private void SetState(int hash, bool value)
        {
            if (!CanAnimate || !parameters.TryGetValue(hash, out var type))
                return;
            if (type == AnimatorControllerParameterType.Bool)
                animator.SetBool(hash, value);
            else if (type == AnimatorControllerParameterType.Float)
                animator.SetFloat(hash, value ? 1f : 0f);
        }

        public void Dispose()
        {
            inventory.SlotChanged -= OnSlotChanged;
            subscriptions.Dispose();
        }
    }
}
