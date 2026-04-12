public class MockVoxelData : IVoxelData
{
    public int SizeX => 16;
    public int SizeY => 16;
    public int SizeZ => 16;

    public BlockType GetBlock(int x, int y, int z)
    {
        if (y < 4) return BlockType.Dirt;
        if (y == 4) return BlockType.Grass;
        return BlockType.Air;
    }
}