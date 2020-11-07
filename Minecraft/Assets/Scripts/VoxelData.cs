using UnityEngine;

public static class VoxelData
{
    public static readonly int chunkSize = 16;
    public static readonly int chunkHeight = 120;
    public static readonly int worldSizeInChunks = 100;

    public static readonly int viewDistInChuncks = 6;

    public static int worldSizeInVoxels
    {
        get { return worldSizeInChunks * chunkSize; }
    }

    public static readonly int textureAtlasSizeInBlocks = 4;
    public static float normalisezedBlockTextureSize
    {
        get { return 1f / textureAtlasSizeInBlocks; }
    }


    public static readonly Vector3[] voxeVerts = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    public static readonly Vector3[] faceChecks = new  Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)
    };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        // Back, Front, Top, Buttom, Left, Right

        {0, 3, 1, 2}, // Back cube side
        {5, 6, 4, 7}, // Front cube side
        {3, 7, 2, 6}, // Top cube side
        {1, 5, 0, 4}, // Buttom cube side
        {4, 7, 0, 3}, // Left cube side
        {1, 2, 5, 6}  // Right cube side
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };
}
