using System.Collections.Generic;
using UnityEngine;

public class ChunkMesher
{
    // origin = chunk 左下角的世界坐标；size = chunk 边长
    // 返回的 mesh 顶点在 chunk 本地空间（0..size），定位靠 transform.position = origin
    public Mesh BuildMesh(IVoxelSource source, Vector3Int origin, int size)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    int wx = origin.x + x;
                    int wy = origin.y + y;
                    int wz = origin.z + z;

                    VoxelType block = source.GetVoxel(wx, wy, wz);
                    if (block == VoxelType.Air) continue;

                    Vector3 pos = new Vector3(x, y, z); // 本地坐标

                    if (y == size - 1 || IsAir(source, wx, wy + 1, wz)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Up);
                    if (y == 0 || IsAir(source, wx, wy - 1, wz)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Down);
                    if (x == 0 || IsAir(source, wx - 1, wy, wz)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Left);
                    if (x == size - 1 || IsAir(source, wx + 1, wy, wz)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Right);
                    if (z == size - 1 || IsAir(source, wx, wy, wz + 1)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Forward);
                    if (z == 0 || IsAir(source, wx, wy, wz - 1)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Back);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private bool IsAir(IVoxelSource source, int wx, int wy, int wz)
    {
        return source.GetVoxel(wx, wy, wz) == VoxelType.Air;
    }

    private enum FaceDirection { Up, Down, Left, Right, Forward, Back }

    private void AddFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 pos, FaceDirection dir)
    {
        int start = vertices.Count;

        switch (dir)
        {
            case FaceDirection.Up:
                vertices.Add(pos + new Vector3(0, 1, 0));
                vertices.Add(pos + new Vector3(0, 1, 1));
                vertices.Add(pos + new Vector3(1, 1, 1));
                vertices.Add(pos + new Vector3(1, 1, 0));
                break;
            case FaceDirection.Down:
                vertices.Add(pos + new Vector3(0, 0, 0));
                vertices.Add(pos + new Vector3(1, 0, 0));
                vertices.Add(pos + new Vector3(1, 0, 1));
                vertices.Add(pos + new Vector3(0, 0, 1));
                break;
            case FaceDirection.Left:
                vertices.Add(pos + new Vector3(0, 0, 0));
                vertices.Add(pos + new Vector3(0, 0, 1));
                vertices.Add(pos + new Vector3(0, 1, 1));
                vertices.Add(pos + new Vector3(0, 1, 0));
                break;
            case FaceDirection.Right:
                vertices.Add(pos + new Vector3(1, 0, 1));
                vertices.Add(pos + new Vector3(1, 0, 0));
                vertices.Add(pos + new Vector3(1, 1, 0));
                vertices.Add(pos + new Vector3(1, 1, 1));
                break;
            case FaceDirection.Forward:
                vertices.Add(pos + new Vector3(0, 0, 1));
                vertices.Add(pos + new Vector3(1, 0, 1));
                vertices.Add(pos + new Vector3(1, 1, 1));
                vertices.Add(pos + new Vector3(0, 1, 1));
                break;
            case FaceDirection.Back:
                vertices.Add(pos + new Vector3(1, 0, 0));
                vertices.Add(pos + new Vector3(0, 0, 0));
                vertices.Add(pos + new Vector3(0, 1, 0));
                vertices.Add(pos + new Vector3(1, 1, 0));
                break;
        }

        triangles.Add(start + 0);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
    }
}
