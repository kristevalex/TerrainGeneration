using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    int mapHeight;
    [SerializeField]
    int mapWidth;
    [SerializeField]
    float noiseScale;
    public bool autoUpdate;
    [Range(0, 1)]
    [SerializeField]
    float fillAmount;

    [SerializeField]
    int octaves;
    [Range(0, 1)]
    [SerializeField]
    float persistance;
    [SerializeField]
    float lacunarity;

    [SerializeField]
    int seed;

    [SerializeField]
    int smoothIterations;

    bool[,] map;

    MapDisplay mapDisplay;


    public void GenerateMap()
    {
        GenerateBaseMap();

        for (int i = 0; i < smoothIterations; i++)
            SmoothMap();

        DisplayMap();
    }

    void GenerateBaseMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity);

        map = new bool[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[x, y] = noiseMap[x, y] < fillAmount;
                if (y == 0 || x == 0 || y == mapHeight - 1 || x == mapWidth - 1)
                    map[x, y] = false;
            }
        }
    }

    void SmoothMap()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int neighbours = CountNeighbours(x, y);

                if (neighbours > 4)
                    map[x, y] = true;
                else if (neighbours < 4)
                    map[x, y] = false;
            }
        }
    }

    int CountNeighbours(int x, int y)
    {
        int neighboursNum = 0;
        for (int i = x - 1; i <= x + 1; i++)
            for (int j = y - 1; j <= y + 1; j++)
                if (i >= 0 && j >= 0 && i < mapWidth && j < mapHeight)
                    if (i != x || j != y)
                        if (map[i, j])
                            neighboursNum++;

        return neighboursNum;
    }

    void DisplayMap()
    {
        if (!mapDisplay)
            mapDisplay = FindObjectOfType<MapDisplay>();

        Color[] cavesMap = new Color[mapHeight * mapWidth];
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map[x, y])
                    cavesMap[y * mapWidth + x] = Color.white;
                else
                    cavesMap[y * mapWidth + x] = Color.black;
            }
        }

        mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(cavesMap, mapWidth, mapHeight));
    }

    private void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;

        if (lacunarity < 1)
            lacunarity = 1;

        if (octaves < 0)
            octaves = 0;

        if (seed < 1)
            seed = 1;
    }
}
