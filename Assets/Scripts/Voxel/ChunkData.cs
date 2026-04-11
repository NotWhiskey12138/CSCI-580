using UnityEngine;

public class ChunkData
{
    public Vector3Int ChunkCoord { get; }
    public int Size { get; }

    private readonly VoxelType[,,] voxels;

    public ChunkData(Vector3Int chunkCoord, int size)
    {
        ChunkCoord = chunkCoord;
        Size = Mathf.Max(1, size);
        voxels = new VoxelType[Size, Size, Size];
    }

    public bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < Size &&
               y >= 0 && y < Size &&
               z >= 0 && z < Size;
    }

    public VoxelType GetLocalVoxel(int x, int y, int z)
    {
        if (!IsInBounds(x, y, z))
        {
            return VoxelType.Air;
        }

        return voxels[x, y, z];
    }

    public void SetLocalVoxel(int x, int y, int z, VoxelType type)
    {
        if (!IsInBounds(x, y, z))
        {
            return;
        }

        voxels[x, y, z] = type;
    }
}