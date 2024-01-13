using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.InputSystem.HID.HID;

public struct Particle
{
    public Vector2 position;
    public Vector2 velocity;
    public Vector2 force;
    public int c;
    public int domain;
    public int active;
    // public int numNeighbors
};

public class Simulator : MonoBehaviour
{
    // public static int maxUsedMemory; // needed for in editor profiler
    ChangeOptions changeOptions;
    // ChangeInteractions changeInteractions;
    GameObject changeIntMat;
    ManageMapOutline manageOutline;
    private Mesh _instanceMesh;
    private int circleVertices;
    private float circleRadius;
    [SerializeField] private Material _instanceMaterial;

    private readonly uint[] _args = { 0, 0, 0, 0, 0 };

    private int _particleCount;
    [HideInInspector] public int archiveParticleCount;
    private int _newParticleCount;  // for preparing for buffer resizes
    private int _particlePadding = 2000;  // for avoiding having to resize buffer too frequently
    private int _currentParticleBufferSize;  // for tracking when buffer needs to be resized

    private float R_MAX;
    private float BETA;
    private float FORCE_FACTOR;
    private float FRICTION = Mathf.Pow(0.5f, 0.05f);

    static public ComputeShader _engine;

    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _particlesBuffer;
    private ComputeBuffer _offsetsBuffer;
    private ComputeBuffer _countsBuffer;
    private ComputeBuffer _intmatBuffer;

    private int numDomainsX;
    private int numDomainsY;

    private Particle[] particles;
    private Particle[] sortedParticles;

    private uint[] domains;
    private uint[] counts;
    private uint[] offsets;

    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapHeight;
    private float[] INTERACTION_MATRIX;

    public IEnumerator LateStart(float waitTime) {
        yield return new WaitForSeconds(waitTime);

        UpdateEngineSettings();
    }

    private void Start()
    {
        changeOptions = GetComponent<ChangeOptions>();
        manageOutline = GetComponent<ManageMapOutline>();

        changeIntMat = GameObject.Find("InteractionMatrix");
        ChangeInteractions changeInteractions = changeIntMat.gameObject.GetComponent<ChangeInteractions>();

        INTERACTION_MATRIX = changeInteractions.interactionMatrix;

        R_MAX = changeOptions._rmax;
        BETA = changeOptions._beta;
        FORCE_FACTOR = changeOptions._forceFactor;

        _particleCount = changeOptions._particleCount;
        circleRadius = changeOptions._particleRadius;
        circleVertices = changeOptions._particleVertices;

        mapWidth = changeOptions._mapWidth;
        mapHeight = changeOptions._mapHeight;

        InitBuffers();
        StartCoroutine (LateStart(0.25f));

        manageOutline.UpdateMapOutline(mapWidth, mapHeight, circleRadius);

    }

    // Update is called once per frame
    private void Update()
    {
        // Additional hotkeys
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResetParticlePositions();
        }

        if (_newParticleCount == 0) return; // smort
        UpdateLengthScales(changeOptions._rmax, mapWidth, mapHeight);

        _particlesBuffer.GetData(particles);

        CheckParticleCount();
        MoveParticles();

        Graphics.DrawMeshInstancedIndirect(_instanceMesh, 0, _instanceMaterial, new Bounds(Vector2.zero, Vector3.one * 9999), _argsBuffer);
    }
    void LateUpdate() {
        // add stuff to do with particle information - and they should be in the correct positions.
        // for example track a particle, particles already contain their index in Particles[] within their own array
    }
    
    public void UpdateEngineSettings() {
        changeIntMat = GameObject.Find("InteractionMatrix");
        ChangeInteractions changeInteractions = changeIntMat.gameObject.GetComponent<ChangeInteractions>();
        changeInteractions.ValidateAll();
        INTERACTION_MATRIX = changeInteractions.interactionMatrix;

        changeOptions.UpdateSettings();

        mapWidth = changeOptions._mapWidth;
        mapHeight = changeOptions._mapHeight;
        // this will set R_MAX, mapWidth, and mapHeight
        UpdateLengthScales(changeOptions._rmax, mapWidth, mapHeight); 

        _newParticleCount = changeOptions._particleCount;  // this gets updated in CheckParticleCount()
        circleRadius = changeOptions._particleRadius;
        circleVertices = changeOptions._particleVertices;
        
        manageOutline.UpdateMapOutline(mapWidth, mapHeight, circleRadius);
        UpdateParticleShapes(circleVertices, circleRadius);

        BETA = changeOptions._beta;
        FORCE_FACTOR = changeOptions._forceFactor;

        _newParticleCount = changeOptions._particleCount;  // this gets updated in CheckParticleCount()

        _intmatBuffer.SetData(INTERACTION_MATRIX);

        _engine.SetFloat("BETA", BETA);
        _engine.SetFloat("FORCE_FACTOR", FORCE_FACTOR);
    }

    private void UpdateLengthScales(float newRMax, int newWidth, int newHeight)
    {
        // set RMAX normally if it hasn't changed
        if ((newRMax == R_MAX) && (newWidth == mapWidth) && (newHeight == mapHeight))  {
            numDomainsX = (int)Mathf.Floor(mapWidth / newRMax);
            numDomainsY = (int)Mathf.Floor(mapHeight / newRMax);

            numDomainsX = Math.Min(numDomainsX, 1000);
            numDomainsY = Math.Min(numDomainsY, 1000);

            _engine.SetFloat("R_MAX", R_MAX);

            _engine.SetInt("numDomainsX", numDomainsX);
            _engine.SetInt("numDomainsY", numDomainsY);

            _engine.SetFloat("mapWidth", mapWidth);
            _engine.SetFloat("mapHeight", mapHeight);

            _offsetsBuffer?.Release();
            // +1 for prefix sum padding, +1 for dummy domain for inactive particles
            _offsetsBuffer = new ComputeBuffer(numDomainsX * numDomainsY + 1 + 1, sizeof(uint));

            _countsBuffer?.Release();
            _countsBuffer = new ComputeBuffer(numDomainsX * numDomainsY + 1 + 1, sizeof(uint));

            offsets = new uint[numDomainsX * numDomainsY + 2];
            counts = new uint[numDomainsX * numDomainsY + 2];

            for (int i = 0; i < numDomainsX * numDomainsY + 2; i++)
            {
                offsets[i] = 0;
                counts[i] = 0;
            }
            _offsetsBuffer.SetData(offsets);
            _countsBuffer.SetData(counts);

            _engine.SetBuffer(_engine.FindKernel("move_particles_batched"), "offsets", _offsetsBuffer);
            return;
        }
        // if _anything_ about the dimensions or R_MAX changed, you need to update the domain shapes

        R_MAX = newRMax;
        mapWidth = newWidth;
        mapHeight = newHeight;

        numDomainsX = (int)Mathf.Floor(mapWidth / newRMax);
        numDomainsY = (int)Mathf.Floor(mapHeight / newRMax);

        numDomainsX = Math.Min(numDomainsX, 1000);
        numDomainsY = Math.Min(numDomainsY, 1000);

        _engine.SetFloat("R_MAX", R_MAX);

        _engine.SetInt("numDomainsX", numDomainsX);
        _engine.SetInt("numDomainsY", numDomainsY);

        _engine.SetFloat("mapWidth", mapWidth);
        _engine.SetFloat("mapHeight", mapHeight);

        _offsetsBuffer?.Release();
        // +1 for prefix sum padding, +1 for dummy domain for inactive particles
        _offsetsBuffer = new ComputeBuffer(numDomainsX * numDomainsY + 1 + 1, sizeof(uint));

        _countsBuffer?.Release();
        _countsBuffer = new ComputeBuffer(numDomainsX * numDomainsY + 1 + 1, sizeof(uint));

        offsets = new uint[numDomainsX * numDomainsY + 2];
        counts = new uint[numDomainsX * numDomainsY + 2];

        for (int i = 0; i < numDomainsX * numDomainsY + 2; i++)
        {
            offsets[i] = 0;
            counts[i] = 0;
        }
        _offsetsBuffer.SetData(offsets);
        _countsBuffer.SetData(counts);

        _engine.SetBuffer(_engine.FindKernel("move_particles_batched"), "offsets", _offsetsBuffer);
    }

    private void UpdateParticleShapes(int vertices, float radius) {
        _instanceMesh = GetCircleMesh(vertices, radius);
    }

    public void ResetParticlePositions()
    {
        particles = new Particle[_currentParticleBufferSize];
        sortedParticles = new Particle[_currentParticleBufferSize];
        domains = new uint[_currentParticleBufferSize];

        for (int i = 0; i < _currentParticleBufferSize; i++)
        {
            if (i < _particleCount)
            {
                particles[i].position = new Vector2(UnityEngine.Random.Range(0f, 1f) * mapWidth, UnityEngine.Random.Range(0f, 1f) * mapHeight);
                particles[i].velocity = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                particles[i].force = new Vector2(0.0f, 0.0f);
                particles[i].c = UnityEngine.Random.Range(0, 6); // assign initial values - to zero
                particles[i].active = 1;
            }
            else
            {
                particles[i].position = new Vector2(0f, 0f);
                particles[i].velocity = new Vector2(0f, 0f);
                particles[i].force = new Vector2(0f, 0f);
                particles[i].c = -1;
                particles[i].active = 0;
            }

            sortedParticles[i].position = new Vector2(0, 0);
            sortedParticles[i].velocity = new Vector2(0, 0);
            sortedParticles[i].force = new Vector2(0.0f, 0.0f);
            sortedParticles[i].c = 0; // assign initial values - to zero 
            sortedParticles[i].active = 0;

            domains[i] = 0;
        }

        _particlesBuffer.SetData(particles);
    }

    private void InitBuffers()
    {
        _instanceMesh = GetCircleMesh(circleVertices, circleRadius);
        // reset buffers
        _currentParticleBufferSize = _particleCount + _particlePadding;

        _particlesBuffer?.Release();
        _particlesBuffer = new ComputeBuffer(_currentParticleBufferSize, sizeof(float) * 9); // change to 9 b/c adding a new float (must specify the exact size of buffer)

        _intmatBuffer?.Release();
        _intmatBuffer = new ComputeBuffer(36, sizeof(float));

        ResetParticlePositions();

        _intmatBuffer.SetData(INTERACTION_MATRIX);

        // Compute shader
        _engine = Resources.Load<ComputeShader>("Engine");

        _engine.SetInt("_particleCount", _particleCount);
        _engine.SetFloat("BETA", BETA);
        
        _engine.SetFloat("FORCE_FACTOR", FORCE_FACTOR);
        _engine.SetFloat("FRICTION", FRICTION);

        _engine.SetBuffer(_engine.FindKernel("move_particles_batched"), "interactionMatrix", _intmatBuffer);
        _engine.SetBuffer(_engine.FindKernel("move_particles_batched"), "particles", _particlesBuffer);

        // Rendering shader
        _instanceMaterial.SetBuffer("particles", _particlesBuffer);

        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        _args[0] = _instanceMesh.GetIndexCount(0);  // index count per instance
        _args[1] = (uint)_particleCount;            // instance count
        _args[2] = _instanceMesh.GetIndexStart(0);  // start index location
        _args[3] = _instanceMesh.GetBaseVertex(0);  // base vertex location
                                                    // start instance location

        _argsBuffer.SetData(_args);
    }

    private void CheckParticleCount()
    {
        int newBufferSize = _newParticleCount + _particlePadding;

        int numDiff = _newParticleCount - _particleCount;

        if (numDiff < 0)  // decreasing number of particles
        {
            // when removing particles, you should remove _then_ resize

            // randomly remove particles
            int numRemoved = 0;
            int _i = 0;

            while (numRemoved < Math.Abs(numDiff))
            {
                if (particles[_i].active == 1)
                {
                    float r = UnityEngine.Random.Range(0f, 1f);

                    // 10% chance of removal; this threshold was chosen arbitrarily,
                    // and may result in skewed removal; however, it seemed better than
                    // something like r < abs(numDiff) / _particleCount, which could take awhile
                    if (r < 0.1)
                    {
                        particles[_i].active = 0;
                        numRemoved++;
                    }
                }

                _i = (_i + 1) % _particleCount;
            }

            // check if buffer size should be decreased
            if (Math.Abs(numDiff) > _particlePadding)
            {
                // copy temporarily into sortedParticles to avoid any issues of possibly
                // losing particles if numDiff > _particlePadding
                int numCopied = 0;
                for (int i = 0; i < _particleCount; i++)
                {
                    if (particles[i].active == 1)
                    {
                        sortedParticles[numCopied] = particles[i];
                        numCopied++;
                    }
                }

                // now resize arrays and buffers, then copy data back
                _currentParticleBufferSize = newBufferSize;

                domains = new uint[_currentParticleBufferSize];
                particles = new Particle[_currentParticleBufferSize];

                _particleCount = _newParticleCount;

                for (int i = 0; i < _particleCount; i++)
                {
                    particles[i] = sortedParticles[i];
                }

                sortedParticles = new Particle[_currentParticleBufferSize];

                _particlesBuffer?.Release();
                _particlesBuffer = new ComputeBuffer(_currentParticleBufferSize, sizeof(float) * 9); // change to 9 b/c adding a new float (must specify the exact size of buffer)

                _instanceMaterial.SetBuffer("particles", _particlesBuffer);

                _engine.SetBuffer(_engine.FindKernel("move_particles_batched"), "particles", _particlesBuffer);
            }
        }
        else if (numDiff > 0)  // increasing number of particles
        {
            // when adding particles, you should resize _then_ add

            // check if buffer size should be increased
            if (Math.Abs(numDiff) > _particlePadding)
            {
                // the logic here is pretty much the same as above;
                // use sortedParticles to store data while you resize

                int numAdded = 0;
                for (int i = 0; i < _currentParticleBufferSize; i++)
                {
                    if (particles[i].active == 1)
                    {
                        sortedParticles[numAdded] = particles[i];
                        numAdded++;
                    }
                }

                _currentParticleBufferSize = newBufferSize;

                domains = new uint[_currentParticleBufferSize];
                particles = new Particle[_currentParticleBufferSize];

                for (int i = 0; i < _particleCount; i++)
                {
                    particles[i] = sortedParticles[i];
                }

                sortedParticles = new Particle[_currentParticleBufferSize];

                _particlesBuffer?.Release();
                _particlesBuffer = new ComputeBuffer(_currentParticleBufferSize, sizeof(float) * 9); // change to 9 b/c adding a new float (must specify the exact size of buffer)

                _instanceMaterial.SetBuffer("particles", _particlesBuffer);
                _engine.SetBuffer(_engine.FindKernel("move_particles_batched"), "particles", _particlesBuffer);
            }

            for (int i = _particleCount; i < _particleCount + numDiff; i++)
            {
                particles[i].position = new Vector2(UnityEngine.Random.Range(0f, 1f) * mapWidth, UnityEngine.Random.Range(0f, 1f) * mapHeight);
                particles[i].velocity = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                particles[i].force = new Vector2(0.0f, 0.0f);
                particles[i].c = UnityEngine.Random.Range(0, 6); // assign initial values - to zero
                particles[i].active = 1;
            }

            _particleCount = _newParticleCount;
        }
        else
        {  // particle count didn't change
            return;
        }

        // update shader variables
        _engine.SetInt("_particleCount", _particleCount);

        _args[1] = (uint)_particleCount;
        _argsBuffer.SetData(_args);
    }

    private void MoveParticles()
    {
        sortParticles();
        _engine.Dispatch(_engine.FindKernel("move_particles_batched"), (int)Mathf.Ceil(_particleCount/128.0f), 1, 1);
    }

    private Mesh GetCircleMesh(int numVertices, float radius)
    {
        Mesh planeMesh = new Mesh();

        planeMesh.vertices = GetCircumferencePoints(numVertices, radius).ToArray();
        planeMesh.triangles = DrawFilledTriangles(planeMesh.vertices);

        planeMesh.RecalculateNormals();

        return planeMesh;
    }

    List<Vector3> GetCircumferencePoints(int sides, float radius)
    {
        List<Vector3> points = new List<Vector3>();

        float circumferenceProgressPerStep = (float)1 / sides;
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

    void sortParticles()
    {
        // clear
        for (int i = 0; i < numDomainsX * numDomainsY + 1; i++)
        {
            counts[i] = 0;
        }

        // compute domains
        for (int i = 0; i < _currentParticleBufferSize; i++)
        {
            // modulo to avoid edge case where x or y position = 1.0
            int dx = (int)Mathf.Floor(particles[i].position.x / R_MAX) % numDomainsX;
            int dy = (int)Mathf.Floor(particles[i].position.y / R_MAX) % numDomainsY;

            if (particles[i].active == 1)
            {
                domains[i] = (uint)(dy * numDomainsX + dx + 1);
            }
            else
            {
                // put inactive particles in an out-of-bounds domain for sorting
                // this is necessary so that Dispatch calls only need _particleCount threads
                domains[i] = (uint)(numDomainsX * numDomainsY + 1);
            }

            counts[domains[i]]++;
        }

        // prefix sum
        for (int i = 0; i < numDomainsX * numDomainsY; i++)
        {
            // each bin now contains the number of particles contained in
            // all preceding bins, PLUS its own count
            counts[i + 1] += counts[i];
        }

        // copy
        for (int i = 0; i < numDomainsX * numDomainsY + 1; i++)
        {
            offsets[i] = counts[i];
        }

        _offsetsBuffer.SetData(offsets);

        // sort
        for (int i = _currentParticleBufferSize - 1; i >= 0; i--)
        {
            if (particles[i].active == 0) continue;

            sortedParticles[counts[domains[i]] - 1] = particles[i];
            counts[domains[i]]--;
        }

        for (int i = 0; i < _currentParticleBufferSize; i++)
        {
            particles[i] = sortedParticles[i];
        }

        _particlesBuffer.SetData(particles);
    }

    void OnDestroy()
    {   // we need to explicitly release the buffers, otherwise Unity will not be satisfied
        _argsBuffer?.Release();
        _argsBuffer = null;

        _particlesBuffer?.Release();
        _particlesBuffer = null;

        _offsetsBuffer?.Release();
        _offsetsBuffer = null;

        _countsBuffer?.Release();
        _countsBuffer = null;

        _intmatBuffer?.Release();
        _intmatBuffer = null;
    }
}
