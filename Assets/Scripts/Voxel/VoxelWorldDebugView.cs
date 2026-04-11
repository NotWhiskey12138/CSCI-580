using System.Collections.Generic;
using UnityEngine;

public class VoxelWorldDebugView : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private float voxelScale = 1f;
    [SerializeField] private bool onlyShowExposedVoxels = true;

    private readonly List<GameObject> spawnedCubes = new();

    [ContextMenu("Rebuild Debug View")]
    public void Rebuild()
    {
        Clear();

        if (world == null)
        {
            Debug.LogWarning("VoxelWorldDebugView: World reference is missing.");
            return;
        }

        foreach (var pair in world.LoadedChunks)
        {
            ChunkData chunk = pair.Value;
            Vector3Int origin = world.ChunkToWorldOrigin(chunk.ChunkCoord);

            for (int x = 0; x < chunk.Size; x++)
            {
                for (int y = 0; y < chunk.Size; y++)
                {
                    for (int z = 0; z < chunk.Size; z++)
                    {
                        VoxelType type = chunk.GetLocalVoxel(x, y, z);

                        if (type == VoxelType.Air)
                        {
                            continue;
                        }

                        int worldX = origin.x + x;
                        int worldY = origin.y + y;
                        int worldZ = origin.z + z;

                        if (onlyShowExposedVoxels && !world.HasExposedFace(worldX, worldY, worldZ))
                        {
                            continue;
                        }

                        CreateCube(worldX, worldY, worldZ, type);
                    }
                }
            }
        }
    }

    [ContextMenu("Clear Debug View")]
    public void Clear()
    {
        for (int i = 0; i < spawnedCubes.Count; i++)
        {
            if (spawnedCubes[i] == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(spawnedCubes[i]);
            }
            else
            {
                DestroyImmediate(spawnedCubes[i]);
            }
        }

        spawnedCubes.Clear();
    }

    private void CreateCube(int worldX, int worldY, int worldZ, VoxelType type)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Voxel_{worldX}_{worldY}_{worldZ}_{type}";
        cube.transform.SetParent(transform);

        cube.transform.localPosition = new Vector3(
            worldX + 0.5f,
            worldY + 0.5f,
            worldZ + 0.5f
        ) * voxelScale;

        cube.transform.localScale = Vector3.one * voxelScale;

        spawnedCubes.Add(cube);
    }
}