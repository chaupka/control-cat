using TheKiwiCoder;

public class ChasePlayer : ActionNode
{
    public float speed = 5;
    public float stoppingDistance = 0.1f;
    public float acceleration = 40.0f;
    public float tolerance = 1.0f;

    protected override void OnStart()
    {
        context.agent.stoppingDistance = stoppingDistance;
        context.agent.speed = speed;
        context.agent.acceleration = acceleration;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        context.agent.destination = blackboard.moveToPosition;

        if (context.agent.pathPending)
        {
            return State.Running;
        }

        if (blackboard.isChasing && context.agent.remainingDistance < tolerance)
        {
            return State.Success;
        }

        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            return State.Failure;
        }

        return State.Running;
    }
}
