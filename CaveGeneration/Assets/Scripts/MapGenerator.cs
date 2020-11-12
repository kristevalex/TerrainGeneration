﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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
    int edgeDistSmoothing;
    [SerializeField]
    int smoothIterations;

    [SerializeField]
    int wallThresholdSize;
    [SerializeField]
    int caveThresholdSize;
        
    bool[,] map;

    MapDisplay mapDisplay;

    public static List<Point> test;


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
        if (!mapDisplay)
            mapDisplay = FindObjectOfType<MapDisplay>();


        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity);

        map = new bool[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[x, y] = noiseMap[x, y] < fillAmount;

                for (int i = 0; i < edgeDistSmoothing; i++)
                {
                    if (y == i || x == i || y == mapHeight - i - 1 || x == mapWidth - i - 1)
                        if (prng.Next(0, edgeDistSmoothing) >= i)
                            map[x, y] = false;
                }
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
        Color[] cavesMap = new Color[mapHeight * mapWidth];
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (cavesMap[y * mapWidth + x] == Color.red)
                    continue;

                if (test.Contains(new Point(x, y)))
                {
                    for (int i = x - 1; i <= x + 1; i++)
                    {
                        for (int j = y - 1; j <= y + 1; j++)
                        {
                            cavesMap[j * mapWidth + i] = Color.red;
                        }
                    }
                    cavesMap[y * mapWidth + x] = Color.green;
                }
                else if (map[x, y])
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
        List<Room> rooms = new List<Room>();
        foreach (List<Point> caveRegion in caveRegions)
        {
            if (caveRegion.Count < caveThresholdSize)
            {
                foreach (Point cavePoint in caveRegion)
                {
                    map[cavePoint.x, cavePoint.y] = false;
                }
            }
            else
            {
                rooms.Add(new Room(caveRegion, map));
            }
        }

        ConnectRooms(rooms);
    }

    void ConnectRooms(List<Room> rooms)
    {
        test = new List<Point>();

        int[] addedIds = new int[rooms.Count];
        int added = 0;
        int addedId = 1;

        Edge[] edges = new Edge[rooms.Count * (rooms.Count - 1) / 2];
        int cnt = 0;
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < i; j++)
            {
                edges[cnt] = new Edge(GetDist(rooms[i], rooms[j]), i, j);
                ++cnt;
            }
        }

        Array.Sort(edges);
        for (int i = 0; i < edges.Length; i++)
        {
            if (added == rooms.Count)
                break;

            if (addedIds[edges[i].to] != addedIds[edges[i].from])
            {
                Debug.Log("connecting: " + edges[i].to + " " + edges[i].to);

                int oldVal = addedIds[edges[i].to];
                if (addedIds[edges[i].to] == 0)
                {
                    addedIds[edges[i].to] = addedIds[edges[i].from];
                }
                if (addedIds[edges[i].from] == 0)
                {
                    addedIds[edges[i].from] = addedIds[edges[i].to];
                }
                else
                {
                    for (int j = 0; j < rooms.Count; j++)
                    {
                        if (addedIds[j] == oldVal)
                        {
                            addedIds[j] = addedIds[edges[i].from];
                        }
                    }
                }

                added++;
                CreateConnection(rooms[edges[i].to], rooms[edges[i].from]);
            }
            else if (addedIds[edges[i].to] == 0)
            {
                addedIds[edges[i].to] = addedId;
                addedIds[edges[i].from] = addedId;
                added++;
                addedId++;

                Debug.Log("connecting: " + edges[i].to + " " + edges[i].to);

                CreateConnection(rooms[edges[i].to], rooms[edges[i].from]);
            }
        }
    }

    int GetDist(Room first, Room second)
    {
        int bestDist = int.MaxValue;
        foreach (Point firstTile in first.edgePoints)
        {
            foreach (Point secondTile in second.edgePoints)
            {
                int curDist = (firstTile.x - secondTile.x) * (firstTile.x - secondTile.x) + (firstTile.y - secondTile.y) * (firstTile.y - secondTile.y);
                if (curDist < bestDist)
                    bestDist = curDist;
            }
        }

        return bestDist;
    }

    void CreateConnection(Room first, Room second)
    {
        Room.Connect(first, second);

        Point firstBest = new Point();
        Point secondBest = new Point();

        int bestDist = int.MaxValue;
        foreach (Point firstTile in first.edgePoints)
        {
            foreach (Point secondTile in second.edgePoints)
            {
                int curDist = (firstTile.x - secondTile.x) * (firstTile.x - secondTile.x) + (firstTile.y - secondTile.y) * (firstTile.y - secondTile.y);
                if (curDist < bestDist)
                {
                    bestDist = curDist;
                    firstBest = firstTile;
                    secondBest = secondTile;
                }
            }
        }

        test.Add(firstBest);
        test.Add(secondBest);        
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

        if (edgeDistSmoothing < 1)
            edgeDistSmoothing = 1;
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

public class Room
{
    public List<Point> points;
    public List<Point> edgePoints;
    public List<Room> connected;

    public Room(List<Point> _points, bool[,] map)
    {
        points = _points;
        connected = new List<Room>();
        edgePoints = new List<Point>();
        foreach (Point point in points)
        {
            for (int i = point.x - 1; i <= point.x + 1; i++)
            {
                for (int j = point.y - 1; j <= point.y + 1; j++)
                {
                    if (i == point.x || j == point.y)
                    {
                        if (map[i, j] == false)
                        {
                            edgePoints.Add(point);
                        }
                    }
                }
            }
        }
    }

    public static void Connect(Room first, Room second)
    {
        first.connected.Add(second);
        second.connected.Add(first);
    }

    public bool IsConnected(Room other)
    {
        return connected.Contains(other);
    }
}

public struct Edge : IComparable
{
    public int dist;

    public int from;
    public int to;

    public Edge (int _dist, int _from, int _to)
    {
        dist = _dist;
        from = _from;
        to = _to;
    }

    int IComparable.CompareTo(object other)
    {
        Edge edge = (Edge)other;

        if (dist == edge.dist) return 0;
        if (dist < edge.dist) return -1;
        if (dist > edge.dist) return 1;
        return 0;
    }
}

