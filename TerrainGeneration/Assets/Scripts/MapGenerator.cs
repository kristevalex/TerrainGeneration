using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapHeight;
    public int mapWidth;
    public float noiseScale;
    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }
}
