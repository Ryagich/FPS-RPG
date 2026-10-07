using VContainer;
using UnityEngine;
using VContainer.Unity;

public sealed class PlayerCameraAimSync : IFixedTickable
{
    private readonly Transform rootSource;
    private readonly Transform aimSource;
    private readonly Transform rootTarget;
    private readonly Transform aimTarget;

    public PlayerCameraAimSync([Key("RootSource")] Transform rootSource,
        [Key("AimSource")] Transform aimSource, [Key("RootTarget")] Transform rootTarget,
        [Key("AimTarget")] Transform aimTarget)
    {
        this.rootSource = rootSource;
        this.aimSource = aimSource;
        this.rootTarget = rootTarget;
        this.aimTarget = aimTarget;
    }

    public void FixedTick()
    {
        rootTarget.SetPositionAndRotation(rootSource.position, rootSource.rotation);
        aimTarget.SetPositionAndRotation(aimSource.position, aimSource.rotation);
    }
}