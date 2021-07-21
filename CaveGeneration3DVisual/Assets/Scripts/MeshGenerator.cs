using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
	public SquareGrid squareGrid;

	[SerializeField]
	MeshFilter walls;

	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();

	Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
	List<List<int>> outlines = new List<List<int>>();
	HashSet<int> checkedVerices = new HashSet<int>();

	public void GenerateMesh(bool[,] map, float squareSize)
	{
		vertices.Clear();
		triangles.Clear();
		triangleDictionary.Clear();
		outlines.Clear();
		checkedVerices.Clear();

		squareGrid = new SquareGrid(map, squareSize);
		for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
		{
			for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
			{
				TriangulateSquare(squareGrid.squares[x, y]);
			}
		}

		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		CreateWallMesh(5.0f);
	}

	void CreateWallMesh(float wallHeight)
    {
		CalculateMeshOutlines();

		List<Vector3> wallVertices = new List<Vector3>();
		List<int> wallTriangles = new List<int>();
		Mesh wallMesh = new Mesh();

		foreach(List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
				int startIndex = wallVertices.Count;
				wallVertices.Add(vertices[outline[i]]);										// left
				wallVertices.Add(vertices[outline[i + 1]]);									// right
				wallVertices.Add(vertices[outline[i]] + Vector3.forward * wallHeight);		// bottom left
				wallVertices.Add(vertices[outline[i + 1]] + Vector3.forward * wallHeight);	// bottom right

				wallTriangles.Add(startIndex + 0);
				wallTriangles.Add(startIndex + 2);
				wallTriangles.Add(startIndex + 3);

				wallTriangles.Add(startIndex + 3);
				wallTriangles.Add(startIndex + 1);
				wallTriangles.Add(startIndex + 0);
			}
        }

		wallMesh.vertices = wallVertices.ToArray();
		wallMesh.triangles = wallTriangles.ToArray();
		walls.mesh = wallMesh;
    }

	void TriangulateSquare(Square square)
	{
		switch (square.configuration)
		{
			case 0:
				break;

			// 1 points:
			case 1:
				MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
				break;
			case 2:
				MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
				break;
			case 4:
				MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
				break;
			case 8:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
				break;

			// 2 points:
			case 3:
				MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
				break;
			case 6:
				MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
				break;
			case 9:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
				break;
			case 12:
				MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
				break;
			case 5:
				MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
				break;
			case 10:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
				break;

			// 3 point:
			case 7:
				MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
				break;
			case 11:
				MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
				break;
			case 13:
				MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
				break;
			case 14:
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
				break;

			// 4 point:
			case 15:
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
				checkedVerices.Add(square.topLeft.vertexIndex);
				checkedVerices.Add(square.topRight.vertexIndex);
				checkedVerices.Add(square.bottomRight.vertexIndex);
				checkedVerices.Add(square.bottomLeft.vertexIndex);
				break;
		}
    }

	void MeshFromPoints(params Node[] points)
	{
		AssignVertices(points);

        for (int i = 2; i < points.Length; i++)
        {
            CreateTriangle(points[0], points[i-1], points[i]);
        }
	}

	void AssignVertices(Node[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].vertexIndex == -1)
			{
				points[i].vertexIndex = vertices.Count;
				vertices.Add(points[i].position);
			}
		}
	}

	void CreateTriangle(Node a, Node b, Node c)
	{
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);

		Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTriangleToDictionary(triangle.vertexIndexA, triangle);
		AddTriangleToDictionary(triangle.vertexIndexB, triangle);
		AddTriangleToDictionary(triangle.vertexIndexC, triangle);
	}

	void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
		if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
			triangleDictionary[vertexIndexKey].Add(triangle);
        }
		else
        {
            List<Triangle> triangleList = new List<Triangle>
            {
                triangle
            };
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

	void CalculateMeshOutlines()
    {
        for (int verexIndex = 0; verexIndex < vertices.Count; verexIndex++)
        {
			if (!checkedVerices.Contains(verexIndex))
            {
				int newOutlineVertex = GetConnectedOutlineVertex(verexIndex);
				if (newOutlineVertex != -1)
                {
					checkedVerices.Add(verexIndex);

                    List<int> newOutline = new List<int>{verexIndex};
                    outlines.Add(newOutline);
					FollowOutline(newOutlineVertex, outlines.Count - 1);
					outlines[outlines.Count - 1].Add(verexIndex);
                }
            }
        }
    }

	void FollowOutline(int vertexIndex, int outlineIndex)
    {
		outlines[outlineIndex].Add(vertexIndex);
		checkedVerices.Add(vertexIndex);
		int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

		if (nextVertexIndex != -1)
        {
			FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

	int GetConnectedOutlineVertex(int vertexIndex)
    {
		List<Triangle> trianglesContaningVertex = triangleDictionary[vertexIndex];

		foreach(Triangle triangleContaningVertex in trianglesContaningVertex)
            for (int i = 0; i < 3; i++)
				if (!checkedVerices.Contains(triangleContaningVertex[i]))
					if (IsOutlineEdge(vertexIndex, triangleContaningVertex[i]))
						return triangleContaningVertex[i];

		return -1;
    }

	bool IsOutlineEdge(int vertexA, int vertexB)
    {
		if (vertexA == vertexB)
			return false;

		List<Triangle> trianglesContainingA = triangleDictionary[vertexA];
		int sharedTrianglesCount = 0;

        foreach (Triangle triangleContainingA in trianglesContainingA)
			if (triangleContainingA.Contains(vertexB))
				sharedTrianglesCount++;

		return sharedTrianglesCount < 2;
    }

	struct Triangle
    {
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;

		public Triangle(int a, int b, int c)
        {
			vertexIndexA = a;
			vertexIndexB = b;
			vertexIndexC = c;
        }

		public int this[int i]
        {
			get
            {
				if (i == 0)
					return vertexIndexA;
				else if (i == 1)
					return vertexIndexB;
				else
					return vertexIndexC;
            }
        }

		public bool Contains(int verexIndex)
        {
			return verexIndex == vertexIndexA || verexIndex == vertexIndexB || verexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(bool[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, -mapHeight / 2 + y * squareSize + squareSize / 2, 0);
                    controlNodes[x, y] = new ControlNode(pos, !map[x, y], squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;

			centreTop = topLeft.right;
			centreRight = bottomRight.above;
			centreBottom = bottomLeft.right;
			centreLeft = bottomLeft.above;

			if (topLeft.active)
				configuration += 8;
			if (topRight.active)
				configuration += 4;
			if (bottomRight.active)
				configuration += 2;
			if (bottomLeft.active)
				configuration += 1;
		}
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex;

        public Node(Vector3 _position)
        {
            position = _position;
            vertexIndex = -1;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _position, bool _active, float squareSize) : base(_position)
        {
            active = _active;
            above = new Node(_position + Vector3.up * squareSize / 2f);
            right = new Node(_position + Vector3.right * squareSize / 2f);
        }
    }
}
