using Dependencies;
using UnityEngine;
using VContainer.Unity;

public sealed class PlayerCameraAimSync : IFixedTickable
{
    private readonly Transform rootSource;
    private readonly Transform aimSource;
    private readonly Transform rootTarget;
    private readonly Transform aimTarget;

    public PlayerCameraAimSync(SyncRootSource rootSourceReference,
        SyncAimSource aimSourceReference, SyncRootTarget rootTargetReference,
        SyncAimTarget aimTargetReference)
    {
        var rootSource = rootSourceReference.Value;
        var aimSource = aimSourceReference.Value;
        var rootTarget = rootTargetReference.Value;
        var aimTarget = aimTargetReference.Value;
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