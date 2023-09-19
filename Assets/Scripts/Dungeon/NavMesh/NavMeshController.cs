using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using NavMeshPlus.Extensions;

public class NavMeshController : MonoBehaviour
{
    [SerializeField]
    Tilemap backTilemap;
    private NavMeshSurface surface;
    private NavMeshData data;
    private List<NavMeshBuildSource> sources = new();
    private CollectSources2d collectSources;
    private Vector3 navMeshSize = new(20, 20, 20);

    [HideInInspector]
    public bool isDone;

    private void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        collectSources = GetComponent<CollectSources2d>();
    }

    public IEnumerator BakeNavMesh()
    {
        isDone = false;
        surface.BuildNavMeshAsync();
        data = surface.navMeshData;

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

    public void Rebake()
    {
        isDone = false;
        // BuildNavMesh(true, platform);
        // surface.UpdateNavMesh(data);
        isDone = true;
    }

    private void BuildNavMesh(bool Async, Vector2 platform)
    {
        Bounds navMeshBounds = new(platform, navMeshSize);
        var markups = new List<NavMeshBuildMarkup>();

        var modifiers = NavMeshModifier.activeModifiers;

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (
                ((surface.layerMask & (1 << modifiers[i].gameObject.layer)) == 1)
                && modifiers[i].AffectsAgentType(surface.agentTypeID)
            )
            {
                markups.Add(
                    new NavMeshBuildMarkup()
                    {
                        root = modifiers[i].transform,
                        overrideArea = modifiers[i].overrideArea,
                        area = modifiers[i].area,
                        ignoreFromBuild = modifiers[i].ignoreFromBuild
                    }
                );
            }
        }

        NavMeshBuilder.CollectSources(
            navMeshBounds,
            surface.layerMask,
            surface.useGeometry,
            surface.defaultArea,
            markups,
            sources
        );

        sources.RemoveAll(
            source =>
                source.component != null
                && source.component.gameObject.GetComponent<NavMeshAgent>() != null
        );

        if (Async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync(
                data,
                surface.GetBuildSettings(),
                sources,
                new Bounds(platform, navMeshSize)
            );
        }
        else
        {
            NavMeshBuilder.UpdateNavMeshData(
                data,
                surface.GetBuildSettings(),
                sources,
                new Bounds(platform, navMeshSize)
            );
        }
    }
}
