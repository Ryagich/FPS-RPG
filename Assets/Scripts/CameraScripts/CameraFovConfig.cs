using UnityEngine;

namespace CameraScripts
{
    [CreateAssetMenu(fileName = "CameraFovConfig", menuName = "configs/Camera/CameraFovConfig")]
    public class CameraFovConfig : ScriptableObject
    {
        [field: SerializeField] public float DefaultFov { get; private set; } = 60.0f;
        [field: SerializeField] public float AimFov { get; private set; } = 50.0f;
        [field: SerializeField] public float RunFov { get; private set; } = 55.0f;
        [field: SerializeField] public float ChangeToRunSpeed { get; private set; } = 5.0f;
        [field: SerializeField] public float ChangeToAimSpeed { get; private set; } = 5.0f;
    }
}