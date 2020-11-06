using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class World : MonoBehaviour
{
    public Transform player;
    Vector3 playerPrevUpdate;
    Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    private void Start()
    {
        spawnPosition = new Vector3(VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f, VoxelData.chunkHeight + 2, VoxelData.worldSizeInChunks * VoxelData.chunkSize / 2f);
        
        GenerateWorld();
    }

    private void Update()
    {
        if(GetChunkCoordFronPos(playerPrevUpdate) != GetChunkCoordFronPos(player.position))
            CheckDistance();

        playerPrevUpdate = player.position;
    }

    void GenerateWorld()
    {
        for (int x = VoxelData.worldSizeInChunks / 2 - VoxelData.viewDistInChuncks; x < VoxelData.worldSizeInChunks / 2 + VoxelData.viewDistInChuncks; x++)
        {
            for (int y = VoxelData.worldSizeInChunks / 2 - VoxelData.viewDistInChuncks; y < VoxelData.worldSizeInChunks / 2 + VoxelData.viewDistInChuncks; y++)
            {
                GenrateChunck(x, y);
            }
        }

        player.position = spawnPosition;
        playerPrevUpdate = spawnPosition;
    }

    void GenrateChunck(int x, int y)
    {
        chunks[x, y] = new Chunk(new ChunkCoord(x, y), this);
    }

    public byte GetVoxel(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos))
            return 0;

        if (pos.y == 0)
            return Blocks.bedrock;
        else if (pos.y == VoxelData.chunkHeight - 1)
            return Blocks.grass;
        else if (pos.y >= VoxelData.chunkHeight - 4)
            return Blocks.dirt;
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
                        GenrateChunck(x, y);
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
