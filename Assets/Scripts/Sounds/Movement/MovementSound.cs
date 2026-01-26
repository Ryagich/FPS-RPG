using Camera.Shake;
using MessagePipe;
using Messages;
using Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Sounds.Movement
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MovementSound : ITickable
    {
        private readonly MovementSoundConfig distanceToPlayMovementSoundConfig;
        private readonly SoundConfig soundConfig;
        private readonly CameraShakeOnStep cameraShakeOnStep;

        private readonly Transform transform;
        private readonly PlayerMovement playerMovement;
        private readonly CharacterController characterController;

        private float distanceAccumulated;
        private Vector3 lastPosition;

        public MovementSound(
                MovementSoundConfig distanceToPlayMovementSoundConfig,
                [Key("MovementSoundConfig")] SoundConfig movementSoundConfig,
                Transform transform,
                PlayerMovement playerMovement,
                CharacterController characterController,
                CameraShakeOnStep cameraShakeOnStep
            )
        {
            this.distanceToPlayMovementSoundConfig = distanceToPlayMovementSoundConfig;
            soundConfig = movementSoundConfig;

            this.transform = transform;
            this.playerMovement = playerMovement;
            this.characterController = characterController;
            this.cameraShakeOnStep = cameraShakeOnStep;

            lastPosition = transform.position;
        }

        public void Tick()
        {
            if (playerMovement == null || !characterController.isGrounded)
                return;

            var currentPosition = transform.position;
            var lastFlatPosition = lastPosition;

            currentPosition.y = 0;
            lastFlatPosition.y = 0;

            var distanceMoved = Vector3.Distance(currentPosition, lastFlatPosition);

            if (distanceMoved > 0.001f)
            {
                distanceAccumulated += distanceMoved;
                var stepDistance = GetStepDistance();

                if (distanceAccumulated >= stepDistance)
                {
                    TryPlayStepSound();
                    cameraShakeOnStep.OnStep(); // ← ВАЖНО
                    distanceAccumulated = 0f;
                }
            }

            lastPosition = transform.position;
        }

        private void TryPlayStepSound()
        {
            var pos = transform.position;
            var rayOrigin = pos + Vector3.up * 0.1f;
            var rayDistance = characterController.skinWidth + 0.3f;

            if (!Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayDistance))
                return;

            var hitLayer = hit.collider.gameObject.layer;

            if ((distanceToPlayMovementSoundConfig.GroundMask.value & (1 << hitLayer)) != 0)
            {
                GlobalMessagePipe.GetPublisher<PlaySoundMessage>()
                                 .Publish(new PlaySoundMessage(
                                                               distanceToPlayMovementSoundConfig.StepOnGroundSoundConfig.SoundSettings, pos, null));
            }
            else if ((distanceToPlayMovementSoundConfig.WoodMask.value & (1 << hitLayer)) != 0)
            {
                GlobalMessagePipe.GetPublisher<PlaySoundMessage>()
                                 .Publish(new PlaySoundMessage(
                                                               distanceToPlayMovementSoundConfig.StepOnWoodSoundConfig.SoundSettings, pos, null));
            }
            else
            {
                GlobalMessagePipe.GetPublisher<PlaySoundMessage>()
                                 .Publish(new PlaySoundMessage(
                                                               distanceToPlayMovementSoundConfig.StepOnRockSoundConfig.SoundSettings, pos, null));
            }

            GlobalMessagePipe.GetPublisher<PlaySoundMessage>()
                             .Publish(new PlaySoundMessage(soundConfig.SoundSettings, pos, null));
        }

        private float GetStepDistance()
        {
            if (playerMovement.IsSprinting.Value)
                return distanceToPlayMovementSoundConfig.SprintStepDistance;
            if (playerMovement.IsCrouching.Value)
                return distanceToPlayMovementSoundConfig.CrouchStepDistance;
            return distanceToPlayMovementSoundConfig.WalkStepDistance;
        }
    }
}
