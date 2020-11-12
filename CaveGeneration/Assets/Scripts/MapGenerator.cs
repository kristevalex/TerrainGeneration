using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapHeight;
    public int mapWidth;
    public float noiseScale;
    public bool autoUpdate;
    public float fillAmount;
    
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        
        Color[] cavesMap = new Color[mapHeight * mapWidth];
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float curHeight = noiseMap[x, y];
                
                if (curHeight < fillAmount)
                    cavesMap[y * mapWidth + x] = Color.black;
                else
                    cavesMap[y * mapWidth + x] = Color.white;
            }
        }

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        
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
