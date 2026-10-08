using UnityEngine;
using Weapon.Providers;

namespace Characters
{
    public sealed class CharacterMovement
    {
        private readonly CharacterMovementConfig config;
        private readonly Transform transform;
        private readonly CharacterController controller;
        private readonly CharacterState state;
        private readonly WeaponProvider weapon;
        private readonly float standingHeight;
        private readonly Vector3 standingCenter;
        private readonly Collider[] standObstructions = new Collider[32];
        private Vector3 velocity;

        public CharacterMovement(CharacterMovementConfig config, Transform transform,
            CharacterController controller, CharacterState state, WeaponProvider weapon)
        {
            this.config = config;
            this.transform = transform;
            this.controller = controller;
            this.state = state;
            this.weapon = weapon;
            standingHeight = controller.height;
            standingCenter = controller.center;
        }

        public Vector3 GetVelocity(float deltaTime)
        {
            var crouching = state.CrouchRequested || !CanStandUp();
            state.SetCrouching(crouching);
            UpdateHeight(deltaTime);
            var crouchActive = crouching || state.CrouchProgress > 0f;
            var sprinting = controller.isGrounded && !crouchActive && state.SprintRequested
                && state.MoveRequest.y > 0f && !weapon.IsShooting()
                && !state.IsAiming && !weapon.IsReloading() && !weapon.IsChangingWeapon;
            state.SetSprinting(sprinting);
            state.MaxSpeed = GetSpeed(state.MoveRequest, crouchActive, sprinting);
            if (!controller.isGrounded)
                return velocity;

            var direction = transform.forward * state.MoveRequest.y + transform.right * state.MoveRequest.x;
            var targetVelocity = direction * state.MaxSpeed;
            var rates = crouchActive ? config.CrouchAccelerationRates
                : sprinting ? config.SprintAccelerationRates : config.WalkAccelerationRates;
            var acceleration = direction.sqrMagnitude > 0.001f ? rates.x : rates.y;
            velocity = Vector3.MoveTowards(velocity, targetVelocity, Mathf.Max(0f, acceleration) * deltaTime);
            return velocity;
        }

        // Navigation uses the same directional speed limits as the physical motor.
        public float GetSpeed(Vector2 direction, bool crouching, bool sprinting)
        {
            if (sprinting && !crouching && direction.y > 0f)
                return Mathf.Max(0f, config.SprintSpeed);
            var speeds = crouching ? config.CrouchSpeed : config.WalkSpeed;
            var forwardWeight = Mathf.Abs(direction.y);
            var sideWeight = Mathf.Abs(direction.x);
            var total = forwardWeight + sideWeight;
            if (total < 0.001f)
                return Mathf.Max(0f, speeds.x);
            return Mathf.Max(0f, ((direction.y >= 0f ? speeds.x : speeds.z) * forwardWeight
                + speeds.y * sideWeight) / total);
        }

        public float GetBrakingAcceleration(bool crouching, bool sprinting) => Mathf.Max(0.01f,
            (crouching ? config.CrouchAccelerationRates
                : sprinting ? config.SprintAccelerationRates : config.WalkAccelerationRates).y);

        private bool CanStandUp()
        {
            if (controller.height >= standingHeight - 0.001f)
                return true;
            var center = transform.TransformPoint(standingCenter);
            var radius = Mathf.Max(0.01f, controller.radius - controller.skinWidth);
            var halfSegment = Mathf.Max(0f, standingHeight * 0.5f - radius);
            var count = Physics.OverlapCapsuleNonAlloc(center - transform.up * (halfSegment - controller.skinWidth),
                center + transform.up * halfSegment, radius, standObstructions,
                config.CrouchCheckMask, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < count; i++)
            {
                if (!standObstructions[i].transform.IsChildOf(transform))
                    return false;
            }
            return count < standObstructions.Length;
        }

        private void UpdateHeight(float deltaTime)
        {
            var crouchingHeight = Mathf.Clamp(config.CrouchingHeight, controller.radius * 2f, standingHeight);
            var target = state.IsCrouching.Value
                ? crouchingHeight
                : standingHeight;
            controller.height = Mathf.MoveTowards(controller.height, target,
                Mathf.Max(0f, config.CrouchChangedSpeed) * deltaTime);
            controller.center = standingCenter + Vector3.up * ((controller.height - standingHeight) * 0.5f);
            state.CrouchProgress = Mathf.InverseLerp(standingHeight, crouchingHeight, controller.height);
        }
    }
}
