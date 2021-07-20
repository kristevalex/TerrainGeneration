using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;

    public void GenerateMesh(bool[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(bool[,] map, float squareSize)
        {

        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;

        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            centreTop = topLeft.right;
            centreLeft = bottomLeft.above;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex;

        public Node(Vector3 _position)
        {
            position = _position;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _position, bool _active, float squareSize) : base(_position)
        {
            active = _active;
            above = new Node(_position + Vector3.up * squareSize);
            right = new Node(_position + Vector3.right * squareSize);
        }
    }
}
