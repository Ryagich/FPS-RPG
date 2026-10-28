using MessagePipe;
using Messages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Player
{
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
        private readonly Animator animator;
        private readonly PlayerCameraSocket cameraSocket;
        private readonly float standingHeight;
        private readonly float colliderBottomOffset;
        private readonly float standingCameraY;

        private bool crouchHeld;

        public PlayerPoseController(
            PlayerMovementConfig config,
            CharacterController controller,
            Transform playerTransform,
            [Key("CameraParentTransform")] Transform cameraParent,
            MoveStates states,
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
            animator = avatarView.Animator;
            this.cameraSocket = cameraSocket;
            standingHeight = controller.height;
            colliderBottomOffset = controller.center.y - standingHeight * .5f;
            standingCameraY = cameraParent.localPosition.y;

            crouchSubscriber.Subscribe(OnCrouchChanged);
            proneSubscriber.Subscribe(OnProneRequested);
        }

        public void Tick()
        {
            if (!states.IsProne.Value && states.IsCrouching.Value && !crouchHeld && CanExpandTo(standingHeight))
                states.IsCrouching.Value = false;

            var isProne = states.IsProne.Value;
            var isCrouching = states.IsCrouching.Value;
            var targetHeight = isProne ? config.ProneHeight : isCrouching ? config.CrouchingHeight : standingHeight;
            var targetCameraY = isProne ? config.CameraPositionInProne : isCrouching ? config.CameraPositionInCrouching : standingCameraY;
            var speed = isProne ? config.ProneChangedSpeed : config.CrouchChangedSpeed;

            var height = Mathf.MoveTowards(controller.height, targetHeight, speed * Time.deltaTime);
            controller.height = height;
            controller.center = new Vector3(controller.center.x, height * .5f + colliderBottomOffset, controller.center.z);
            // При активном CameraSocket позиция камеры берётся из анимированной головы,
            // как в demo Kinemation. Camera parent продолжает владеть только pitch/bob-offset.
            if (!cameraSocket.UsesAvatarSocket)
            {
                var cameraPosition = cameraParent.localPosition;
                cameraParent.localPosition = new Vector3(cameraPosition.x,
                                                          Mathf.MoveTowards(cameraPosition.y, targetCameraY, speed * Time.deltaTime),
                                                          cameraPosition.z);
            }

            animator.SetBool(Crouching, isCrouching);
            animator.SetBool(Proning, isProne);
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
