using Player;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CameraScripts
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CameraFovController : ITickable
    {
        private readonly CameraFovConfig cameraFovConfig;
        private readonly Camera сamera;

        private float defaultFov;
        private float targetFov;
        private float speed;
        
        public CameraFovController
            (
                CameraFovConfig cameraFovConfig,
                MoveStates moveStates,
                [Key("CameraParentTransform")] Transform cameraParentTransform
            )
        {
            this.cameraFovConfig = cameraFovConfig;
            сamera = cameraParentTransform.GetComponentInChildren<Camera>();

            moveStates.IsSprinting.Subscribe(ChangeRunFov);

            defaultFov = сamera.fieldOfView;
            
            targetFov = cameraFovConfig.RunFov;
            speed = cameraFovConfig.ChangeToRunSpeed;
        }
        
        public void ChangeRunFov(bool value)
        {
            if (value)
            {
                targetFov = cameraFovConfig.RunFov;
                speed = cameraFovConfig.ChangeToRunSpeed;
            }
            else
            {
                targetFov = defaultFov;
            }
        }

        public void Tick()
        {
            if (Mathf.Abs(сamera.fieldOfView - targetFov) > .01f)
            {
                сamera.fieldOfView = Mathf.Lerp(
                                                сamera.fieldOfView,
                                                targetFov,
                                                Time.deltaTime * speed
                                               );
            }
            else
            {
                сamera.fieldOfView = targetFov;
            }
        }
    }
}