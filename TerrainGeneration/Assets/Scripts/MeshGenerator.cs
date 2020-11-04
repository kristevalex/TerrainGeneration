using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerreinMesh(float[,] heightMap, float heightMult, AnimationCurve animationCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        MeshData meshData = new MeshData(width, height);
        int vertexID = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.vertices[vertexID] = new Vector3(topLeftX + x, animationCurve.Evaluate(heightMap[x, y]) * heightMult, topLeftZ -  y);
                meshData.uvs[vertexID] = new Vector2(x / (float)width, y / (float)height);

                if (y != height - 1 && x != width - 1)
                {
                    meshData.AddTriangle(vertexID, vertexID + width + 1, vertexID + width);
                    meshData.AddTriangle(vertexID + width + 1, vertexID, vertexID + 1);
                }

                ++vertexID;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int trianglesEnd = 0;

    public MeshData (int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshHeight * meshWidth];
        uvs = new Vector2[meshHeight * meshWidth];
        triangles = new int[6 * (meshHeight - 1) * (meshWidth - 1)];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[trianglesEnd] = a;
        triangles[trianglesEnd + 1] = b;
        triangles[trianglesEnd + 2] = c;
        trianglesEnd += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}