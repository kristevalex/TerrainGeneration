using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    int seed;

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
    int smoothIterations;
    
    [SerializeField]
    int wallThresholdSize;
    [SerializeField]
    int caveThresholdSize;
        
    bool[,] map;

    MapDisplay mapDisplay;


    public void GenerateMap()
    {
        GenerateBaseMap();

        for (int i = 0; i < smoothIterations; i++)
            SmoothMap();

        RefineRegions();

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
                if (IsInMap(i, j))
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

    void RefineRegions()
    {
        List<List<Point>> wallRegions = GetRegions(false);

        foreach (List<Point> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Point wallPoint in wallRegion)
                {
                    map[wallPoint.x, wallPoint.y] = true;
                }
            }
        }

        List<List<Point>> caveRegions = GetRegions(true);

        foreach (List<Point> caveRegion in caveRegions)
        {
            if (caveRegion.Count < caveThresholdSize)
            {
                foreach (Point cavePoint in caveRegion)
                {
                    map[cavePoint.x, cavePoint.y] = false;
                }
            }
        }
    }

    List<List<Point>> GetRegions(bool tileType)
    {
        List<List<Point>> regions = new List<List<Point>>();
        bool[,] mapUsed = new bool[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (mapUsed[x, y] == false && map[x, y] == tileType)
                {
                    List<Point> newRegion = GetRegionPoints(x, y);
                    regions.Add(newRegion);

                    foreach (Point point in newRegion)
                         mapUsed[point.x, point.y] = true;
                }
            }
        }

        return regions;
    }

    List<Point> GetRegionPoints(int startX, int startY)
    {
        List<Point> points = new List<Point>();
        bool[,] mapUsed = new bool[mapWidth, mapHeight];
        bool tileType = map[startX, startY];

        Queue<Point> queue = new Queue<Point>();
        queue.Enqueue(new Point(startX, startY));
        mapUsed[startX, startY] = true;

        while (queue.Count > 0)
        {
            Point cur = queue.Dequeue();
            points.Add(cur);

            for (int i = cur.x - 1; i <= cur.x + 1; i++)
            {
                for (int j = cur.y - 1; j <= cur.y + 1; j++)
                {
                    if (IsInMap(i, j) && (i == cur.x || j == cur.y))
                    {
                        if (mapUsed[i, j] == false && map[i, j] == tileType)
                        {
                            mapUsed[i, j] = true;
                            queue.Enqueue(new Point(i, j));
                        }
                    }
                }
            }
        }

        return points;
    }

    bool IsInMap (int x, int y)
    {
        return x >= 0 && y >= 0 && x < mapWidth && y < mapHeight;
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

public struct Point
{
    public int x;
    public int y;

    public Point (int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
