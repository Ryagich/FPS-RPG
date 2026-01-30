 using UnityEngine;

public class HandsTargets : MonoBehaviour
{
    [field: SerializeField] private Transform leftTarget = null!;
    [field: SerializeField] private Transform rightTarget = null!;

    [field: SerializeField] private Transform currentLeftTarget = null!;
    [field: SerializeField] private Transform currentRightTarget = null!;

    private void Update()
    {
        if (currentLeftTarget)
        {
            leftTarget.position = currentLeftTarget.position;
            leftTarget.rotation = currentLeftTarget.rotation;
        }
        if (currentRightTarget)
        {
            rightTarget.position = currentRightTarget.position;
            rightTarget.rotation = currentRightTarget.rotation;
        }
    }

    public void SetTarget(Transform newLeftTarget, Transform newRightTarget)
    {
        currentLeftTarget = newLeftTarget;
        currentRightTarget = newRightTarget;
    }
}