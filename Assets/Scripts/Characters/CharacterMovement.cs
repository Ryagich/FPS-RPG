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
            // Use resolved movement so contacts cannot leave hidden momentum in the motor.
            velocity = new Vector3(state.Velocity.x, 0f, state.Velocity.z);
            var input = Vector2.ClampMagnitude(state.MoveRequest, 1f);
            var direction = transform.forward * input.y + transform.right * input.x;
            var speed = state.MaxSpeed;
            if (!controller.isGrounded)
            {
                // Coast without input; steering retains takeoff speed without adding sprint speed in midair.
                if (input.sqrMagnitude < 0.000001f)
                    return velocity;
                speed = Mathf.Max(speed, velocity.magnitude);
            }
            var targetVelocity = direction * speed;
            var rates = !controller.isGrounded ? config.AirAccelerationRates
                : crouchActive ? config.CrouchAccelerationRates
                : sprinting ? config.SprintAccelerationRates : config.WalkAccelerationRates;
            velocity = UpdateVelocity(velocity, targetVelocity, rates, deltaTime);
            return velocity;
        }

        private static Vector3 UpdateVelocity(Vector3 current, Vector3 target, Vector2 rates, float deltaTime)
        {
            var acceleration = Mathf.Max(0f, rates.x);
            var braking = Mathf.Max(0f, rates.y);
            var targetSpeed = target.magnitude;
            if (targetSpeed < 0.001f)
                return Vector3.MoveTowards(current, Vector3.zero, braking * deltaTime);

            var direction = target / targetSpeed;
            var forwardSpeed = Vector3.Dot(current, direction);
            var sidewaysVelocity = current - direction * forwardSpeed;
            sidewaysVelocity = Vector3.MoveTowards(sidewaysVelocity, Vector3.zero, braking * deltaTime);
            // Cancel motion against the command before accelerating in the requested direction.
            var accelerationTime = deltaTime;
            if (forwardSpeed < 0f)
            {
                accelerationTime = braking > 0f ? Mathf.Max(0f, deltaTime + forwardSpeed / braking) : 0f;
                forwardSpeed = Mathf.MoveTowards(forwardSpeed, 0f, braking * deltaTime);
            }
            forwardSpeed = Mathf.MoveTowards(forwardSpeed, targetSpeed,
                (forwardSpeed > targetSpeed ? braking : acceleration) * accelerationTime);
            var result = direction * forwardSpeed + sidewaysVelocity;
            // Turning must not create speed by accelerating while lateral momentum is still present.
            return Vector3.ClampMagnitude(result, Mathf.Max(current.magnitude, targetSpeed));
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
