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

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    private bool _isActive;
    public bool isPopulated = false;

    byte[,,] blocks = new byte[VoxelData.chunkSize, VoxelData.chunkSize, VoxelData.chunkHeight];

    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad)
    {
        IsActive = true;
        coord = _coord;
        world = _world;

        if(generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkSize, 0.0f, coord.y * VoxelData.chunkSize);
        chunkObject.name = "Chunk (" + coord.x + ", " + coord.y + ")";

        GenerateBlocks();
        CreateMeshData();
        CreateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        if (!_isActive)
            chunkObject.SetActive(false);
    }

    void GenerateBlocks()
    {        
        for (int x = 0; x < VoxelData.chunkSize; x++)
        {
            for (int y = 0; y < VoxelData.chunkSize; y++)
            {
                int biome = Noise.GetBiome((int)(Position.x + x), (int)(Position.z + y), world.seed, world.basicBiomeGrid, world.biomes, world.biomeNoiseMult, world.biomeNoiseDist);

                int height = Noise.GetWeight((int)(Position.x + x), (int)(Position.z + y), world); //GenerateHeight((int)(Position.x + x), (int)(Position.z + y), world.biomes[biome].heightCurve, world.seed, world.biomes[biome].scale, world.biomes[biome].octaves, world.biomes[biome].persistance, world.biomes[biome].lacunarity);
                
                for (int i = 0; i < VoxelData.chunkHeight; i++)
                {
                    blocks[x, y, i] = world.GetVoxel(Position + new Vector3(x, i, y), height, biome);
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
            return world.CheckForVoxel(pos + Position);

        return world.blockTypes[blocks[x, y, z]].isSolid;
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
