using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;

    [Header("Thecnical")]
    public Transform player;
    [SerializeField]
    private GameObject debugScreen;
    Vector3 playerPrevUpdate;
    Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;

    [Space(10)]

    [Header("Blocks")]
    public BlockType[] blockTypes;

    [Space(10)]

    [Header("Biomes")]
    public int basicBiomeGrid;
    public float biomeNoiseMult;
    public float biomeNoiseDist;
    public float smoothnessMod;

    public BiomeType[] biomes;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    List<ChunkCoord> chuncksToCreate = new List<ChunkCoord>();
    bool isCreatingChuncks;

    Queue<VoxelMod>[,] modifications = new Queue<VoxelMod>[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];
    

    private void Start()
    {
        SeedRandom.SetSeed(seed);

        for (int i = 0; i < biomes.Length; i++)
        {
            VoxelData.biomeProbobilitySum += biomes[i].probobilityWeight;
        }

        isCreatingChuncks = false;

        
        spawnPosition = new Vector3(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f, 
                                    Noise.GetWeight((int)(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f), (int)(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f), this) + 2, 
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
            
            UpdateChuncks(chuncksToCreate[0]);

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


        for (int x = VoxelData.worldSizeInChunks / 2 - VoxelData.viewDistInChuncks; x < VoxelData.worldSizeInChunks / 2 + VoxelData.viewDistInChuncks; x++)
        {
            for (int y = VoxelData.worldSizeInChunks / 2 - VoxelData.viewDistInChuncks; y < VoxelData.worldSizeInChunks / 2 + VoxelData.viewDistInChuncks; y++)
            {
                if (chunks[x, y] != null && chunks[x, y].isReady && modifications[x, y] != null)
                {
                    while (modifications[x, y].Count > 0)
                    {
                        chunks[x, y].modifications.Enqueue(modifications[x, y].Dequeue());
                    }

                    chunks[x, y].UpdateChunk();
                }
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

    public bool CheckForVoxelTrancparency(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos))
            return false;

        ChunkCoord voxelChunck = new ChunkCoord(pos);

        if (chunks[voxelChunck.x, voxelChunck.y] != null && chunks[voxelChunck.x, voxelChunck.y].isPopulated)
            return blockTypes[chunks[voxelChunck.x, voxelChunck.y].GetVoxelFromGlobalVector(pos)].isTransparent;

        return blockTypes[GetVoxel(pos)].isTransparent;
    }

    public byte GetVoxel(Vector3 pos, int height = -1, int biome = -1) 
    {
        if (!IsVoxelInWorld(pos))
            return 0;

        if (biome == -1)
            biome = Noise.GetBiome(biomes, Noise.GetBiomes((int)pos.x, (int)pos.z, seed, basicBiomeGrid, biomes, biomeNoiseMult, biomeNoiseDist, smoothnessMod));

        if (height == -1)
            height = Noise.GetWeight((int)pos.x, (int)pos.z, this);
        if (pos.y == 0)
            return Blocks.bedrock;
        else if (pos.y > height)
        {
            if (pos.y == height + 1)
                for (int i = 0; i < biomes[biome].structures.Length; i++)
                    if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[biome].structures[i].treeZoneScale, seed) > biomes[biome].structures[i].treeZoneThreshold)
                        if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[biome].structures[i].treePlacementScale, seed) > biomes[biome].structures[i].treePlacementThreshold)
                            switch (biomes[biome].structures[i].name)
                            {
                                case "Tree":
                                    Structure.makeTree(pos, modifications, biomes[biome].structures[i].minHeight, biomes[biome].structures[i].maxHeight);
                                    break;

                                case "Cactus":
                                    Structure.makeCactus(pos, modifications, biomes[biome].structures[i].minHeight, biomes[biome].structures[i].maxHeight);
                                    break;
                            }


            return Blocks.air;
        }
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

    public void UpdateChuncks(ChunkCoord pos)
    {
        for (int x = pos.x - 1; x <= pos.x + 1; x++)
        {
            for (int y = pos.y - 1; y <= pos.y + 1; y++)
            {
                if (chunks[x, y] != null && chunks[x, y].isReady && modifications[x, y] != null)
                {
                    while (modifications[x, y].Count > 0)
                    {
                        chunks[x, y].modifications.Enqueue(modifications[x, y].Dequeue());
                    }

                    chunks[x, y].UpdateChunk();
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
    public bool isTransparent;

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
    public float strength;
    public int probobilityWeight;

    public StructureData[] structures;
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}