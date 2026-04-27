using System.Collections.Generic;
using UnityEngine;

public class ChunkViewManager : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private Material chunkMaterial;
    [SerializeField] private Material edgeMaterial;

    [SerializeField] private Transform player;
    [SerializeField] private int viewDistance = 2;     // visible radius (in chunks)
    [SerializeField] private int preloadDistance = 4;  // preload radius (in chunks)
    [SerializeField] private VoxelTextureAtlas atlas;

    private readonly Dictionary<Vector3Int, ChunkRenderer> renderers = new();
    private readonly Queue<ChunkRenderer> pool = new(); // object pool

    private Vector3Int lastPlayerChunk = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    private Material runtimeFallbackMaterial;

    private void Start()
    {
        Rebuild();
    }

    private void Update()
    {
        if (world == null || player == null)
            return;

        // update only when crossing chunk boundary
        Vector3Int currentChunk = WorldToChunkCoord(player.position);
        if (currentChunk != lastPlayerChunk)
        {
            RefreshVisibleChunks();
            lastPlayerChunk = currentChunk;
        }
    }

    [ContextMenu("Rebuild All")]
    public void Rebuild()
    {
        Clear();

        if (world == null || player == null)
            return;

        RefreshVisibleChunks(force: true);
        lastPlayerChunk = WorldToChunkCoord(player.position);
    }

    public void RebuildChunks(List<Vector3Int> coords)
    {
        if (world == null)
            return;

        // rebuild only affected chunks
        foreach (var coord in coords)
        {
            if (!renderers.TryGetValue(coord, out var cr) || cr == null)
                continue;

            cr.Build(world, coord, world.ChunkSize, atlas);

            if (edgeMaterial != null)
            {
                var edgeRenderer = cr.GetComponentInChildren<ChunkEdgeRenderer>(true);
                if (edgeRenderer != null)
                {
                    edgeRenderer.Build(world, coord, world.ChunkSize);
                }
            }
        }
    }

    private void RefreshVisibleChunks(bool force = false)
    {
        if (world == null || player == null)
            return;

        Vector3Int center = WorldToChunkCoord(player.position);

        HashSet<Vector3Int> desiredLoaded = new();
        HashSet<Vector3Int> desiredVisible = new();

        // classify chunks into preload / visible sets
        foreach (var pair in world.LoadedChunks)
        {
            Vector3Int coord = pair.Key;

            if (IsWithinRadius(coord, center, preloadDistance))
                desiredLoaded.Add(coord);

            if (IsWithinRadius(coord, center, viewDistance))
                desiredVisible.Add(coord);
        }

        // unload chunks outside preload range (return to pool)
        List<Vector3Int> toRemove = new();
        foreach (var kv in renderers)
        {
            if (!desiredLoaded.Contains(kv.Key))
                toRemove.Add(kv.Key);
        }

        foreach (var coord in toRemove)
            ReleaseChunkObject(coord);

        // load missing chunks within preload range
        foreach (var coord in desiredLoaded)
        {
            if (!renderers.ContainsKey(coord))
                CreateChunkObject(coord);
        }

        // toggle visibility
        foreach (var kv in renderers)
        {
            bool visible = desiredVisible.Contains(kv.Key);
            kv.Value.gameObject.SetActive(visible);
        }
    }

    private void CreateChunkObject(Vector3Int coord)
    {
        ChunkRenderer cr;

        // reuse from pool if available
        if (pool.Count > 0)
        {
            cr = pool.Dequeue();
            cr.gameObject.SetActive(false);
            cr.transform.SetParent(transform, false);
            cr.gameObject.name = $"Chunk_{coord.x}_{coord.y}_{coord.z}";
        }
        else
        {
            GameObject go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
            go.transform.SetParent(transform, false);
            cr = go.AddComponent<ChunkRenderer>();
        }

        // assign material
        var mr = cr.GetComponent<MeshRenderer>();
        if (mr != null)
            mr.sharedMaterial = GetChunkMaterial();

        // build mesh (pass atlas)
        cr.Build(world, coord, world.ChunkSize, atlas);

        // optional edge overlay
        if (edgeMaterial != null)
        {
            var edgeRenderer = cr.GetComponentInChildren<ChunkEdgeRenderer>(true);
            if (edgeRenderer == null)
            {
                GameObject edgeGo = new GameObject("Edges");
                edgeGo.transform.SetParent(cr.transform, false);

                edgeRenderer = edgeGo.AddComponent<ChunkEdgeRenderer>();
                var edgeMr = edgeGo.GetComponent<MeshRenderer>();
                if (edgeMr != null)
                    edgeMr.sharedMaterial = edgeMaterial;
            }

            edgeRenderer.Build(world, coord, world.ChunkSize);
        }

        renderers[coord] = cr;
    }

    private void ReleaseChunkObject(Vector3Int coord)
    {
        // return chunk to pool instead of destroying
        if (!renderers.TryGetValue(coord, out var cr) || cr == null)
        {
            renderers.Remove(coord);
            return;
        }

        renderers.Remove(coord);

        cr.gameObject.SetActive(false);
        pool.Enqueue(cr);
    }

    private bool IsWithinRadius(Vector3Int coord, Vector3Int center, int radius)
    {
        // AABB distance in chunk space
        return Mathf.Abs(coord.x - center.x) <= radius
            && Mathf.Abs(coord.y - center.y) <= radius
            && Mathf.Abs(coord.z - center.z) <= radius;
    }

    private Vector3Int WorldToChunkCoord(Vector3 worldPos)
    {
        // convert world position to chunk coordinate
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / world.ChunkSize),
            Mathf.FloorToInt(worldPos.y / world.ChunkSize),
            Mathf.FloorToInt(worldPos.z / world.ChunkSize)
        );
    }

    private Material GetChunkMaterial()
    {
        // fallback material if none assigned
        if (chunkMaterial != null)
            return chunkMaterial;

        if (runtimeFallbackMaterial == null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader != null)
                runtimeFallbackMaterial = new Material(shader);
        }

        return runtimeFallbackMaterial;
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        // destroy active chunks
        foreach (var kv in renderers)
        {
            if (kv.Value != null)
            {
                if (Application.isPlaying)
                    Destroy(kv.Value.gameObject);
                else
                    DestroyImmediate(kv.Value.gameObject);
            }
        }

        // clear pool
        while (pool.Count > 0)
        {
            ChunkRenderer cr = pool.Dequeue();
            if (cr == null) continue;

            if (Application.isPlaying)
                Destroy(cr.gameObject);
            else
                DestroyImmediate(cr.gameObject);
        }

        renderers.Clear();
    }

    private void OnDestroy()
    {
        // cleanup runtime material
        if (runtimeFallbackMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(runtimeFallbackMaterial);
            else
                DestroyImmediate(runtimeFallbackMaterial);
        }
    }
}