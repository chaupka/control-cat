using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class RandomPositionInNavMesh : ActionNode
{
    public Bounds bounds;

    protected override void OnStart()
    {
        bounds = context.navMeshSurface.navMeshData.sourceBounds;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        var randomPosition = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.z, bounds.max.z));
        // TODO check if randomposition is walkable
        NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, bounds.extents.magnitude, context.navMeshSurface.layerMask);
        blackboard.moveToPosition = hit.position;
        return State.Success;
    }
}
