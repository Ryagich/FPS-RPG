using Characters;
using Gravity;
using UnityEngine;
using UnityEngine.AI;

namespace Bot
{
    // NavMesh plans a route; only CharacterMotor changes the actual position.
    public sealed class BotNavigation
    {
        private readonly Transform transform;
        private readonly CharacterController controller;
        private readonly CharacterState state;
        private readonly CharacterMovement movement;
        private readonly CharacterMovementConfig movementConfig;
        private readonly BotNavigationSettings settings;
        private readonly NavMeshQueryFilter filter;
        private readonly GravityConfig gravityConfig;
        private readonly NavMeshPath path = new();
        private Vector3[] corners = System.Array.Empty<Vector3>();
        private Vector3 target;
        private Vector3 lastPosition;
        private int cornerIndex;
        private float nextRepath;
        private float stuckTime;
        private bool hasTarget;

        public BotNavigation(Transform transform, CharacterController controller, CharacterState state,
            CharacterMovement movement, CharacterMovementConfig movementConfig,
            BotNavigationSettings settings, GravityConfig gravityConfig)
        {
            this.transform = transform;
            this.controller = controller;
            this.state = state;
            this.movement = movement;
            this.movementConfig = movementConfig;
            this.settings = settings;
            this.gravityConfig = gravityConfig;
            filter = new NavMeshQueryFilter { agentTypeID = settings.AgentTypeId, areaMask = settings.AreaMask };
        }

        public void Reset()
        {
            hasTarget = false;
            corners = System.Array.Empty<Vector3>();
            stuckTime = 0f;
        }

        public Vector2 GetMove(Vector3 destination, bool crouching, bool sprinting, float yawDelta,
            out bool jump)
        {
            jump = false;
            if (!hasTarget || (destination - target).sqrMagnitude > 0.01f)
            {
                target = destination;
                hasTarget = true;
                nextRepath = 0f;
                stuckTime = 0f;
                lastPosition = transform.position;
            }
            if (Time.time >= nextRepath)
                Repath();
            if (cornerIndex >= corners.Length)
                return Vector2.zero;

            var position = transform.position;
            while (cornerIndex < corners.Length - 1
                && FlatDistance(position, corners[cornerIndex]) < settings.CornerDistance
                && Mathf.Abs(corners[cornerIndex].y - position.y) < controller.stepOffset + 0.1f)
                cornerIndex++;
            var offset = corners[cornerIndex] - position;
            var distance = new Vector2(offset.x, offset.z).magnitude;
            var lastCorner = cornerIndex == corners.Length - 1;
            var tolerance = lastCorner ? settings.ArrivalDistance : settings.CornerDistance;
            if (lastCorner && distance <= tolerance && Mathf.Abs(offset.y) < controller.stepOffset + 0.1f)
                return Vector2.zero;

            var planar = new Vector3(offset.x, 0f, offset.z);
            var basis = Quaternion.AngleAxis(yawDelta, Vector3.up) * transform.rotation;
            var local = Quaternion.Inverse(basis) * planar.normalized;
            var direction = new Vector2(local.x, local.z);
            var actualSprint = sprinting && !crouching && !state.IsAiming && direction.y > 0f;
            var maxSpeed = movement.GetSpeed(direction, crouching, actualSprint);
            var braking = movement.GetBrakingAcceleration(crouching, actualSprint);
            // Brake for the goal and sharp corners; the motor has finite acceleration.
            var cornerSpeed = 0f;
            if (!lastCorner)
            {
                var nextDirection = corners[cornerIndex + 1] - corners[cornerIndex];
                nextDirection.y = 0f;
                cornerSpeed = maxSpeed * Mathf.Clamp01(Vector3.Dot(planar.normalized, nextDirection.normalized));
            }
            var safeSpeed = Mathf.Sqrt(cornerSpeed * cornerSpeed
                + 2f * braking * Mathf.Max(0f, distance - tolerance));
            var speed = Mathf.Min(maxSpeed, safeSpeed);
            var velocity = new Vector3(state.Velocity.x, 0f, state.Velocity.z);
            var overshooting = Vector3.Dot(velocity, planar.normalized) > safeSpeed + 0.1f;
            var input = overshooting || maxSpeed <= 0.001f
                ? Vector2.zero : direction * Mathf.Clamp01(speed / maxSpeed);

            if (input.sqrMagnitude > 0.01f && state.IsGrounded)
            {
                stuckTime = FlatDistance(position, lastPosition) < 0.001f
                    ? stuckTime + Time.deltaTime : 0f;
                if (stuckTime >= settings.StuckTimeout)
                {
                    nextRepath = 0f;
                    stuckTime = 0f;
                    input = Vector2.zero;
                }
            }
            else
            {
                stuckTime = 0f;
            }
            lastPosition = position;
            // Link traversal uses the current takeoff velocity: there is no air steering in the motor.
            var gravity = Mathf.Max(0.01f, gravityConfig.Gravity);
            var impulse = Mathf.Sqrt(2f * gravity * Mathf.Max(0f, movementConfig.JumpHeight));
            var discriminant = impulse * impulse - 2f * gravity * offset.y;
            var flightTime = discriminant >= 0f ? (impulse + Mathf.Sqrt(discriminant)) / gravity : 0f;
            var jumpReach = state.HorizontalSpeed * flightTime;
            var crossesGap = NavMesh.SamplePosition(position, out var onMesh, settings.SampleRadius, filter)
                && NavMesh.Raycast(onMesh.position, corners[cornerIndex], out var edge, filter)
                && FlatDistance(position, edge.position) <= controller.radius + state.HorizontalSpeed * Time.deltaTime;
            var requiresJump = crossesGap;
            if (requiresJump && state.IsGrounded)
            {
                var velocityAligned = planar.sqrMagnitude > 0.001f && velocity.sqrMagnitude > 0.01f
                    && Vector3.Dot(velocity.normalized, planar.normalized) > 0.95f;
                jump = !crouching && discriminant >= 0f && velocityAligned && distance <= jumpReach;
                // An unreachable link must not drag the physical character off the route.
                if (!jump && crossesGap)
                    input = Vector2.zero;
            }
            return input;
        }

        public float GetYawDelta(Vector3 destination, float deltaTime)
        {
            var direction = destination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                return 0f;
            var desiredYaw = Quaternion.LookRotation(direction).eulerAngles.y;
            return Mathf.Clamp(Mathf.DeltaAngle(transform.eulerAngles.y, desiredYaw),
                -settings.TurnSpeed * deltaTime, settings.TurnSpeed * deltaTime);
        }

        private void Repath()
        {
            nextRepath = Time.time + Mathf.Max(0.05f, settings.RepathInterval);
            if (!NavMesh.SamplePosition(transform.position, out var start, settings.SampleRadius, filter)
                || !NavMesh.SamplePosition(target, out var end, settings.SampleRadius, filter)
                || !NavMesh.CalculatePath(start.position, end.position, filter, path)
                || path.status == NavMeshPathStatus.PathInvalid)
            {
                corners = System.Array.Empty<Vector3>();
                cornerIndex = 0;
                return;
            }
            // Partial paths end at the nearest reachable point. Never move off the mesh to the raw target.
            corners = path.corners;
            cornerIndex = corners.Length > 1 ? 1 : corners.Length;
        }

        private static float FlatDistance(Vector3 first, Vector3 second)
            => new Vector2(first.x - second.x, first.z - second.z).magnitude;
    }
}
