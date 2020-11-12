using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGenerator.autoUpdate)
            {
                mapGenerator.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGenerator.GenerateMap();
        }
    }
}
