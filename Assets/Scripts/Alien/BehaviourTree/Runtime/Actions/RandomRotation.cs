using UnityEngine;
using TheKiwiCoder;

public class RandomRotation : ActionNode
{
    [Range(0, 360)]
    public float minHeadRotation = 50;

    [Range(0, 360)]
    public float maxHeadRotation = 50;

    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override State OnUpdate()
    {
        blackboard.moveToHeadAngle =
            Random.Range(minHeadRotation, maxHeadRotation) * (Random.Range(0, 2) * 2 - 1);
        return State.Success;
    }
}
