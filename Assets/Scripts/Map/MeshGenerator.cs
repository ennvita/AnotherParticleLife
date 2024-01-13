using System.Collections;
using System;
using UnityEngine;
using JetBrains.Annotations;
using Unity.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;

    public void GenerateMesh(int[,] map, float squareSize) {
        squareGrid = new SquareGrid(map, squareSize);
        vertices = new List<Vector3>();
        triangles = new List<int>();
            for (int i = 0; i < squareGrid.squares.GetLength(0); i++) {
                for (int j = 0; j < squareGrid.squares.GetLength(1); j++) {
                    TriangulateSquare(squareGrid.squares[i,j]);
                }
            }
            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
    }
    void TriangulateSquare(Square square) {
        switch (square.configuration) {
		case 0:
			break;

		// 1 points:
		case 1:
			MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
			break;
		case 2:
			MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
			break;
		case 4:
			MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
			break;
		case 8:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
			break;

		// 2 points:
		case 3:
			MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
			break;
		case 6:
			MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
			break;
		case 9:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
			break;
		case 12:
			MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
			break;
		case 5:
			MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
			break;
		case 10:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
			break;

		// 3 point:
		case 7:
			MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
			break;
		case 11:
			MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
			break;
		case 13:
			MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
			break;
		case 14:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
			break;

		// 4 point:
		case 15:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			break;
		}
    }
    void MeshFromPoints(params Node[] points) {
        AssignedVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
        
    }
    void AssignedVertices(Node[] points) {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }
    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

    }
    void OnDrawGizmos() {
        // if (squareGrid != null) {
        //     for (int i = 0; i < squareGrid.squares.GetLength(0); i++) {
        //         for (int j = 0; j < squareGrid.squares.GetLength(1); j++) {
        //             Gizmos.color = (squareGrid.squares[i,j].topLeft.active)?Color.black:Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[i,j].topLeft.position, Vector3.one * .4f);
        
        //             Gizmos.color = (squareGrid.squares[i,j].topRight.active)?Color.black:Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[i,j].topRight.position, Vector3.one * .4f);

        //             Gizmos.color = (squareGrid.squares[i,j].bottomRight.active)?Color.black:Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[i,j].bottomRight.position, Vector3.one * .4f);

        //             Gizmos.color = (squareGrid.squares[i,j].bottomLeft.active)?Color.black:Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[i,j].bottomLeft.position, Vector3.one * .4f);
                    
        //             Gizmos.color = Color.grey;
        //             Gizmos.DrawCube(squareGrid.squares[i,j].centerTop.position, Vector3.one * .1f);
        //             Gizmos.DrawCube(squareGrid.squares[i,j].centerRight.position, Vector3.one * .1f);
        //             Gizmos.DrawCube(squareGrid.squares[i,j].centerBottom.position, Vector3.one * .1f);
        //             Gizmos.DrawCube(squareGrid.squares[i,j].centerLeft.position, Vector3.one * .1f);
        //         }
        //     }
        // }
    }

    public class SquareGrid {
        public Square[,] squares;
        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;
            
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int i = 0; i < nodeCountX; i++) {
                for (int j = 0; j < nodeCountY; j++) {
                    Vector3 pos = new Vector3(-mapWidth/2 + i * squareSize + squareSize/2, -mapHeight/2 + j *squareSize + squareSize/2);
                    controlNodes[i,j] = new ControlNode(pos,map[i,j] == 1, squareSize);
                }
            }
            squares = new Square[nodeCountX -1, nodeCountY -1];
            for (int i = 0; i < nodeCountX-1; i++) {
                for (int j = 0; j < nodeCountY-1; j++) {
                    squares[i,j] = new Square(controlNodes[i,j+1], controlNodes[i+1,j+1], controlNodes[i+1,j], controlNodes[i,j]);
                }
            }
        }
    }
    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration;
        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;

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
    public class Node {
        public Vector3 position;
        public int vertexIndex = -1;
        public Node(Vector3 _pos) {
            position = _pos;
        }
    }
    public class ControlNode : Node {
        public bool active;
        public Node above, right;
        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize/2f);
            right = new Node(position + Vector3.right * squareSize/2f);
        }
    }
}