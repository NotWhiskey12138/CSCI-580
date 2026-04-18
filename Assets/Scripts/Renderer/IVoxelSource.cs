using UnityEngine;

public interface IVoxelSource
{
    VoxelType GetVoxel(int worldX, int worldY, int worldZ);
}
