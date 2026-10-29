using UnityEngine;
using VContainer;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerMovement
    {
        private readonly PlayerMovementConfig playerMovementConfig;
        private readonly Transform playerTransform;
        private readonly PlayerGrounding grounding;
        private readonly InputProvider inputProvider;
        private readonly MoveStates moveStates;

        private Vector3 currentVelocity;

        public bool IsMoving => currentVelocity.sqrMagnitude > .001f;

        public PlayerMovement
            (
                PlayerMovementConfig playerMovementConfig,
                Transform playerTransform,
                PlayerGrounding grounding,
                InputProvider inputProvider,
                MoveStates moveStates
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.playerTransform = playerTransform;
            this.grounding = grounding;
            this.inputProvider = inputProvider;
            this.moveStates = moveStates;
        }
        
        public Vector3 GetVelocity()
        {
            if (grounding.IsFalling)
            {
                moveStates.IsSprinting.Value = false;
                return currentVelocity * Time.deltaTime;
            }

            moveStates.DesiredDirection = Vector3.ClampMagnitude(playerTransform.forward * moveStates.MoveInput.y
                                                               + playerTransform.right * moveStates.MoveInput.x, 1f);

            var canSprintForward = !moveStates.IsCrouching.Value
                                   && !moveStates.IsProne.Value
                                   && !moveStates.IsCrouchingInput
                                   && moveStates.IsSprintingInput
                                   && moveStates.MoveInput.y > 0f
                                   && Mathf.Approximately(moveStates.MoveInput.x, 0f)
                                   && inputProvider.CanRun();
            moveStates.IsSprinting.Value = canSprintForward;

            var currSpeed = moveStates.IsProne.Value
                                ? GetSpeed(playerMovementConfig.ProneSpeed)
                                : moveStates.IsCrouching.Value
                                ? GetSpeed(playerMovementConfig.CrouchSpeed)
                                : canSprintForward
                                    ? playerMovementConfig.SprintSpeed
                                    : GetSpeed(playerMovementConfig.WalkSpeed);
            var currentAccelerationRate = moveStates.IsProne.Value || moveStates.IsCrouching.Value
                                              ? playerMovementConfig.CrouchAccelerationRates
                                              : canSprintForward
                                                  ? playerMovementConfig.SprintAccelerationRates
                                                  : playerMovementConfig.WalkAccelerationRates;
            var targetVelocity = moveStates.DesiredDirection * currSpeed;
            var accel = moveStates.DesiredDirection.sqrMagnitude > 0.001f ? currentAccelerationRate.x : currentAccelerationRate.y;
            
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, accel * Time.deltaTime);
            moveStates.Direction = currSpeed > .001f
                                     ? Vector3.ClampMagnitude(currentVelocity / currSpeed, 1f)
                                     : Vector3.zero;
            return currentVelocity * Time.deltaTime;
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
        
    }
}
