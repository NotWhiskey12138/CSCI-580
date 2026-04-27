using System.Collections.Generic;
using UnityEngine;

public class ChunkMesher
{
    public const int OpaqueSubmesh = 0;
    public const int FireSubmesh = 1;
    public const int SmokeSubmesh = 2;

    // origin = chunk 左下角的世界坐标；size = chunk 边长
    // 返回的 mesh 顶点在 chunk 本地空间（0..size），定位靠 transform.position = origin
    public Mesh BuildMesh(IVoxelSource source, Vector3Int origin, int size, VoxelTextureAtlas atlas)
    {
        List<Vector3> vertices = new();
        List<int> opaqueTriangles = new();
        List<int> fireTriangles = new();
        List<int> smokeTriangles = new();
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

                    if (y == size - 1 || ShouldRenderFace(block, source.GetVoxel(wx, wy + 1, wz))) AddFace(vertices, opaqueTriangles, fireTriangles, smokeTriangles, uvs, pos, FaceDirection.Up, block, atlas);
                    if (y == 0 || ShouldRenderFace(block, source.GetVoxel(wx, wy - 1, wz))) AddFace(vertices, opaqueTriangles, fireTriangles, smokeTriangles, uvs, pos, FaceDirection.Down, block, atlas);
                    if (x == 0 || ShouldRenderFace(block, source.GetVoxel(wx - 1, wy, wz))) AddFace(vertices, opaqueTriangles, fireTriangles, smokeTriangles, uvs, pos, FaceDirection.Left, block, atlas);
                    if (x == size - 1 || ShouldRenderFace(block, source.GetVoxel(wx + 1, wy, wz))) AddFace(vertices, opaqueTriangles, fireTriangles, smokeTriangles, uvs, pos, FaceDirection.Right, block, atlas);
                    if (z == size - 1 || ShouldRenderFace(block, source.GetVoxel(wx, wy, wz + 1))) AddFace(vertices, opaqueTriangles, fireTriangles, smokeTriangles, uvs, pos, FaceDirection.Forward, block, atlas);
                    if (z == 0 || ShouldRenderFace(block, source.GetVoxel(wx, wy, wz - 1))) AddFace(vertices, opaqueTriangles, fireTriangles, smokeTriangles, uvs, pos, FaceDirection.Back, block, atlas);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.subMeshCount = 3;
        mesh.SetTriangles(opaqueTriangles, OpaqueSubmesh);
        mesh.SetTriangles(fireTriangles, FireSubmesh);
        mesh.SetTriangles(smokeTriangles, SmokeSubmesh);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private bool ShouldRenderFace(VoxelType current, VoxelType neighbor)
    {
        if (neighbor == VoxelType.Air)
        {
            return true;
        }

        if (IsTransient(current) || IsTransient(neighbor))
        {
            return current != neighbor;
        }

        return false;
    }

    private bool IsTransient(VoxelType voxelType)
    {
        return voxelType == VoxelType.Fire || voxelType == VoxelType.Smoke;
    }

    private enum FaceDirection { Up, Down, Left, Right, Forward, Back }

    private void AddFace(
        List<Vector3> vertices,
        List<int> opaqueTriangles,
        List<int> fireTriangles,
        List<int> smokeTriangles,
        List<Vector2> uvs,
        Vector3 pos,
        FaceDirection dir,
        VoxelType type,
        VoxelTextureAtlas atlas)
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

        List<int> triangles = type switch
        {
            VoxelType.Fire => fireTriangles,
            VoxelType.Smoke => smokeTriangles,
            _ => opaqueTriangles
        };
        triangles.Add(start + 0);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 3);

        // add UVs from atlas (fallback to default if atlas null)
        if (atlas != null)
        {
            var faceUvs = atlas.GetUVs(type);
            uvs.Add(faceUvs[0]);
            uvs.Add(faceUvs[1]);
            uvs.Add(faceUvs[2]);
            uvs.Add(faceUvs[3]);
        }
        else
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
        }
    }
}
