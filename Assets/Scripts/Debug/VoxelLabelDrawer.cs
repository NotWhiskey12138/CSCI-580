using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class VoxelLabelDrawer : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private bool showCoords = false;
    [SerializeField] private int fontSize = 9;
    [SerializeField] private Color color = Color.white;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (world == null) return;
        var sceneCam = SceneView.currentDrawingSceneView?.camera;
        if (sceneCam == null) return;

        var camPos = sceneCam.transform.position;
        float maxDistSq = maxDistance * maxDistance;

        var style = new GUIStyle
        {
            normal = { textColor = color },
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize
        };

        foreach (var pair in world.LoadedChunks)
        {
            var chunk = pair.Value;
            var origin = world.ChunkToWorldOrigin(chunk.ChunkCoord);

            for (int x = 0; x < chunk.Size; x++)
            for (int y = 0; y < chunk.Size; y++)
            for (int z = 0; z < chunk.Size; z++)
            {
                if (chunk.GetLocalVoxel(x, y, z) == VoxelType.Air) continue;

                int wx = origin.x + x, wy = origin.y + y, wz = origin.z + z;
                if (!world.HasExposedFace(wx, wy, wz)) continue;

                Vector3 center = new Vector3(wx + 0.5f, wy + 0.5f, wz + 0.5f);
                if ((center - camPos).sqrMagnitude > maxDistSq) continue;

                VoxelType t = chunk.GetLocalVoxel(x, y, z);
                string label = showCoords ? $"{t}\n({wx},{wy},{wz})" : t.ToString();
                Handles.Label(center, label, style);
            }
        }
    }
#endif
}