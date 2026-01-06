using UnityEngine;

public sealed class PlayerCameraAimSync : MonoBehaviour
{
    [field: SerializeField] public Transform RootSource { get; set; } = null!;
    [field: SerializeField] public Transform AimSource { get; set; } = null!;

    [field: SerializeField] public Transform RootTarget { get; set; } = null!;
    [field: SerializeField] public Transform AimTarget { get; set; } = null!;

    private void FixedUpdate()
    {
        if (RootSource && RootTarget)
        {
            RootTarget.position = RootSource.position;
            RootTarget.rotation = RootSource.rotation;
        }

        if (AimSource && AimTarget)
        {
            AimTarget.position = AimSource.position;
            AimTarget.rotation = AimSource.rotation;
        }
    }
}