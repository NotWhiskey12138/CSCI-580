using System;
using UnityEngine;

public class VoxelBootstrap : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private ChunkViewManager viewManager;

    private void Start()
    {
        BuildMinimalWorld();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            world.updateChunk(new Vector3Int(0, 10, 0), 10);
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