using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Tilemaps;

public class NavMeshController : MonoBehaviour
{
    [SerializeField]
    Tilemap backTilemap;
    private NavMeshSurface surface;

    [HideInInspector]
    public bool isDone;

    private void Start()
    {
        surface = GetComponent<NavMeshSurface>();
    }

    public IEnumerator FixNavMeshAgent()
    {
        isDone = false;
        surface.BuildNavMesh();
        while (
            !NavMesh.SamplePosition(
                backTilemap.cellBounds.center,
                out _,
                backTilemap.size.magnitude,
                NavMesh.AllAreas
            )
        )
        {
            yield return null;
        }
        isDone = true;
    }
}
