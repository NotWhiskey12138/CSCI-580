using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkMeshTester : MonoBehaviour
{
    private void Start()
    {
        IVoxelData data = new MockVoxelData();
        ChunkMesher mesher = new ChunkMesher();

        Mesh mesh = mesher.BuildMesh(data);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}