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
        private readonly InputProvider inputProvider;
        private readonly MoveStates moveStates;

        private readonly float characterHeight;
        private readonly float cameraDefaultLocalPosition;
        private Vector3 currentVelocity;
        
        private float currentHeight;

        public PlayerMovement
            (
                PlayerMovementConfig playerMovementConfig,
                Transform playerTransform,
                [Key("CameraParentTransform")] Transform cameraParentTransform,
                CharacterController characterController,
                InputProvider inputProvider,
                MoveStates moveStates
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.playerTransform = playerTransform;
            this.cameraParentTransform = cameraParentTransform;
            this.characterController = characterController;
            this.inputProvider = inputProvider;
            this.moveStates = moveStates;

            characterHeight = characterController.height;
            currentHeight = characterHeight;
            cameraDefaultLocalPosition = cameraParentTransform.localPosition.y;
        }
        
        public Vector3 GetVelocity()
        {
            moveStates.IsCrouching.Value = moveStates.IsCrouchingInput ? moveStates.IsCrouchingInput : !CanStandUp();
            moveStates.IsSprinting.Value = false;

            if (!characterController.isGrounded)
                return currentVelocity * Time.deltaTime;
            moveStates.Direction = Vector3.ClampMagnitude(playerTransform.forward * moveStates.MoveInput.y 
                                                        + playerTransform.right * moveStates.MoveInput.x, 1f);
            moveStates.IsSprinting.Value = !moveStates.IsCrouchingInput && moveStates.IsSprintingInput && inputProvider.CanRun();

            var canSprintForward = !moveStates.IsCrouching.Value && moveStates.IsSprinting.Value && moveStates.MoveInput.y > 0f;
            var currSpeed = moveStates.IsCrouching.Value
                                ? GetSpeed(playerMovementConfig.CrouchSpeed)
                                : canSprintForward
                                    ? playerMovementConfig.SprintSpeed
                                    : GetSpeed(playerMovementConfig.WalkSpeed);
            var currentAccelerationRate = moveStates.IsCrouching.Value
                                              ? playerMovementConfig.CrouchAccelerationRates
                                              : canSprintForward
                                                  ? playerMovementConfig.SprintAccelerationRates
                                                  : playerMovementConfig.WalkAccelerationRates;
            var targetVelocity = moveStates.Direction * currSpeed;
            var accel = moveStates.Direction.sqrMagnitude > 0.001f ? currentAccelerationRate.x : currentAccelerationRate.y;
            
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
            var forwardBackSpeed = moveStates.MoveInput.y > 0f ? speedVariants.x : speedVariants.z;
            var sideSpeed = speedVariants.y;
            var forwardWeight = Mathf.Abs(moveStates.MoveInput.y);
            var sideWeight = Mathf.Abs(moveStates.MoveInput.x);
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
            var targetHeight = moveStates.IsCrouching.Value ? playerMovementConfig.CrouchingHeight : characterHeight;
            var targetCameraY = moveStates.IsCrouching.Value ? playerMovementConfig.CameraPositionInCrouching : cameraDefaultLocalPosition;
            
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, playerMovementConfig.CrouchChangedSpeed * Time.deltaTime );
            characterController.height = currentHeight;
            characterController.center = new Vector3(0, currentHeight / 2f, 0);
            cameraParentTransform.localPosition =
                localPos.WithY(Mathf.MoveTowards(cameraParentTransform.localPosition.y,
                                                 targetCameraY,
                                                 playerMovementConfig.CrouchChangedSpeed * Time.deltaTime));
        }
    }
}