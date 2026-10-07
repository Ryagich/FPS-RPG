using System;
using CameraScripts;
using Inventory;
using MessagePipe;
using Messages;
using Player;
using UnityEngine;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;

namespace Bot
{
    public struct ReloadMessage
    {
    }
    
    public struct StartWeaponChangeMessage
    {
    }
    
    public struct StopWeaponChangeMessage
    {
    }

    public class BotAnimation : ITickable
    {
        private PlayerMovement movement;
        private PlayerCamera playerCamera;
        private Transform leftTarget;
        private Transform rightTarget;
      
        private Animator animator = null!;
        private IDisposable subscription = null!;

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int SpeedXParam = Animator.StringToHash("SpeedX");
        private static readonly int SpeedZParam = Animator.StringToHash("SpeedZ");
        private static readonly int IsCrouchingParam = Animator.StringToHash("IsCrouching");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int RotationSpeedParam = Animator.StringToHash("RotationSpeed");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int VerticalAim = Animator.StringToHash("VerticalAim");
        private static readonly int Shoot = Animator.StringToHash("Shoot");
        private static readonly int IsAiming = Animator.StringToHash("IsAiming");
        private static readonly int StartChangeWeapon = Animator.StringToHash("StartChangeWeapon");
        private static readonly int StopChangeWeapon = Animator.StringToHash("StopChangeWeapon");
        private static readonly int Reload = Animator.StringToHash("Reload");

        public BotAnimation(
            Animator animator,
            PlayerMovement movement,
            ISubscriber<AimChangedMessage> aimChangedSubscriber,
            ISubscriber<ReloadMessage> reloadSubscriber,
            ISubscriber<StartWeaponChangeMessage> startWeaponChangeSubscriber,
            ISubscriber<StopWeaponChangeMessage> stopWeaponChangeSubscriber,
            ISubscriber<ShootMessage> shootMessageSubscriber)
        {
            Debug.Log("Construct");
            var bag = DisposableBag.CreateBuilder();

            aimChangedSubscriber.Subscribe(OnAimChanged).AddTo(bag);
            reloadSubscriber.Subscribe(OnReload).AddTo(bag);
            startWeaponChangeSubscriber.Subscribe(OnStartChangeWeapon).AddTo(bag);
            stopWeaponChangeSubscriber.Subscribe(OnStopChangeWeapon).AddTo(bag);
            shootMessageSubscriber.Subscribe(OnShoot).AddTo(bag);

            subscription = bag.Build();
        }

        private void OnShoot(ShootMessage _)
        {
            Debug.Log("OnShoot");
            animator.SetTrigger(Shoot);
        }

        private void OnReload(ReloadMessage obj)
        {
            Debug.Log("OnReload");
            animator.SetTrigger(Reload);
        }

        private void OnAimChanged(AimChangedMessage message)
        {
            Debug.Log("OnAimChanged");
            animator.SetBool(IsAiming, message.IsAiming);
        }

        private void OnStartChangeWeapon(StartWeaponChangeMessage _)
        {
            Debug.Log("OnStartChangeWeapon");
            animator.SetTrigger(StartChangeWeapon);
        }

        private void OnStopChangeWeapon(StopWeaponChangeMessage _)
        {
            Debug.Log("OnStopChangeWeapon");
            animator.SetTrigger(StopChangeWeapon);
        }

        private void OnSlotChanged(InventorySlot prev, InventorySlot next)
        {
            Debug.Log("OnSlotChanged");
            
            if (next.Item is not Weapon.Weapon weapon)
            {
                return;
            }

            // var ikPoints = weapon!.GetComponent<IKPointsInWeapon>();
            // if (ikPoints!.isActiveAndEnabled && !ikPoints)
            // {
            //     return;
            // }
            //
            // Snap(leftTarget, ikPoints.LeftTarget);
            // Snap(rightTarget, ikPoints.RightTarget);
            // TODO: вернуть IK

            SetWeaponTypeAnimation(weapon);
        }

        private void SetWeaponTypeAnimation(Weapon.Weapon weapon)
        {
            Debug.Log("SetWeaponTypeAnimation");
            
            var rifleAim = 1;
            var pistolAim = 2;

            if (weapon.Config.Type == WeaponType.Pistol)
            {
                animator.SetLayerWeight(rifleAim, 0f);
                animator.SetLayerWeight(pistolAim, 1f);
            }
            else
            {
                animator.SetLayerWeight(pistolAim, 0f);
                animator.SetLayerWeight(rifleAim, 1f);
            }
        }

        private void Snap(Transform child, Transform parent)
        {
            child.SetParent(parent);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        public void Tick()
        {
            var horizontalVelocity = movement.GetHorizontalSpeed() / movement.GetMaxHorizontalSpeed();
            animator.SetFloat(SpeedParam, horizontalVelocity);

            var direction = movement.GetLocalMovement();
            animator.SetFloat(SpeedXParam, direction.x);
            animator.SetFloat(SpeedZParam, direction.z);

            var crouching = movement.IsCrouching ? 1f : 0;
            animator.SetFloat(IsCrouchingParam, crouching);

            var yaw = playerCamera.XRotationRate;
            var verticalAim = playerCamera.XRotation / -90f;
            animator.SetFloat(RotationSpeedParam, yaw);
            animator.SetFloat(VerticalAim, verticalAim);

            animator.SetFloat(VerticalSpeed, direction.y);
            animator.SetBool(IsGrounded, movement.IsGrounded);
        }

        private void OnDestroy()
        {
            subscription.Dispose();
        }
    }
}