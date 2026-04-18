using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    // 由 ChunkViewManager 调用：生成这个 chunk 的 mesh 并摆到正确位置
    public void Build(IVoxelSource source, Vector3Int chunkCoord, int chunkSize)
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        Vector3Int origin = chunkCoord * chunkSize;

        // chunk 的 mesh 顶点在本地空间 (0..chunkSize)，
        // 所以 GameObject 放到 origin，就和世界坐标对齐了
        transform.localPosition = origin;

        var mesher = new ChunkMesher();
        Mesh mesh = mesher.BuildMesh(source, origin, chunkSize);
        mesh.name = $"ChunkMesh_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}";

        // 运行时销毁旧 mesh，避免内存泄漏
        if (meshFilter.sharedMesh != null && Application.isPlaying)
        {
            Object.Destroy(meshFilter.sharedMesh);
        }

        meshFilter.sharedMesh = mesh;
    }
}