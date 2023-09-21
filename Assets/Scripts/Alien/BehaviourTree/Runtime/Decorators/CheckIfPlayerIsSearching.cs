using UnityEngine;
using TheKiwiCoder;

public class CheckIfPlayerIsSearching : DecoratorNode
{
    [Range(0, 360)]
    public float maxAgitateTime = 5.0f;

    protected override void OnStart()
    {
        blackboard.agitateTimer = maxAgitateTime;
        blackboard.searchPosition = blackboard.moveToPosition;
    }

    protected override void OnStop() { }

    protected override State OnUpdate()
    {
        if (blackboard.isAgitated && !blackboard.isChasing)
        {
            blackboard.agitateTimer -= Time.deltaTime;
            if (blackboard.agitateTimer < 0)
            {
                blackboard.isAgitated = false;
            }
            return child.Update();
        }
        context.bodyRenderer.color = Color.black;
        Abort();
        return State.Failure;
    }
}
