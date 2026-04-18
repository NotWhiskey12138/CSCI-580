using UnityEngine;

public class VoxelBootstrap : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private ChunkViewManager viewManager;

    private void Start()
    {
        BuildMinimalWorld();
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