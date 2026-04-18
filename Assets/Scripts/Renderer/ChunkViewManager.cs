using System.Collections.Generic;
using UnityEngine;

public class ChunkViewManager : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private Material chunkMaterial;
    [SerializeField] private Material edgeMaterial;

    private readonly Dictionary<Vector3Int, ChunkRenderer> renderers = new();

    [ContextMenu("Rebuild All")]
    public void Rebuild()
    {
        Clear();

        if (world == null)
        {
            Debug.LogWarning("ChunkViewManager: World 未赋值");
            return;
        }

        foreach (var pair in world.LoadedChunks)
        {
            Vector3Int coord = pair.Key;

            // 1. 实心 mesh（solid）
            var go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
            go.transform.SetParent(transform, false);

            var cr = go.AddComponent<ChunkRenderer>();
            var mr = go.GetComponent<MeshRenderer>();
            if (chunkMaterial != null) mr.sharedMaterial = chunkMaterial;
            cr.Build(world, coord, world.ChunkSize);

            // 2. 线框 mesh（edges）作为子物体，local=0
            if (edgeMaterial != null)
            {
                var edgeGo = new GameObject("Edges");
                edgeGo.transform.SetParent(go.transform, false); // 继承 parent 的 origin 偏移
                var er = edgeGo.AddComponent<ChunkEdgeRenderer>();
                edgeGo.GetComponent<MeshRenderer>().sharedMaterial = edgeMaterial;
                er.Build(world, coord, world.ChunkSize);
            }

            renderers[coord] = cr;
        }
    }

    public void RebuildChunks(List<Vector3Int> coords)
    {
        foreach (var coord in coords)
        {
            if (!renderers.TryGetValue(coord, out var cr))
            {
                var go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
                go.transform.SetParent(transform, false);

                cr = go.AddComponent<ChunkRenderer>();
                var mr = go.GetComponent<MeshRenderer>();
                if (chunkMaterial != null) mr.sharedMaterial = chunkMaterial;

                if (edgeMaterial != null)
                {
                    var edgeGo = new GameObject("Edges");
                    edgeGo.transform.SetParent(go.transform, false);
                    var er = edgeGo.AddComponent<ChunkEdgeRenderer>();
                    edgeGo.GetComponent<MeshRenderer>().sharedMaterial = edgeMaterial;
                    er.Build(world, coord, world.ChunkSize);
                }

                renderers[coord] = cr;
            }

            cr.Build(world, coord, world.ChunkSize);

            if (edgeMaterial != null)
            {
                var edgeRenderer = cr.GetComponentInChildren<ChunkEdgeRenderer>();
                if (edgeRenderer == null)
                {
                    var edgeGo = new GameObject("Edges");
                    edgeGo.transform.SetParent(cr.transform, false);
                    edgeRenderer = edgeGo.AddComponent<ChunkEdgeRenderer>();
                    edgeGo.GetComponent<MeshRenderer>().sharedMaterial = edgeMaterial;
                }

                edgeRenderer.Build(world, coord, world.ChunkSize);
            }
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
        renderers.Clear();
    }
}
