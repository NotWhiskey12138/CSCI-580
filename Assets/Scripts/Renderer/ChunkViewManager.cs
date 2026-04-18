using System.Collections.Generic;
using UnityEngine;

public class ChunkViewManager : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private Material chunkMaterial;

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

            var go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
            go.transform.SetParent(transform, false);

            // RequireComponent 会连带加上 MeshFilter + MeshRenderer
            var cr = go.AddComponent<ChunkRenderer>();

            var mr = go.GetComponent<MeshRenderer>();
            if (chunkMaterial != null)
            {
                mr.sharedMaterial = chunkMaterial;
            }

            cr.Build(world, coord, world.ChunkSize);

            renderers[coord] = cr;
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