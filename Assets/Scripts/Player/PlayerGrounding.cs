using UnityEngine;
using VContainer.Unity;

namespace Player
{
    /// <summary>
    /// Read-only observer of the final IK targets. It never writes to the rig or the animation graph.
    /// </summary>
    public sealed class FootGroundContactReader : ITickable
    {
        private const float FootRadius = .08f;
        // These values match the established FootPlacement IK query. IK targets are ankle pivots,
        // not foot soles, so a short "sole contact" cast reports false even on a flat floor.
        private const float CastStartOffset = .55f;
        private const float CastDistance = 2f;
        private const float MaxFootOffset = 1.4f;

        private readonly Animator animator;
        private readonly Transform playerTransform;
        private readonly PlayerMovementConfig config;
        private Transform rightFootTarget;
        private Transform leftFootTarget;

        public bool HasTargets => rightFootTarget != null && leftFootTarget != null;
        public bool HasGroundContact { get; private set; }

        public FootGroundContactReader(PlayerAvatarView avatarView, Transform playerTransform,
                                       PlayerMovementConfig config)
        {
            animator = avatarView.Animator;
            this.playerTransform = playerTransform;
            this.config = config;
            FindFootTargets();
        }

        public void Tick()
        {
            if (!HasTargets)
                FindFootTargets();

            HasGroundContact = HasTargets &&
                               (IsFootOnGround(rightFootTarget) || IsFootOnGround(leftFootTarget));
        }

        private void FindFootTargets()
        {
            foreach (var item in animator.GetComponentsInChildren<Transform>(true))
            {
                if (item.name == "IK RightFoot")
                    rightFootTarget = item;
                else if (item.name == "IK LeftFoot")
                    leftFootTarget = item;
            }
        }

        private bool IsFootOnGround(Transform footTarget)
        {
            var up = playerTransform.up;
            var origin = footTarget.position + up * CastStartOffset;
            if (!Physics.SphereCast(origin, FootRadius, -up, out var hit, CastDistance, config.GroundCheckMask,
                                    QueryTriggerInteraction.Ignore))
                return false;

            var footOffset = Vector3.Dot(hit.point - footTarget.position, up);
            return Mathf.Abs(footOffset) <= MaxFootOffset && Vector3.Angle(hit.normal, up) <= 45f;
        }
    }

    /// <summary>Separates immediate physical ground support from the delayed locomotion fall state.</summary>
    public sealed class PlayerGrounding : ITickable
    {
        private readonly CharacterController controller;
        private readonly Transform playerTransform;
        private readonly FootGroundContactReader footGroundContactReader;
        private readonly PlayerMovementConfig config;
        private float unsupportedDuration;
        private bool forceUngrounded;
        private bool jumpInProgress;
        private bool leftGroundSinceJump;

        public bool HasGroundSupport { get; private set; }
        public bool IsFalling { get; private set; }
        /// <summary>
        /// True only for the airborne phase that began with an accepted jump input.
        /// Falling from an edge deliberately does not set this flag.
        /// </summary>
        public bool IsJumpInitiated { get; private set; }
        /// <summary>Increments only for a jump that was accepted while supported.</summary>
        public int JumpStartedVersion { get; private set; }

        public PlayerGrounding(CharacterController controller, Transform playerTransform,
                               FootGroundContactReader footGroundContactReader, PlayerMovementConfig config)
        {
            this.controller = controller;
            this.playerTransform = playerTransform;
            this.footGroundContactReader = footGroundContactReader;
            this.config = config;
            HasGroundSupport = HasRawGroundSupport();
        }

        public void Tick()
        {
            // This value is updated by the previous CharacterController.Move. It is the only
            // valid landing confirmation while airborne, matching FPS_WA's IsInAir check.
            var isCapsuleGrounded = controller.isGrounded;
            var hasRawSupport = HasRawGroundSupport();
            HasGroundSupport = !forceUngrounded && hasRawSupport;

            if (forceUngrounded)
            {
                forceUngrounded = false;
                IsFalling = true;
                return;
            }

            if (jumpInProgress)
            {
                leftGroundSinceJump |= !isCapsuleGrounded;
                if (!leftGroundSinceJump || !isCapsuleGrounded)
                {
                    IsFalling = true;
                    return;
                }

                jumpInProgress = false;
                leftGroundSinceJump = false;
                unsupportedDuration = 0f;
                IsFalling = false;
                IsJumpInitiated = false;
                return;
            }

            // Once airborne, feet may already see a lower surface long before the controller lands.
            // Landing is therefore confirmed only by the physical capsule.
            if (IsFalling)
            {
                if (isCapsuleGrounded)
                {
                    unsupportedDuration = 0f;
                    IsFalling = false;
                }

                return;
            }

            // While still grounded, a planted IK foot prevents transient capsule gaps from
            // starting a fall. It is deliberately not used to exit the airborne state.
            var hasFootSupport = footGroundContactReader.HasTargets
                ? footGroundContactReader.HasGroundContact
                : hasRawSupport;
            if (hasFootSupport)
            {
                unsupportedDuration = 0f;
                IsFalling = false;
                return;
            }

            forceUngrounded = false;
            unsupportedDuration += Time.deltaTime;
            IsFalling = unsupportedDuration >= config.FallDelay;
        }

        public void BeginJump()
        {
            JumpStartedVersion++;
            IsJumpInitiated = true;
            forceUngrounded = true;
            HasGroundSupport = false;
            unsupportedDuration = config.FallDelay;
            IsFalling = true;
            jumpInProgress = true;
            leftGroundSinceJump = false;
        }

        private bool HasRawGroundSupport()
        {
            if (controller.isGrounded)
                return true;

            return HasWalkableGround(controller.skinWidth + config.GroundProbeDistance);
        }

        private bool HasWalkableGround(float distance)
        {
            var up = playerTransform.up;
            var center = playerTransform.TransformPoint(controller.center);
            var radius = Mathf.Max(0.01f, controller.radius * config.GroundProbeRadiusScale - controller.skinWidth);
            var capsuleBottomCenter = center - up * (controller.height * .5f - controller.radius);
            var origin = capsuleBottomCenter + up * controller.skinWidth;
            if (!Physics.SphereCast(origin, radius, -up, out var hit, distance, config.GroundCheckMask,
                                    QueryTriggerInteraction.Ignore))
                return false;

            return Vector3.Angle(hit.normal, up) <= controller.slopeLimit;
        }
    }
}
