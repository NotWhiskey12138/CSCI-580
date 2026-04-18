using System;
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
            world.updateChunk(new Vector3Int(playerPosX,playerPosY,playerPosZ), radius);
            viewManager.Rebuild();
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