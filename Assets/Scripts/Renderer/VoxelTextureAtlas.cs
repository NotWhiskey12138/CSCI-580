using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/VoxelTextureAtlas")]
public class VoxelTextureAtlas : ScriptableObject
{
    [System.Serializable]
    public struct VoxelTile
    {
        public VoxelType type;
        public Vector2Int tileCoord; // tileCoord.x in [0..tilesX-1], tileCoord.y in [0..tilesY-1], (0,0) is bottom-left
    }

    [Header("Atlas texture")]
    public Texture2D atlasTexture;
    public int tilesX = 4;
    public int tilesY = 4;

    [Header("Per-type tile mapping")]
    public List<VoxelTile> tiles = new();

    private readonly Dictionary<VoxelType, Vector2Int> map = new();

    private void OnEnable()
    {
        map.Clear();
        foreach (var t in tiles)
        {
            if (!map.ContainsKey(t.type))
                map[t.type] = t.tileCoord;
        }
    }

    // Returns 4 UVs matching the face vertex order used by ChunkMesher (0,0),(1,0),(1,1),(0,1)
    public Vector2[] GetUVs(VoxelType type)
    {
        if (atlasTexture == null || tilesX <= 0 || tilesY <= 0)
        {
            // fallback: full 0..1 UVs
            return new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)
            };
        }

        if (!map.TryGetValue(type, out var coord))
        {
            // fallback to (0,0) tile if not configured
            coord = new Vector2Int(0, 0);
        }

        float tileW = 1f / tilesX;
        float tileH = 1f / tilesY;

        float u0 = coord.x * tileW;
        float v0 = coord.y * tileH;
        // local face UV order: (0,0),(1,0),(1,1),(0,1)
        return new Vector2[]
        {
            new Vector2(u0, v0),
            new Vector2(u0 + tileW, v0),
            new Vector2(u0 + tileW, v0 + tileH),
            new Vector2(u0, v0 + tileH)
        };
    }
}