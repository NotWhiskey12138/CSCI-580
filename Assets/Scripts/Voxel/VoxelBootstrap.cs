using System.Collections.Generic;
using UnityEngine;

public class VoxelBootstrap : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private ChunkViewManager viewManager;
    [SerializeField] private Transform player;
    [SerializeField] private int radius;

    private int playerPosX;
    private int playerPosY;
    private int playerPosZ;
    private void Start()
    {
        BuildMinimalWorld();
    }

    public void Update()
    {
        playerPosX = (int)player.position.x;
        playerPosY = (int)player.position.y;
        playerPosZ = (int)player.position.z;
        
        if (Input.GetKey(KeyCode.R))
        {
            List<Vector3Int> affectedChunks = world.UpdateChunk(new Vector3Int(playerPosX, playerPosY, playerPosZ), radius);
            viewManager.RebuildChunks(affectedChunks);
            Debug.Log($"Player at ({playerPosX}, {playerPosY}, {playerPosZ}) destroyed voxels in radius {radius}. Affected chunks: {affectedChunks.Count}");
        }
    }

    [ContextMenu("Build Minimal World")]
    public void BuildMinimalWorld()
    {
        if (world == null || viewManager  == null)
        {
            Debug.LogWarning("VoxelBootstrap: Please assign both World and DebugView.");
            return;
        }

        world.GenerateWorld();
        viewManager.Rebuild();
    }
}