using UnityEngine;
using TheKiwiCoder;
using Utility;

public class CheckIfPlayerIsInFov : DecoratorNode
{
    public float viewRadius = 5f;

    [Range(0, 360)]
    public float viewAngle;

    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override State OnUpdate()
    {
        var collider = Physics2D.OverlapCircle(
            context.transform.position,
            viewRadius,
            LayerTag.playerLayer
        );
        if (collider != null && GameStateController.singleton.aiDirector.player)
        {
            var playerPosition = GameStateController.singleton.aiDirector.player.transform.position;
            var dirToPlayer = (playerPosition - context.head.transform.position).normalized;
            if (Vector2.Angle(context.headTransform.up, dirToPlayer) < (viewAngle / 2))
            {
                var distToPlayer = Vector2.Distance(
                    playerPosition,
                    context.head.transform.position
                );
                if (
                    !Physics2D.Raycast(
                        context.head.transform.position,
                        dirToPlayer,
                        distToPlayer,
                        LayerTag.terrainLayer
                    )
                )
                {
                    blackboard.moveToPosition = playerPosition;
                    blackboard.isAgitated = true;
                    blackboard.isChasing = true;
                    context.bodyRenderer.color = Color.red;

                    // TODO _animator.SetBool("Walking", true);
                    return child.Update();
                }
            }
        }
        blackboard.isChasing = false;
        Abort();
        return State.Failure;
    }
}
