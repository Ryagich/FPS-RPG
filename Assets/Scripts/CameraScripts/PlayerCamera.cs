using MessagePipe;
using Messages;
using Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CameraScripts
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerCamera : ITickable
    {
        private readonly PlayerMovementConfig playerMovementConfig;
        private readonly Transform playerBody;
        public readonly Transform cameraParentTransform;

        private float XRotation { get; set; }
        private float yawRotation;
        private Vector2 lookDelta;

        /// <summary>Накопленные yaw/pitch для процедурных слоёв full-body анимации.</summary>
        public Vector2 LookAngles => new(yawRotation, XRotation);
        public Vector2 LastAppliedLookDelta { get; private set; }
        
        public PlayerCamera
            (
                PlayerMovementConfig playerMovementConfig,
                Transform playerBody, 
                [Key("CameraParentTransform")] Transform cameraParentTransform,
                ISubscriber<LookDeltaMessage> lookDeltaMessageSubscriber
            )
        {
            this.playerMovementConfig = playerMovementConfig;
            this.playerBody = playerBody;
            this.cameraParentTransform = cameraParentTransform;

            lookDeltaMessageSubscriber.Subscribe(OnLookDeltaChanged);
        }

        public void Tick()
        {
            if (lookDelta == Vector2.zero)
            {
                LastAppliedLookDelta = Vector2.zero;
                return;
            }

            Rotate();
            lookDelta = Vector2.zero;
        }

        private void OnLookDeltaChanged(LookDeltaMessage msg)
        {
            lookDelta = msg.Delta;
        }
        
        public void Rotate()
        {
            var direction = lookDelta * playerMovementConfig.Sensitivity;
            LastAppliedLookDelta = direction;
            XRotation -= direction.y;
            XRotation = Mathf.Clamp(XRotation, playerMovementConfig.cameraYClamp.x,playerMovementConfig.cameraYClamp.y);
            
            cameraParentTransform.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * direction.x);
            yawRotation += direction.x;
        }
        
        public void RotateX(Vector2 delta)
        {
            LastAppliedLookDelta = delta;
            XRotation -= delta.y;
            XRotation = Mathf.Clamp(XRotation, -90f, 90f);
            
            cameraParentTransform.localRotation = Quaternion.Euler(XRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * delta.x);
            yawRotation += delta.x;
        }
    }
}
