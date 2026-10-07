using VContainer;
using UnityEngine;
using VContainer.Unity;

public sealed class HandsTargets : ITickable
{
    private readonly Transform leftTarget;
    private readonly Transform rightTarget;
    private Transform currentLeftTarget;
    private Transform currentRightTarget;

    public HandsTargets([Key("LeftHandTarget")] Transform leftTarget,
        [Key("RightHandTarget")] Transform rightTarget)
    {
        this.leftTarget = leftTarget;
        this.rightTarget = rightTarget;
    }

    public void Tick()
    {
        if (currentLeftTarget != null)
            leftTarget.SetPositionAndRotation(currentLeftTarget.position, currentLeftTarget.rotation);
        if (currentRightTarget != null)
            rightTarget.SetPositionAndRotation(currentRightTarget.position, currentRightTarget.rotation);
    }

    public void SetTarget(Transform left, Transform right)
    {
        currentLeftTarget = left;
        currentRightTarget = right;
    }
}