using System.Collections.Generic;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 8;
    [SerializeField] private int chunksX = 2;
    [SerializeField] private int chunksY = 1;
    [SerializeField] private int chunksZ = 2;

    [Header("Terrain Settings")]
    [SerializeField] private float noiseScale = 0.15f;
    [SerializeField] private float heightMultiplier = 5f;
    [SerializeField] private int baseHeight = 2;
    [SerializeField] private int seed = 0;

    private readonly Dictionary<Vector3Int, ChunkData> chunks = new();

    public int ChunkSize => chunkSize;
    public IEnumerable<KeyValuePair<Vector3Int, ChunkData>> LoadedChunks => chunks;

    private void OnValidate()
    {
        chunkSize = Mathf.Max(1, chunkSize);
        chunksX = Mathf.Max(1, chunksX);
        chunksY = Mathf.Max(1, chunksY);
        chunksZ = Mathf.Max(1, chunksZ);

        noiseScale = Mathf.Max(0.001f, noiseScale);
        heightMultiplier = Mathf.Max(1f, heightMultiplier);
        baseHeight = Mathf.Max(1, baseHeight);
    }

    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        chunks.Clear();


        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                for (int cz = 0; cz < chunksZ; cz++)
                {
                    Vector3Int chunkCoord = new Vector3Int(cx, cy, cz);
                    chunks[chunkCoord] = new ChunkData(chunkCoord, chunkSize);
                }
            }
        }


        int worldMaxHeight = chunksY * chunkSize;

        for (int x = 0; x < chunksX * chunkSize; x++)
        {
            for (int z = 0; z < chunksZ * chunkSize; z++)
            {
                float sampleX = (x + seed) * noiseScale;
                float sampleZ = (z + seed) * noiseScale;

                int columnHeight = Mathf.Clamp(
                    Mathf.FloorToInt(Mathf.PerlinNoise(sampleX, sampleZ) * heightMultiplier) + baseHeight,
                    1,
                    worldMaxHeight
                );

                for (int y = 0; y < columnHeight; y++)
                {
                    VoxelType type;

                    if (y == columnHeight - 1)
                    {
                        type = VoxelType.Grass;
                    }
                    else if (y >= columnHeight - 3)
                    {
                        type = VoxelType.Dirt;
                    }
                    else
                    {
                        type = VoxelType.Stone;
                    }

                    SetVoxel(x, y, z, type);
                }
            }
        }
    }

    public VoxelType GetVoxel(int worldX, int worldY, int worldZ)
    {
        if (!TryGetChunkAndLocal(worldX, worldY, worldZ, out ChunkData chunk, out Vector3Int localPos))
        {
            return VoxelType.Air;
        }

        return chunk.GetLocalVoxel(localPos.x, localPos.y, localPos.z);
    }

    public void SetVoxel(int worldX, int worldY, int worldZ, VoxelType type)
    {
        if (!TryGetChunkAndLocal(worldX, worldY, worldZ, out ChunkData chunk, out Vector3Int localPos))
        {
            return;
        }

        chunk.SetLocalVoxel(localPos.x, localPos.y, localPos.z, type);
    }

    public bool IsSolid(int worldX, int worldY, int worldZ)
    {
        return GetVoxel(worldX, worldY, worldZ) != VoxelType.Air;
    }

    public bool HasExposedFace(int worldX, int worldY, int worldZ)
    {
        if (!IsSolid(worldX, worldY, worldZ))
        {
            return false;
        }

        return !IsSolid(worldX + 1, worldY, worldZ) ||
               !IsSolid(worldX - 1, worldY, worldZ) ||
               !IsSolid(worldX, worldY + 1, worldZ) ||
               !IsSolid(worldX, worldY - 1, worldZ) ||
               !IsSolid(worldX, worldY, worldZ + 1) ||
               !IsSolid(worldX, worldY, worldZ - 1);
    }

    public Vector3Int WorldToChunkCoord(int worldX, int worldY, int worldZ)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldX / (float)chunkSize),
            Mathf.FloorToInt(worldY / (float)chunkSize),
            Mathf.FloorToInt(worldZ / (float)chunkSize)
        );
    }

    public Vector3Int WorldToLocalCoord(int worldX, int worldY, int worldZ)
    {
        return new Vector3Int(
            Mod(worldX, chunkSize),
            Mod(worldY, chunkSize),
            Mod(worldZ, chunkSize)
        );
    }

    public Vector3Int ChunkToWorldOrigin(Vector3Int chunkCoord)
    {
        return chunkCoord * chunkSize;
    }

    private bool TryGetChunkAndLocal(int worldX, int worldY, int worldZ, out ChunkData chunk, out Vector3Int localPos)
    {
        Vector3Int chunkCoord = WorldToChunkCoord(worldX, worldY, worldZ);
        localPos = WorldToLocalCoord(worldX, worldY, worldZ);

        return chunks.TryGetValue(chunkCoord, out chunk);
    }

    private int Mod(int value, int size)
    {
        int result = value % size;
        if (result < 0)
        {
            result += size;
        }
        return result;
    }
}