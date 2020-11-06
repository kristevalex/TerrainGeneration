﻿using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    float zeroX;
    float zeroY;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        zeroX = VoxelData.worldSizeInVoxels / 2;
        zeroY = VoxelData.worldSizeInVoxels / 2;
    }

    void Update()
    {
        int biome = Noise.GetBiome((int)(world.player.position.x), (int)(world.player.position.z), world.seed, world.basicBiomeGrid, 
                                   world.biomes.Length, world.biomeNoiseMult, world.biomeNoiseDist);

        text.text = "Alexey Kristev's Minecraft like game\n" +
                    frameRate + " fps\n" +
                    "XYZ: " + ( - zeroX) + " / " + world.player.position.y + " / " + (world.player.position.z - zeroY) + "\n" +
                    "Biome: " + world.biomes[biome].name + "\n";


        if (timer > 1f)
        {
            frameRate = (1f / Time.unscaledDeltaTime);
            timer = 0f;
        }
        else
            timer += Time.deltaTime;
    }
}
