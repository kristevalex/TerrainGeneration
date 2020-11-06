using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("Thecnical")]
    public Transform player;
    [SerializeField]
    private GameObject debugScreen;
    Vector3 playerPrevUpdate;
    Vector3 spawnPosition;
    
    public Material material;

    [Space(10)]

    [Header("Blocks")]
    public BlockType[] blockTypes;

    [Space(10)]

    [Header("Biomes")]
    [SerializeField]
    public int seed;

    public BiomeType[] biomes;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    List<ChunkCoord> chuncksToCreate = new List<ChunkCoord>();
    bool isCreatingChuncks;

    private void Start()
    {
        isCreatingChuncks = false;

        int biome = 1;

        spawnPosition = new Vector3(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f, 
                                    Noise.GenerateHeight((int)(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f), (int)(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f),
                                    biomes[biome].heightCurve, seed, biomes[biome].scale, biomes[biome].octaves, biomes[biome].persistance, biomes[biome].lacunarity) + 2, 
                                    VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f);
        
        GenerateWorld();
    }

    private void Update()
    {
        if(GetChunkCoordFronPos(playerPrevUpdate) != GetChunkCoordFronPos(player.position))
            CheckDistance();

        playerPrevUpdate = player.position;

        if (chuncksToCreate.Count > 0 && !isCreatingChuncks)
            StartCoroutine("CreateChuncks");

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    IEnumerator CreateChuncks()
    {
        isCreatingChuncks = true;

        while (chuncksToCreate.Count > 0)
        {
            chunks[chuncksToCreate[0].x, chuncksToCreate[0].y].Init();
            chuncksToCreate.RemoveAt(0);

            yield return null;
        }

        isCreatingChuncks = false;
    }

    void GenerateWorld()
    {
        for (int x = VoxelData.worldSizeInChunks / 2 - VoxelData.viewDistInChuncks; x < VoxelData.worldSizeInChunks / 2 + VoxelData.viewDistInChuncks; x++)
        {
            for (int y = VoxelData.worldSizeInChunks / 2 - VoxelData.viewDistInChuncks; y < VoxelData.worldSizeInChunks / 2 + VoxelData.viewDistInChuncks; y++)
            {
                chunks[x, y] = new Chunk(new ChunkCoord(x, y), this, true);
            }
        }

        player.position = spawnPosition;
        Physics.SyncTransforms();
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos))
            return false;

        ChunkCoord voxelChunck = new ChunkCoord(pos);

        if (chunks[voxelChunck.x, voxelChunck.y] != null && chunks[voxelChunck.x, voxelChunck.y].isPopulated)
            return blockTypes[chunks[voxelChunck.x, voxelChunck.y].GetVoxelFromGlobalVector(pos)].isSolid;

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public byte GetVoxel(Vector3 pos, int height = -1) 
    {
        if (!IsVoxelInWorld(pos))
            return 0;

        int biome = 1;

        if (height == -1)
            height = Noise.GenerateHeight((int)pos.x, (int)pos.z, biomes[biome].heightCurve, seed, biomes[biome].scale, biomes[biome].octaves, biomes[biome].persistance, biomes[biome].lacunarity);

        if (pos.y == 0)
            return Blocks.bedrock;
        else if (pos.y > height)
            return Blocks.air;
        else if (pos.y == height)
            return biomes[biome].topBlock;
        else if (pos.y >= height - 3)
            return biomes[biome].topLayer;
        else
            return Blocks.stone;
    }

    bool IsChunkInWorld(ChunkCoord chunkCoord)
    {
        if (chunkCoord.x < 0 || chunkCoord.x > VoxelData.worldSizeInChunks - 1)
            return false;
        if (chunkCoord.y < 0 || chunkCoord.y > VoxelData.worldSizeInChunks - 1)
            return false;

        return true;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x < 0 || pos.x >= VoxelData.worldSizeInVoxels)
            return false;
        if (pos.z < 0 || pos.z >= VoxelData.worldSizeInVoxels)
            return false;
        if (pos.y < 0 || pos.y >= VoxelData.chunkHeight)
            return false;
       

        return true;
    }

    ChunkCoord GetChunkCoordFronPos(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkSize);
        int y = Mathf.FloorToInt(pos.z / VoxelData.chunkSize);

        return new ChunkCoord(x, y);
    }

    void CheckDistance()
    {
        ChunkCoord coord = GetChunkCoordFronPos(player.position);
        ChunkCoord coordPrev = GetChunkCoordFronPos(playerPrevUpdate);

        for (int x = coord.x - VoxelData.viewDistInChuncks; x < coord.x + VoxelData.viewDistInChuncks; x++)
        {
            for (int y = coord.y - VoxelData.viewDistInChuncks; y < coord.y + VoxelData.viewDistInChuncks; y++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, y)))
                {
                    if (chunks[x, y] == null)
                    {
                        chunks[x, y] = new Chunk(new ChunkCoord(x, y), this, false);
                        chuncksToCreate.Add(new ChunkCoord(x, y));
                    }
                    else if (chunks[x, y].IsActive == false)
                        chunks[x, y].IsActive = true;
                }
            }
        }

        for (int x = coordPrev.x - VoxelData.viewDistInChuncks - 1; x < coordPrev.x + VoxelData.viewDistInChuncks + 1; x++)
        {
            for (int y = coordPrev.y - VoxelData.viewDistInChuncks - 1; y < coordPrev.y + VoxelData.viewDistInChuncks + 1; y++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, y)))
                {
                    if (x >= coord.x - VoxelData.viewDistInChuncks && x < coord.x + VoxelData.viewDistInChuncks &&
                        y >= coord.y - VoxelData.viewDistInChuncks && y < coord.y + VoxelData.viewDistInChuncks)
                        continue;

                    if (chunks[x, y] == null)
                        continue;

                    if (chunks[x, y].IsActive == true)
                        chunks[x, y].IsActive = false;
                }
            }
        }
    }
}

[System.Serializable]
public struct BlockType
{
    public string name;
    public bool isSolid;

    [Header("Texture values")]
    public int backFace;
    public int frontFace;
    public int topFace;
    public int buttomFace;
    public int leftFace;
    public int rightFace;

    // Back, Front, Top, Buttom, Left, Right
    public int GetTextureId(int faceId)
    {
        switch(faceId)
        {
            case 0:
                return backFace;
            case 1:
                return frontFace;
            case 2:
                return topFace;
            case 3:
                return buttomFace;
            case 4:
                return leftFace;
            case 5:
                return rightFace;
            default:
                return 0;
        }
    }
}

[System.Serializable]
public struct BiomeType
{
    public string name;
    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public AnimationCurve heightCurve;
    public byte topBlock;
    public byte topLayer;
}