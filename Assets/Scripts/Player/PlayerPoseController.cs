using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Player
{
    /// <summary>Physical pose of the player. The airborne pose is selected exclusively from PlayerGrounding.</summary>
    public enum PlayerPose
    {
        Standing,
        Crouching,
        Prone,
        Airborne
    }

    /// <summary>
    /// Владелец позы игрока: collider, высота камеры и параметры Animator.
    /// Намерения приходят из нашего MessagePipe; demo input Kinemation не используется.
    /// </summary>
    public sealed class PlayerPoseController : ITickable
    {
        private static readonly int Crouching = Animator.StringToHash("Crouching");
        private static readonly int Proning = Animator.StringToHash("Proning");

        private readonly PlayerMovementConfig config;
        private readonly CharacterController controller;
        private readonly Transform playerTransform;
        private readonly Transform cameraParent;
        private readonly MoveStates states;
        private readonly PlayerGrounding grounding;
        private readonly Animator animator;
        private readonly PlayerCameraSocket cameraSocket;
        private readonly float standingHeight;
        private readonly Vector3 standingCenter;
        private readonly float colliderBottomOffset;
        private readonly float standingCameraY;

        private bool crouchHeld;
        public PlayerPose CurrentPose { get; private set; }

        public PlayerPoseController(
            PlayerMovementConfig config,
            CharacterController controller,
            Transform playerTransform,
            [Key("CameraParentTransform")] Transform cameraParent,
            MoveStates states,
            PlayerGrounding grounding,
            PlayerAvatarView avatarView,
            PlayerCameraSocket cameraSocket,
            ISubscriber<ChangeCrouchingStateMessage> crouchSubscriber,
            ISubscriber<ChangeProneStateMessage> proneSubscriber)
        {
            this.config = config;
            this.controller = controller;
            this.playerTransform = playerTransform;
            this.cameraParent = cameraParent;
            this.states = states;
            this.grounding = grounding;
            animator = avatarView.Animator;
            this.cameraSocket = cameraSocket;
            standingHeight = controller.height;
            standingCenter = controller.center;
            colliderBottomOffset = controller.center.y - standingHeight * .5f;
            standingCameraY = cameraParent.localPosition.y;

            crouchSubscriber.Subscribe(OnCrouchChanged);
            proneSubscriber.Subscribe(OnProneRequested);
        }

        public void Tick()
        {
            // Do not resolve a standing/crouching shape during a jump. PlayerGrounding is the
            // single authority for entering and leaving the airborne pose.
            if (!grounding.IsFalling && !states.IsProne.Value && states.IsCrouching.Value && !crouchHeld &&
                CanExpandTo(standingHeight))
                states.IsCrouching.Value = false;

            CurrentPose = ResolvePose();
            var targetHeight = GetTargetHeight(CurrentPose);
            var targetCameraY = CurrentPose == PlayerPose.Prone ? config.CameraPositionInProne
                : CurrentPose == PlayerPose.Crouching ? config.CameraPositionInCrouching
                : standingCameraY;
            var speed = GetHeightChangeSpeed(CurrentPose);

            if (CurrentPose != PlayerPose.Airborne && targetHeight > controller.height && !CanExpandTo(targetHeight))
            {
                // A jump can finish beneath an obstacle. Convert that physical restriction into
                // the same crouching pose used by input, instead of retaining an orphaned
                // airborne-sized capsule while the animator believes the player is standing.
                states.IsCrouching.Value = true;
                CurrentPose = PlayerPose.Crouching;
                targetHeight = GetTargetHeight(CurrentPose);
                speed = GetHeightChangeSpeed(CurrentPose);
            }

            var height = Mathf.MoveTowards(controller.height, targetHeight, speed * Time.deltaTime);
            controller.height = height;
            // Same geometry rule as the FPS Animation Framework demo: the collider changes only
            // for stances. Airborne is animation/locomotion state, not a capsule shape.
            controller.center = GetCenterAtHeight(height);
            // При активном CameraSocket позиция камеры берётся из анимированной головы,
            // как в demo Kinemation. Camera parent продолжает владеть только pitch/bob-offset.
            if (!cameraSocket.UsesAvatarSocket)
            {
                var cameraPosition = cameraParent.localPosition;
                cameraParent.localPosition = new Vector3(cameraPosition.x,
                                                          Mathf.MoveTowards(cameraPosition.y, targetCameraY, speed * Time.deltaTime),
                                                          cameraPosition.z);
            }

            animator.SetBool(Crouching, CurrentPose == PlayerPose.Crouching);
            animator.SetBool(Proning, CurrentPose == PlayerPose.Prone);
        }

        private PlayerPose ResolvePose()
        {
            if (grounding.IsFalling)
                return PlayerPose.Airborne;

            if (states.IsProne.Value)
                return PlayerPose.Prone;

            // If there is no headroom after a landing, crouching becomes the actual pose even
            // when the crouch key is no longer held. It will return to standing once CanExpandTo
            // succeeds on a later tick.
            if (states.IsCrouching.Value || !CanExpandTo(standingHeight))
            {
                states.IsCrouching.Value = true;
                return PlayerPose.Crouching;
            }

            return PlayerPose.Standing;
        }

        private float GetTargetHeight(PlayerPose pose)
        {
            return pose switch
            {
                PlayerPose.Prone => config.ProneHeight,
                PlayerPose.Crouching => config.CrouchingHeight,
                PlayerPose.Airborne => standingHeight,
                _ => standingHeight
            };
        }

        private float GetHeightChangeSpeed(PlayerPose pose)
        {
            if (pose == PlayerPose.Prone)
                return config.ProneChangedSpeed;
            if (pose == PlayerPose.Crouching)
                return config.CrouchChangedSpeed;
            return config.CrouchChangedSpeed;
        }

        private Vector3 GetCenterAtHeight(float height)
        {
            return new Vector3(standingCenter.x, height * .5f + colliderBottomOffset, standingCenter.z);
        }

        private void OnCrouchChanged(ChangeCrouchingStateMessage message)
        {
            crouchHeld = message.State;
            if (states.IsProne.Value)
            {
                if (message.State)
                    LeaveProneToCrouch();
                return;
            }

            if (message.State)
                states.IsCrouching.Value = true;
        }

        private void OnProneRequested(ChangeProneStateMessage message)
        {
            if (states.IsProne.Value)
            {
                LeaveProneToCrouch();
                return;
            }

            states.IsProne.Value = true;
            states.IsCrouching.Value = false;
        }

        private void LeaveProneToCrouch()
        {
            states.IsProne.Value = false;
            states.IsCrouching.Value = true;
        }

        private bool CanExpandTo(float targetHeight)
        {
            var radius = controller.radius;
            var bottom = playerTransform.position + Vector3.up * (colliderBottomOffset + radius + controller.skinWidth);
            var top = playerTransform.position + Vector3.up * (colliderBottomOffset + targetHeight - radius);
            return !Physics.CheckCapsule(bottom, top, radius, config.CrouchCheckMask, QueryTriggerInteraction.Ignore);
        }
    }
}
