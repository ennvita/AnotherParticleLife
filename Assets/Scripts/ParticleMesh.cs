using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ParticleMesh : MonoBehaviour
{
    [Range(3, 100)][SerializeField] private int numVertices = 10;
    [Range(1,  10)][SerializeField] private float radius = 1;

    private MeshFilter filter;
    private MeshRenderer meshRenderer;

    //private Vector3[] vertices;
    //private int[] triangles;

    private int currentNumVertices;
    private float currentRadius;

    // Start is called before the first frame update
    void Start()
    {
        GenerateInitialMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (numVertices != currentNumVertices || radius != currentRadius)
        {
            UpdatePlaneMesh(filter, numVertices, radius);
            currentNumVertices = numVertices;
            currentRadius = radius;
        }
    }

    private void OnValidate()
    {
        if (!filter || !meshRenderer)
        {
            GenerateInitialMesh();
        }

        if (numVertices != currentNumVertices || radius != currentRadius)
        {
            UpdatePlaneMesh(filter, numVertices, radius);
            currentNumVertices = numVertices;
            currentRadius = radius;
        }
    }

    private void GenerateInitialMesh()
    {
        this.name = "Particle";

        filter = this.GetComponent<MeshFilter>();
        meshRenderer = this.GetComponent<MeshRenderer>();

        GeneratePlaneMesh(meshRenderer, filter, numVertices, radius);
    }

    private Mesh GeneratePlaneMesh(MeshRenderer renderer, MeshFilter filter, int numVertices, float radius)
    {
        Shader shader = Resources.Load<Shader>("ParticleShaderGraph");
        if (shader != null)
        {
            renderer.sharedMaterial = new Material(shader);
        }
        else
        {
            print("Unable to find ParticleShaderGraph");
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }

        Mesh planeMesh = UpdatePlaneMesh(filter, numVertices, radius);

        return planeMesh;
    }

    private Mesh UpdatePlaneMesh(MeshFilter filter, int numVertices, float radius)
    {
        Mesh planeMesh = new Mesh();

        planeMesh.vertices = GetCircumferencePoints(numVertices, radius).ToArray();
        planeMesh.triangles = DrawFilledTriangles(planeMesh.vertices);

        planeMesh.RecalculateNormals();

        filter.mesh = planeMesh;

        return planeMesh;
    }

    //void DrawFilled(Mesh mesh, int sides, float radius)
    //{
    //    vertices  = GetCircumferencePoints(numVertices, radius).ToArray();
    //    triangles = DrawFilledTriangles(vertices);

    //    mesh.Clear();
    //    mesh.vertices  = vertices;
    //    mesh.triangles = triangles;
    //}

    List<Vector3> GetCircumferencePoints(int sides, float radius)
    {
        List<Vector3> points = new List<Vector3>();

        float circumferenceProgressPerStep = (float) 1 / sides;
        float TAU = 2 * Mathf.PI;
        float radianProgressPerStep = circumferenceProgressPerStep * TAU;

        for (int i = 0; i < sides; i++)
        {
            float currentRadian = radianProgressPerStep * i;
            points.Add(new Vector3(Mathf.Cos(currentRadian) * radius, Mathf.Sin(currentRadian) * radius, 0));
        }

        return points;
    }
    int[] DrawFilledTriangles(Vector3[] points)
    {
        int triangleAmount = points.Length - 2;
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < triangleAmount; i++)
        {
            newTriangles.Add(0);
            newTriangles.Add(i + 2);
            newTriangles.Add(i + 1);
        }

        return newTriangles.ToArray();
    }
}
