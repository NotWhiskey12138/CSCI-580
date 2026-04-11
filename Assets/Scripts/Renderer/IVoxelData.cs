public interface IVoxelData
{
    int SizeX { get; }
    int SizeY { get; }
    int SizeZ { get; }

    BlockType GetBlock(int x, int y, int z);
}