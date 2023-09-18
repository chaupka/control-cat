using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Tilemaps;

public class NavMeshController : MonoBehaviour
{
    [SerializeField] Tilemap backTilemap;
    [HideInInspector] public bool isDone;

    public IEnumerator FixNavMeshAgent()
    {
        GetComponent<NavMeshSurface>().BuildNavMeshAsync();
        while (!NavMesh.SamplePosition(backTilemap.cellBounds.center, out _, backTilemap.size.magnitude, NavMesh.AllAreas))
        {
            yield return null;
        }
        isDone = true;
    }
}
