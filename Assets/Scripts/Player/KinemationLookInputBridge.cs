using CameraScripts;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Input;
using UnityEngine;
using VContainer.Unity;

namespace Player
{
    using KINEMATION.FPSAnimationFramework.Runtime.Layers.IkMotionLayer;
    using MessagePipe;
    using Messages;

    /// <summary>
    /// Передаёт единственный источник поворота камеры в LookLayer Kinemation.
    /// Именно LookLayer распределяет pitch между грудью, шеей и головой full-body модели.
    /// </summary>
    public sealed class KinemationLookInputBridge : ITickable
    {
        private readonly PlayerCamera playerCamera;
        private readonly FPSAnimator fpsAnimator;
        private readonly UserInputController input;
        private readonly IkMotionLayerSettings leanMotion;
        private readonly float leanAngle;
        private float leanDirection;
        private bool isLeanMotionPending;

        public KinemationLookInputBridge(
            PlayerCamera playerCamera,
            PlayerAvatarView avatarView,
            PlayerMovementConfig movementConfig,
            ISubscriber<ChangeLeanStateMessage> leanStateSubscriber)
        {
            this.playerCamera = playerCamera;
            fpsAnimator = avatarView.Animator.GetComponent<FPSAnimator>();
            input = avatarView.Animator.GetComponent<UserInputController>();
            leanMotion = movementConfig.LeanMotion;
            leanAngle = movementConfig.LeanAngle;

            if (fpsAnimator == null || input == null)
                throw new MissingComponentException(
                    "Full-body player requires FPSAnimator and UserInputController for camera-driven body pose.");

            leanStateSubscriber.Subscribe(OnLeanStateChanged);
        }

        public void Tick()
        {
            // FPSAnimator initializes UserInputController in Start(). Until a weapon profile
            // is linked, the LookLayer is absent and writing input would be meaningless.
            if (!fpsAnimator.HasLinkedProfile)
                return;

            if (isLeanMotionPending && leanMotion != null)
            {
                fpsAnimator.LinkAnimatorLayer(leanMotion);
                isLeanMotionPending = false;
            }

            var look = playerCamera.LookAngles;
            input.SetValue(FPSANames.MouseInput, new Vector4(look.x, look.y, 0f, 0f));
            var lookDelta = playerCamera.LastAppliedLookDelta;
            input.SetValue(FPSANames.MouseDeltaInput, new Vector4(lookDelta.x, -lookDelta.y, 0f, 0f));
            input.SetValue(FPSANames.LeanInput, leanDirection * leanAngle);
        }

        private void OnLeanStateChanged(ChangeLeanStateMessage message)
        {
            leanDirection = message.Direction;
            isLeanMotionPending = leanMotion != null;
        }
    }
}
