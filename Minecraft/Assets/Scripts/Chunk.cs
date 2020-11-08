using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Blocks
{
    public static byte air = 0;
    public static byte bedrock = 1;
    public static byte stone = 2;
    public static byte grass = 3;
    public static byte dirt = 4;
    public static byte sand = 5;
    public static byte glass = 6;
    public static byte wood = 7;
    public static byte leave = 8;
    public static byte cactus = 9;
}

public class Chunk 
{
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    ChunkCoord coord;

    World world;

    int vertexId = 0;
    public bool isReady = false;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
    List<Vector2> uvs = new List<Vector2>();

    private bool _isActive;
    public bool isPopulated = false;

    byte[,,] blocks = new byte[VoxelData.chunkSize, VoxelData.chunkSize, VoxelData.chunkHeight];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad)
    {
        IsActive = true;
        coord = _coord;
        world = _world;

        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkSize, 0.0f, coord.y * VoxelData.chunkSize);
        chunkObject.name = "Chunk (" + coord.x + ", " + coord.y + ")";

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        GenerateBlocks();
        CreateMeshData();
        CreateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        if (!_isActive)
            chunkObject.SetActive(false);

        isReady = true;
    }

    void GenerateBlocks()
    {        
        for (int x = 0; x < VoxelData.chunkSize; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize; y++)
            {
                float[] biomeWeights = Noise.GetBiomes((int)(Position.x + x), (int)(Position.z + y), world.seed, world.basicBiomeGrid, world.biomes, world.biomeNoiseMult, world.biomeNoiseDist, world.smoothnessMod);

                int height = Noise.GetWeight((int)(Position.x + x), (int)(Position.z + y), world, biomeWeights); 
                
                for (int i = 0; i < VoxelData.chunkHeight; i++)
                {
                    blocks[x, y, i] = world.GetVoxel(Position + new Vector3(x, i, y), height, Noise.GetBiome(world.biomes, biomeWeights));
                }
            }
        }

        isPopulated = true;
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

    bool IsBlockInChunck(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkSize - 1)
            return false;
        if (y < 0 || y > VoxelData.chunkSize - 1)
            return false;
        if (z < 0 || z > VoxelData.chunkHeight - 1)
            return false;

        return true;
    }

    bool CheckBlock(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.z);
        int z = Mathf.FloorToInt(pos.y);

        if (!IsBlockInChunck(x, y, z))
            return world.CheckForVoxelTrancparency(pos + Position);

        return world.blockTypes[blocks[x, y, z]].isTransparent;
    }

    public byte GetVoxelFromGlobalVector(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.z);
        int zCheck = Mathf.FloorToInt(pos.y);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return blocks[xCheck, yCheck, zCheck];
    }

    public bool IsActive
    {
        get { return _isActive; }
        set {
            _isActive = value;
            if(chunkObject != null)
                chunkObject.SetActive(value); 
            }
    }

    public Vector3 Position
    {
        get { return chunkObject.transform.position; }
    }

    void AddVoxelDataToChunck(Vector3 pos)
    {
        if (!world.blockTypes[blocks[(int)pos.x, (int)pos.z, (int)pos.y]].isSolid)
            return;

        byte blockId = blocks[(int)pos.x, (int)pos.z, (int)pos.y];
        bool isTransparent = world.blockTypes[blockId].isTransparent;


        for (int i = 0; i < VoxelData.voxelTris.GetLength(0); i++)
        {
            if (CheckBlock(pos + VoxelData.faceChecks[i]))
            {
                for (int j = 0; j < VoxelData.voxelTris.GetLength(1); j++)
                {
                    vertices.Add(pos + VoxelData.voxeVerts[VoxelData.voxelTris[i, j]]);
                }

                AddTexture(world.blockTypes[blockId].GetTextureId(i));

                if (!isTransparent)
                {
                    triangles.Add(vertexId++);
                    triangles.Add(vertexId++);
                    triangles.Add(vertexId);
                    triangles.Add(vertexId--);
                    triangles.Add(vertexId++);
                    triangles.Add(++vertexId);
                    ++vertexId;
                }
                else
                {
                    transparentTriangles.Add(vertexId++);
                    transparentTriangles.Add(vertexId++);
                    transparentTriangles.Add(vertexId);
                    transparentTriangles.Add(vertexId--);
                    transparentTriangles.Add(vertexId++);
                    transparentTriangles.Add(++vertexId);
                    ++vertexId;
                }
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            blocks[(int)v.position.x, (int)v.position.z, (int)v.position.y] = v.id;
        }

        ClearMeshData();

        for (int z = 0; z < VoxelData.chunkHeight; z++)
        {
            for (int x = 0; x < VoxelData.chunkSize; x++)
            {
                for (int y = 0; y < VoxelData.chunkSize; y++)
                {

                    if (world.blockTypes[blocks[x, y, z]].isSolid)
                        AddVoxelDataToChunck(new Vector3(x, z, y));

                }
            }
        }

        CreateMesh();
    }

    void ClearMeshData()
    {
        vertexId = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
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

public class ChunkCoord
{
    public int x;
    public int y;

    public ChunkCoord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public ChunkCoord()
    {
        x = 0;
        y = 0;
    }

    public ChunkCoord(Vector3 pos)
    {
        x = Mathf.FloorToInt(pos.x) / VoxelData.chunkSize;
        y = Mathf.FloorToInt(pos.z) / VoxelData.chunkSize;
    }

    public static bool operator ==(ChunkCoord ft, ChunkCoord sd)
    {
        if (ft.x != sd.x || ft.y != sd.y)
            return false;

        return true;
    }

    public static bool operator !=(ChunkCoord ft, ChunkCoord sd)
    {
        if (ft.x == sd.x && ft.y == sd.y)
            return false;

        return true;
    }
}
