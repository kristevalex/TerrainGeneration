using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, Mesh, Biomes};
    public DrawMode drawMode;

    public int mapHeight;
    public int mapWidth;
    public float noiseScale;
    public bool autoUpdate;
    public float heightMult;
    public AnimationCurve animationCurve;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int biomeGrid;
    public float noiseMult;
    public float noiseDist;

    public int seed;
    public Vector2 offset;

    public TerrainType[] regions;
    public BiomeType[] biomes;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        int[,] biomeMap = Noise.GenerateBiomeMap(mapWidth, mapHeight, seed, biomeGrid, biomes.Length, noiseMult, noiseDist);

        Color[] regionsMap = new Color[mapHeight * mapWidth];
        Color[] biomesColorMap = new Color[mapHeight * mapWidth];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float curHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (curHeight <= regions[i].height)
                    {
                        regionsMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }

                biomesColorMap[y * mapWidth + x] = biomes[biomeMap[x, y]].color;
            }
        }

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(regionsMap, mapWidth, mapHeight));
        else if (drawMode == DrawMode.Mesh)
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerreinMesh(noiseMap, heightMult, animationCurve), TextureGenerator.TextureFromColorMap(regionsMap, mapWidth, mapHeight));
        else if (drawMode == DrawMode.Biomes)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(biomesColorMap, mapWidth, mapHeight));
    }

    private void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;

        if (lacunarity < 1)
            lacunarity = 1;

        if (biomeGrid < 1)
            biomeGrid = 1;

        if (noiseMult < 1)
            noiseMult = 1;

        if (noiseDist < 0.0001f)
            noiseDist = 0.0001f;

        if (octaves < 0)
            octaves = 0;

        if (seed < 1)
            seed = 1;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

[System.Serializable]
public struct BiomeType
{
    public string name;
    public float noiseHeight;
    public Color color;
}
