using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void setBlock(int x1, int y1, int z1, int x2, int y2, int z2, byte type, Queue<VoxelMod>[,] queue)
    {
        int chunkX;
        int chunkY;

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                chunkX = x / VoxelData.chunkSize;
                chunkY = y / VoxelData.chunkSize;

                if (queue[chunkX, chunkY] == null)
                    queue[chunkX, chunkY] = new Queue<VoxelMod>();

                for (int z = z1; z <= z2; z++)
                {
                    queue[chunkX, chunkY].Enqueue(new VoxelMod(new Vector3(x - chunkX * VoxelData.chunkSize, z, y - chunkY * VoxelData.chunkSize), type));
                }
            }
        }
    }

    public static bool makeTree(Vector3 pos, Queue<VoxelMod>[,] queue, int minHeight, int maxHeight)
    {
        int height = SeedRandom.Get((int)pos.x, (int)pos.y) % (maxHeight - minHeight + 1) + minHeight;

        int chunkX = (int) pos.x / VoxelData.chunkSize;
        int chunkY = (int) pos.z / VoxelData.chunkSize;

        pos.x -= chunkX * VoxelData.chunkSize;
        pos.z -= chunkY * VoxelData.chunkSize;

        if (queue[chunkX, chunkY] == null)
            queue[chunkX, chunkY] = new Queue<VoxelMod>();

        for (int i = 0; i < height; i++)
            queue[chunkX, chunkY].Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + i, pos.z), Blocks.wood));

        queue[chunkX, chunkY].Enqueue(new VoxelMod(new Vector3(pos.x, pos.y + height, pos.z), Blocks.leave));

        pos.x += chunkX * VoxelData.chunkSize;
        pos.z += chunkY * VoxelData.chunkSize;

        setBlock((int)pos.x - 2, (int)pos.z - 2, (int)pos.y + height - 4, (int)pos.x + 2, (int)pos.z + 2, (int)pos.y + height, Blocks.leave, queue);

        return true;
    }
}
