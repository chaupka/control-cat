using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class RandomSearchPositionInNavMesh : ActionNode
{
    [Range(1, 100)]
    public float maxDistanceToPosition = 3;

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        var randomPosition = blackboard.searchPosition + new Vector2(Random.Range(0, 2), Random.Range(0, 2)).normalized * Random.Range(1, maxDistanceToPosition);
        // TODO check if randomposition is walkable
        NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, maxDistanceToPosition, context.navMeshSurface.layerMask);
        blackboard.moveToPosition = hit.position;
        return State.Success;
    }
}
