using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and stores the cloth particles and constraints.
/// Also owns the Unity Mesh used for visualization.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ClothSystem : MonoBehaviour
{
    [Header("Cloth dimensions")]
    public int particlesX = 30;
    public int particlesY = 30;

    public float width = 3.0f;
    public float height = 3.0f;

    [Header("Particle properties")]
    public float totalMass = 0.3f;

    [Header("Initial orientation")]
    public Vector3 initialPosition = Vector3.zero;

    [Header("Boundary conditions")]
    public bool fixTopLeft = true;
    public bool fixTopRight = true;

    public ClothParticle[] particles;
    public List<ClothConstraint> constraints;

    public Mesh mesh;

    private MeshFilter meshFilter;

    private int vertexCount;

    public int ParticleCount => particlesX * particlesY;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        meshFilter = GetComponent<MeshFilter>();

        CreateParticles();
        CreateConstraints();
        CreateMesh();

        UpdateMesh();
    }

    // ============================================================
    // PARTICLES
    // ============================================================

    private void CreateParticles()
    {
        particles = new ClothParticle[ParticleCount];

        float dx =
            width / (particlesX - 1);

        float dy =
            height / (particlesY - 1);

        float particleMass =
            totalMass / ParticleCount;

        for (int j = 0; j < particlesY; j++)
        {
            for (int i = 0; i < particlesX; i++)
            {
                int index =
                    GetIndex(i, j);

                // Cloth initially lies in the XY plane.
                Vector3 position =
                    initialPosition +
                    new Vector3(
                        i * dx,
                        0.0f,
                        -j * dy);

                bool fixedParticle = false;

                if (j == 0)
                {
                    if (i == 0 && fixTopLeft)
                        fixedParticle = true;

                    if (i == particlesX - 1 &&
                        fixTopRight)
                        fixedParticle = true;
                }

                particles[index] =
                    new ClothParticle(
                        position,
                        particleMass,
                        fixedParticle);
            }
        }
    }

    // ============================================================
    // CONSTRAINTS
    // ============================================================

    private void CreateConstraints()
    {
        constraints =
            new List<ClothConstraint>();

        for (int j = 0; j < particlesY; j++)
        {
            for (int i = 0; i < particlesX; i++)
            {
                int current =
                    GetIndex(i, j);

                // Horizontal structural constraint
                if (i < particlesX - 1)
                {
                    int right =
                        GetIndex(i + 1, j);

                    AddConstraint(
                        particles[current],
                        particles[right]);
                }

                // Vertical structural constraint
                if (j < particlesY - 1)
                {
                    int below =
                        GetIndex(i, j + 1);

                    AddConstraint(
                        particles[current],
                        particles[below]);
                }
            }
        }
    }

    private void AddConstraint(
        ClothParticle a,
        ClothParticle b)
    {
        constraints.Add(
            new ClothConstraint(a, b));
    }

    // ============================================================
    // MESH
    // ============================================================

    private void CreateMesh()
    {
        mesh = new Mesh();

        mesh.name = "PBD Cloth Mesh";

        // A 30x30 particle grid has 900 vertices.
        // Use 32-bit indices to be safe for larger future grids.
        mesh.indexFormat =
            UnityEngine.Rendering.IndexFormat.UInt32;

        vertexCount =
            ParticleCount;

        Vector3[] vertices =
            new Vector3[vertexCount];

        Vector2[] uv =
            new Vector2[vertexCount];

        for (int j = 0; j < particlesY; j++)
        {
            for (int i = 0; i < particlesX; i++)
            {
                int index =
                    GetIndex(i, j);

                vertices[index] =
                    transform.InverseTransformPoint(
                        particles[index].position);

                uv[index] =
                    new Vector2(
                        (float)i / (particlesX - 1),
                        (float)j / (particlesY - 1));
            }
        }

        int quadCount =
            (particlesX - 1) *
            (particlesY - 1);

        int[] triangles =
            new int[quadCount * 6];

        int triangleIndex = 0;

        for (int j = 0; j < particlesY - 1; j++)
        {
            for (int i = 0; i < particlesX - 1; i++)
            {
                int v0 = GetIndex(i, j);
                int v1 = GetIndex(i + 1, j);
                int v2 = GetIndex(i, j + 1);
                int v3 = GetIndex(i + 1, j + 1);

                // Triangle 1
                triangles[triangleIndex++] = v0;
                triangles[triangleIndex++] = v2;
                triangles[triangleIndex++] = v1;

                // Triangle 2
                triangles[triangleIndex++] = v1;
                triangles[triangleIndex++] = v2;
                triangles[triangleIndex++] = v3;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    // ============================================================
    // MESH UPDATE
    // ============================================================

    public void UpdateMesh()
    {
        if (mesh == null)
            return;

        Vector3[] vertices =
            new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i] =
                transform.InverseTransformPoint(
                    particles[i].position);
        }

        mesh.vertices = vertices;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // ============================================================
    // UTILITY
    // ============================================================

    public int GetIndex(int x, int y)
    {
        return y * particlesX + x;
    }
}