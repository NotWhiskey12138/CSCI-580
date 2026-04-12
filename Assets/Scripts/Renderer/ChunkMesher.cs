using System.Collections.Generic;
using UnityEngine;

public class ChunkMesher
{
    public Mesh BuildMesh(IVoxelData data)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        for (int x = 0; x < data.SizeX; x++)
        {
            for (int y = 0; y < data.SizeY; y++)
            {
                for (int z = 0; z < data.SizeZ; z++)
                {
                    BlockType block = data.GetBlock(x, y, z);
                    if (block == BlockType.Air) continue;

                    Vector3 pos = new Vector3(x, y, z);

                    if (IsAir(data, x, y + 1, z)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Up);
                    if (IsAir(data, x, y - 1, z)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Down);
                    if (IsAir(data, x - 1, y, z)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Left);
                    if (IsAir(data, x + 1, y, z)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Right);
                    if (IsAir(data, x, y, z + 1)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Forward);
                    if (IsAir(data, x, y, z - 1)) AddFace(vertices, triangles, uvs, pos, FaceDirection.Back);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    private bool IsAir(IVoxelData data, int x, int y, int z)
    {
        if (x < 0 || x >= data.SizeX || y < 0 || y >= data.SizeY || z < 0 || z >= data.SizeZ)
            return true;

        return data.GetBlock(x, y, z) == BlockType.Air;
    }

    private enum FaceDirection
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }

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