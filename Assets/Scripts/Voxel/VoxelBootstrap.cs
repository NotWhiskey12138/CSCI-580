using UnityEngine;

public class VoxelBootstrap : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private VoxelWorldDebugView debugView;

    private void Start()
    {
        BuildMinimalWorld();
    }

    [ContextMenu("Build Minimal World")]
    public void BuildMinimalWorld()
    {
        if (world == null || debugView == null)
        {
            Debug.LogWarning("VoxelBootstrap: Please assign both World and DebugView.");
            return;
        }

        world.GenerateWorld();
        debugView.Rebuild();
    }
}