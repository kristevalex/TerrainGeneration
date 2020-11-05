using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField]
    MeshRenderer meshRenderer;
    [SerializeField]
    MeshFilter meshFilter;

    World world;

    int vertexId = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] blocks = new byte[VoxelData.chunkSize, VoxelData.chunkSize, VoxelData.chunkHeight];

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        GenerateBlocks();

        CreateMeshData();
        
        CreateMesh();
    }

    void GenerateBlocks()
    {
        for (int i = 0; i < VoxelData.chunkHeight / 2; i++)
        {
            for (int x = 0; x < VoxelData.chunkSize; x++)
            {
                for (int y = 0; y < VoxelData.chunkSize; y++)
                {
                    blocks[x, y, i] = 0;
                }
            }
        }
    }

    void CreateMeshData()
    {
        for (int i = 0; i < VoxelData.chunkHeight; i++)
        {
            for (int x = 0; x < VoxelData.chunkSize; x++)
            {
                for (int y = 0; y < VoxelData.chunkSize; y++)
                {
                    AddVoxelDataToChunck(new Vector3(x, i, y));
                }
            }
        }
    }

    bool CheckBlock(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.z);
        int z = Mathf.FloorToInt(pos.y);

        if (x < 0 || x > VoxelData.chunkSize - 1)
            return false;
        if (y < 0 || y > VoxelData.chunkSize - 1)
            return false;
        if (z < 0 || z > VoxelData.chunkHeight - 1)
            return false;

        return world.blockTypes[blocks[x, y, z]].isDolid;
    }

    void AddVoxelDataToChunck(Vector3 pos)
    {
        for (int i = 0; i < VoxelData.voxelTris.GetLength(0); i++)
        {
            if (!CheckBlock(pos + VoxelData.faceChecks[i]))
            {
                for (int j = 0; j < VoxelData.voxelTris.GetLength(1); j++)
                {
                    vertices.Add(pos + VoxelData.voxeVerts[VoxelData.voxelTris[i, j]]);
                }

                AddTexture(world.blockTypes[blocks[(int)pos.x, (int)pos.z, (int)pos.y]].GetTextureId(i));

                triangles.Add(vertexId++);
                triangles.Add(vertexId++);
                triangles.Add(vertexId);
                triangles.Add(vertexId--);
                triangles.Add(vertexId++);
                triangles.Add(++vertexId);
                ++vertexId;
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureId)
    {
        float y = textureId / VoxelData.textureAtlasSizeInBlocks;
        float x = textureId - y * VoxelData.textureAtlasSizeInBlocks;

        y = VoxelData.textureAtlasSizeInBlocks - 1 - y;

        x *= VoxelData.normalisezedBlockTextureSize;
        y *= VoxelData.normalisezedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.normalisezedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.normalisezedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.normalisezedBlockTextureSize, y + VoxelData.normalisezedBlockTextureSize));
    }
}
