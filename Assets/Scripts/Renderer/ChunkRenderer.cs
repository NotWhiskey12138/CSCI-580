using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Build mesh for this chunk and place it at the correct world position
    public void Build(IVoxelSource source, Vector3Int chunkCoord, int chunkSize, VoxelTextureAtlas atlas, Material opaqueMaterial, Material fireMaterial, Material smokeMaterial)
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        Vector3Int origin = chunkCoord * chunkSize;

        // Mesh is built in local chunk space (0..chunkSize), so offset the object
        transform.localPosition = origin;

        var mesher = new ChunkMesher();
        Mesh mesh = mesher.BuildMesh(source, origin, chunkSize, atlas);
        mesh.name = $"ChunkMesh_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}";

        // Destroy old mesh to avoid runtime memory leak
        if (meshFilter.sharedMesh != null && Application.isPlaying)
        {
            Destroy(meshFilter.sharedMesh);
        }

        meshFilter.sharedMesh = mesh;
        if (meshRenderer != null)
        {
            meshRenderer.sharedMaterials = new[] { opaqueMaterial, fireMaterial, smokeMaterial };
        }

        // Force collider to refresh with new mesh (Unity requires null first)
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = null;
    }
}
