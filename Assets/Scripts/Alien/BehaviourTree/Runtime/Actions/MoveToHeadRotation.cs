using TheKiwiCoder;
using UnityEngine;

public class MoveToHeadRotation : ActionNode
{
    public float tolerance = 0.01f;
    Vector2 lastKnownDirection;
    public float rotationSmoothing = 0.1f;
    public float maxDistHead = 2.0f;

    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override State OnUpdate()
    {
        if (context.agent.pathPending)
        {
            return State.Running;
        }

        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            return State.Failure;
        }

        lastKnownDirection = context.agent.steeringTarget - context.headTransform.position;
        var lookRotation = Quaternion.LookRotation(Vector3.forward, lastKnownDirection);
        if (!blackboard.isChasing)
        {
            lookRotation *= Quaternion.Euler(Vector3.forward * blackboard.moveToHeadAngle);
        }
        context.headTransform.rotation = Quaternion.Lerp(
            context.headTransform.rotation,
            lookRotation,
            rotationSmoothing * Time.deltaTime
        );
        context.headTransform.localPosition =
            context.headTransform.rotation
            * Vector3.up
            * Mathf.Min(
                (
                    Mathf.Cos(
                        context.agent.velocity.magnitude * Mathf.PI / context.agent.speed + Mathf.PI
                    ) + 1
                )
                    * 0.5f
                    * maxDistHead,
                maxDistHead
            );
        if (
            Mathf.Abs(lookRotation.eulerAngles.z - context.headTransform.rotation.eulerAngles.z)
            < tolerance
        )
        {
            return State.Success;
        }

        return State.Running;
    }
}
