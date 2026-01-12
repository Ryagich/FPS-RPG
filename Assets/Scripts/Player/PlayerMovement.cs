using MessagePipe;
using Messages;
using UnityEngine;
using Utils;
using VContainer;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerMovement
    {
        private readonly PlayerMovementConfig playerMovementConfig;
        private readonly Transform playerTransform;
        private readonly Transform cameraParentTransform;
        private readonly CharacterController characterController;

        private readonly float characterHeight;
        private readonly float cameraDefaultLocalPosition;
        private Vector2 direction;
        private Vector3 currentVelocity;
        
        private bool isSprint;
        private bool isCrouchingInput;
        private bool isCrouching;
        private float currentHeight;
        
        public PlayerMovement
            (
                PlayerMovementConfig playerMovementConfig,
                Transform playerTransform,
                [Key("CameraParentTransform")] Transform cameraParentTransform,
                CharacterController characterController,
                ISubscriber<PlayerMoveMessage> playerMoveMessageSubscriber,
                ISubscriber<ChangeSprintStateMessage> changeSprintStateMessageSubscriber,
                ISubscriber<ChangeCrouchingStateMessage> changeCrouchingStateMessageSubscriber
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.playerTransform = playerTransform;
            this.cameraParentTransform = cameraParentTransform;
            this.characterController = characterController;

            characterHeight = characterController.height;
            currentHeight = characterHeight;
            cameraDefaultLocalPosition = cameraParentTransform.localPosition.y;
            
            playerMoveMessageSubscriber.Subscribe(OnMove);
            changeSprintStateMessageSubscriber.Subscribe(OnChangeSprintState);
            changeCrouchingStateMessageSubscriber.Subscribe(OnChangeCrouchingState);
        }

        public Vector3 GetVelocity()
        {
            isCrouching = isCrouchingInput ? isCrouchingInput : !CanStandUp();
            
            if (!characterController.isGrounded)
                return currentVelocity * Time.deltaTime;
            var canSprintForward = !isCrouching && isSprint && direction.y > 0f;
            var currSpeed = isCrouching
                                ? GetSpeed(playerMovementConfig.CrouchSpeed)
                                : canSprintForward
                                    ? playerMovementConfig.SprintSpeed
                                    : GetSpeed(playerMovementConfig.WalkSpeed);
            var currentAccelerationRate = isCrouching 
                                              ? playerMovementConfig.CrouchAccelerationRates
                                              : canSprintForward
                                                  ? playerMovementConfig.SprintAccelerationRates
                                                  : playerMovementConfig.WalkAccelerationRates;
            var input = Vector3.ClampMagnitude(playerTransform.forward * direction.y + playerTransform.right * direction.x, 1f);
            var targetVelocity = input * currSpeed;
            var accel = input.sqrMagnitude > 0.001f ? currentAccelerationRate.x : currentAccelerationRate.y;
            
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accel * Time.deltaTime);
            UpdateCrouch();
            return currentVelocity * Time.deltaTime;
        }

        private bool CanStandUp()
        {
            var position = playerTransform.position;
            var radius = characterController.radius;
            
            var bottom = position + (Vector3.up * (radius + characterController.skinWidth));
            var top = position + (Vector3.up * (characterHeight - radius));

            return !Physics.CheckCapsule(bottom, top, radius, playerMovementConfig.CrouchCheckMask, QueryTriggerInteraction.Ignore);
        }
        
        private float GetSpeed(Vector3 speedVariants)
        {
            var forwardBackSpeed = direction.y > 0f ? speedVariants.x : speedVariants.z;
            var sideSpeed = speedVariants.y;
            var forwardWeight = Mathf.Abs(direction.y);
            var sideWeight = Mathf.Abs(direction.x);
            var totalWeight = forwardWeight + sideWeight;
            var currSpeed =
                totalWeight > 0f
                    ? (forwardBackSpeed * forwardWeight + sideSpeed * sideWeight) / totalWeight
                    : 0f;
            return currSpeed;
        }
        
        private void UpdateCrouch()
        {
            var localPos = cameraParentTransform.localPosition;
            var targetHeight = isCrouching ? playerMovementConfig.CrouchingHeight : characterHeight;
            var targetCameraY = isCrouching ? playerMovementConfig.CameraPositionInCrouching : cameraDefaultLocalPosition;
            
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, playerMovementConfig.CrouchChangedSpeed * Time.deltaTime );
            characterController.height = currentHeight;
            characterController.center = new Vector3(0, currentHeight / 2f, 0);
            cameraParentTransform.localPosition =
                localPos.WithY(Mathf.MoveTowards(cameraParentTransform.localPosition.y,
                                                 targetCameraY,
                                                 playerMovementConfig.CrouchChangedSpeed * Time.deltaTime));
        }
        
        private void OnMove(PlayerMoveMessage msg)
        {
            direction = msg.Direction;
        }

        private void OnChangeSprintState(ChangeSprintStateMessage msg)
        {
            isSprint = msg.State;
        }
        
        private void OnChangeCrouchingState(ChangeCrouchingStateMessage msg)
        {
            isCrouchingInput = msg.State;
        }
    }
}