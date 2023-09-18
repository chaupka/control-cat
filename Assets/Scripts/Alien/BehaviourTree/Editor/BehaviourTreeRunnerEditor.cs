using UnityEngine;
using UnityEditor;
using TheKiwiCoder;

[CustomEditor(typeof(BehaviourTreeRunner))]
public class BehaviourTreeRunnerEditor : Editor
{
    BehaviourTreeRunner treeRunner;
    CheckIfPlayerIsInFov checkIfPlayerIsInFov;
    Transform headTransform;

    private void OnSceneGUI()
    {
        treeRunner = (BehaviourTreeRunner)target;
        headTransform = Context.GetHead(treeRunner.transform);
        checkIfPlayerIsInFov = (CheckIfPlayerIsInFov)treeRunner.tree.nodes.Find(node => node.name.StartsWith(nameof(CheckIfPlayerIsInFov)));
        DrawFov();
    }

    private void DrawFov()
    {
        Handles.color = Color.white;
        Handles.DrawWireArc(headTransform.position, Vector3.forward, Vector3.right, 360, checkIfPlayerIsInFov.viewRadius);
        Vector3 viewAngleA = DirFromAngle(-checkIfPlayerIsInFov.viewAngle / 2);
        Vector3 viewAngleB = DirFromAngle(checkIfPlayerIsInFov.viewAngle / 2);
        Handles.DrawLine(headTransform.position, headTransform.position + viewAngleA * checkIfPlayerIsInFov.viewRadius);
        Handles.DrawLine(headTransform.position, headTransform.position + viewAngleB * checkIfPlayerIsInFov.viewRadius);
    }

    public Vector2 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees -= headTransform.rotation.eulerAngles.z;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
