using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkEdgeRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;

    private void Awake() => meshFilter = GetComponent<MeshFilter>();

    // 注意：这个组件不会设置自己的 transform，定位靠 parent
    public void Build(IVoxelSource source, Vector3Int chunkCoord, int chunkSize)
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        Vector3Int origin = chunkCoord * chunkSize;
        var vertices = new List<Vector3>();
        var indices = new List<int>();

        for (int x = 0; x < chunkSize; x++)
        for (int y = 0; y < chunkSize; y++)
        for (int z = 0; z < chunkSize; z++)
        {
            int wx = origin.x + x, wy = origin.y + y, wz = origin.z + z;
            if (source.GetVoxel(wx, wy, wz) == VoxelType.Air) continue;

            Vector3 p = new Vector3(x, y, z);
            if (source.GetVoxel(wx, wy + 1, wz) == VoxelType.Air) AddFace(vertices, indices, p, Face.Up);
            if (source.GetVoxel(wx, wy - 1, wz) == VoxelType.Air) AddFace(vertices, indices, p, Face.Down);
            if (source.GetVoxel(wx - 1, wy, wz) == VoxelType.Air) AddFace(vertices, indices, p, Face.Left);
            if (source.GetVoxel(wx + 1, wy, wz) == VoxelType.Air) AddFace(vertices, indices, p, Face.Right);
            if (source.GetVoxel(wx, wy, wz + 1) == VoxelType.Air) AddFace(vertices, indices, p, Face.Forward);
            if (source.GetVoxel(wx, wy, wz - 1) == VoxelType.Air) AddFace(vertices, indices, p, Face.Back);
        }

        Mesh mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        mesh.name = $"ChunkEdges_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}";

        if (meshFilter.sharedMesh != null && Application.isPlaying)
            Object.Destroy(meshFilter.sharedMesh);

        meshFilter.sharedMesh = mesh;
    }

    private enum Face { Up, Down, Left, Right, Forward, Back }

    private static void AddFace(List<Vector3> v, List<int> i, Vector3 o, Face f)
    {
        int b = v.Count;
        switch (f)
        {
            case Face.Up:
                v.Add(o + new Vector3(0,1,0)); v.Add(o + new Vector3(1,1,0));
                v.Add(o + new Vector3(1,1,1)); v.Add(o + new Vector3(0,1,1)); break;
            case Face.Down:
                v.Add(o + new Vector3(0,0,0)); v.Add(o + new Vector3(1,0,0));
                v.Add(o + new Vector3(1,0,1)); v.Add(o + new Vector3(0,0,1)); break;
            case Face.Left:
                v.Add(o + new Vector3(0,0,0)); v.Add(o + new Vector3(0,1,0));
                v.Add(o + new Vector3(0,1,1)); v.Add(o + new Vector3(0,0,1)); break;
            case Face.Right:
                v.Add(o + new Vector3(1,0,0)); v.Add(o + new Vector3(1,1,0));
                v.Add(o + new Vector3(1,1,1)); v.Add(o + new Vector3(1,0,1)); break;
            case Face.Forward:
                v.Add(o + new Vector3(0,0,1)); v.Add(o + new Vector3(1,0,1));
                v.Add(o + new Vector3(1,1,1)); v.Add(o + new Vector3(0,1,1)); break;
            case Face.Back:
                v.Add(o + new Vector3(0,0,0)); v.Add(o + new Vector3(1,0,0));
                v.Add(o + new Vector3(1,1,0)); v.Add(o + new Vector3(0,1,0)); break;
        }
        i.Add(b+0); i.Add(b+1);
        i.Add(b+1); i.Add(b+2);
        i.Add(b+2); i.Add(b+3);
        i.Add(b+3); i.Add(b+0);
    }
}