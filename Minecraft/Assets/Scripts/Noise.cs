using UnityEngine;

public static class Noise
{
    public static int GenerateHeight(int x, int y, AnimationCurve heightCurve, int seed, float scale, int octaves, float persistance, float lacunarity)
    {
        System.Random prng = new System.Random(seed);

        float offsetXg = prng.Next(-100000, 100000);
        float offsetYg = prng.Next(-100000, 100000);

        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale < 0.0001f)
            scale = 0.0001f;

        float maxValue = 0;
        
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            maxValue += 1f * amplitude;

            float sampleX = (x + offsetXg) / scale * frequency + octaveOffsets[i].x;
            float sampleY = (y + offsetYg) / scale * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }


        return Mathf.FloorToInt(heightCurve.Evaluate(noiseHeight / maxValue));
    }

    public static int[,] GenerateBiomeMap(int mapWidth, int mapHeight, int seed, int biomesGrid, int biomesNum, float noiseMult, float noiseDist)
    {
        int[,] biomesMap = new int[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        SeedRandom.SetSeed(seed);

        int xS = prng.Next(10, 20);
        int yS = prng.Next(10, 20);

        float offsetX = prng.Next(-100000, 100000);
        float offsetY = prng.Next(-100000, 100000);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int gridX = (int)Mathf.Floor(x / biomesGrid);
                int gridY = (int)Mathf.Floor(y / biomesGrid);

                if (x / biomesGrid - gridX > 0.5f)
                    gridX -= 2;
                else
                    gridX -= 1;

                if (y / biomesGrid - gridY > 0.5f)
                    gridY -= 2;
                else
                    gridY -= 1;

                int closest = 0;
                int closestDist = int.MaxValue;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int curBiome = i * 4 + j;
                        int biomeX = SeedRandom.Get(gridX + i, gridY + j) % biomesGrid;
                        int biomeY = SeedRandom.Get(gridX + i, gridY + j) % biomesGrid;

                        int dist = ((gridX + i) * biomesGrid + biomeX - x) * ((gridX + i) * biomesGrid + biomeX - x) +
                                   ((gridY + j) * biomesGrid + biomeY - y) * ((gridY + j) * biomesGrid + biomeY - y);

                        dist += (int)(Mathf.PerlinNoise(noiseDist * ((gridX + i) * biomesGrid + biomeX - x + offsetX) / 100f,
                                                         noiseDist * ((gridY + j) * biomesGrid + biomeY - y + offsetY) / 100f) * noiseMult);

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = curBiome;
                        }
                    }
                }

                biomesMap[x, y] = SeedRandom.Get(gridX + closest / 4, gridY + closest % 4) % biomesNum;

            }
        }

        return biomesMap;
    }
}


