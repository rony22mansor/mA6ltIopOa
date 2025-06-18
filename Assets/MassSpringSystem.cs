using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MassSpringSystem : MonoBehaviour
{
    [Header("Simulation Parameters")]
    public float stiffness = 500f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixedTop = false;
    [Range(0, 1)] public float fixedTopRatio = 0.2f;

    [Header("Rendering")]
    public float pointSize = 0.1f;

    [Header("Physics")]
    public float groundLevel = 0f;
    public float groundStiffness = 1000f;

    [Header("Mesh Resolution")]
    public int resolution = 1;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Spring> springs = new List<Spring>();

    private Mesh originalMesh;
    private Vector3[] originalVertices;
    private int[] originalTriangles;

    void Start()
    {
        originalMesh = GetComponent<MeshFilter>().mesh;

        if (resolution > 1)
            originalMesh = SubdivideMesh(originalMesh, resolution);

        originalVertices = originalMesh.vertices;
        originalTriangles = originalMesh.triangles;

        GenerateMassPoints();
        CreateSpringsFromMesh();

        var renderer = gameObject.AddComponent<MassSpringRenderer>();
        renderer.massPoints = massPoints;
        renderer.springs = springs;

        renderer.springMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        renderer.sphereMaterial = new Material(Shader.Find("Standard"));
        renderer.sphereMesh = CreateSphereMesh();
        renderer.pointSize = pointSize;
    }

    void GenerateMassPoints()
    {
        massPoints.Clear();
        float maxY = float.MinValue;

        foreach (var v in originalVertices)
        {
            Vector3 worldPos = transform.TransformPoint(v);
            massPoints.Add(new MassPoint(worldPos, 1f));
            if (worldPos.y > maxY)
                maxY = worldPos.y;
        }

        if (isFixedTop)
        {
            float thresholdY = maxY - (maxY * fixedTopRatio);
            foreach (var mp in massPoints)
            {
                if (mp.Position.y >= thresholdY)
                    mp.IsFixed = true;
            }
        }
    }

    void CreateSpringsFromMesh()
    {
        springs.Clear();
        HashSet<(int, int)> springPairs = new HashSet<(int, int)>();

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int i0 = originalTriangles[i];
            int i1 = originalTriangles[i + 1];
            int i2 = originalTriangles[i + 2];

            TryAddSpring(i0, i1, springPairs);
            TryAddSpring(i1, i2, springPairs);
            TryAddSpring(i2, i0, springPairs);
        }
    }

    void TryAddSpring(int indexA, int indexB, HashSet<(int, int)> pairs)
    {
        int min = Mathf.Min(indexA, indexB);
        int max = Mathf.Max(indexA, indexB);
        var pair = (min, max);

        if (!pairs.Contains(pair))
        {
            pairs.Add(pair);
            springs.Add(new Spring(massPoints[min], massPoints[max], stiffness));
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        foreach (var spring in springs)
            spring.ApplyForce();

        foreach (var mp in massPoints)
        {
            if (!mp.IsFixed)
            {
                mp.ApplyForce(Vector3.up * gravity * mp.Mass, dt);
                mp.ApplyDamping(damping);

                if (mp.Position.y < groundLevel)
                {
                    float penetration = groundLevel - mp.Position.y;
                    Vector3 groundForce = Vector3.up * penetration * groundStiffness;
                    mp.ApplyForce(groundForce, dt);
                }
            }
        }
    }

    Mesh CreateSphereMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        return mesh;
    }

    Mesh SubdivideMesh(Mesh mesh, int factor)
    {
        Mesh newMesh = new Mesh();
        var verts = new List<Vector3>();
        var tris = new List<int>();

        Vector3[] oldVerts = mesh.vertices;
        int[] oldTris = mesh.triangles;

        for (int i = 0; i < oldTris.Length; i += 3)
        {
            Vector3 v0 = oldVerts[oldTris[i]];
            Vector3 v1 = oldVerts[oldTris[i + 1]];
            Vector3 v2 = oldVerts[oldTris[i + 2]];

            for (int u = 0; u < factor; u++)
            {
                for (int v = 0; v < factor - u; v++)
                {
                    float fu0 = (float)u / factor;
                    float fv0 = (float)v / factor;
                    float fu1 = (float)(u + 1) / factor;
                    float fv1 = (float)v / factor;
                    float fu2 = (float)u / factor;
                    float fv2 = (float)(v + 1) / factor;

                    Vector3 p0 = v0 * (1 - fu0 - fv0) + v1 * fu0 + v2 * fv0;
                    Vector3 p1 = v0 * (1 - fu1 - fv1) + v1 * fu1 + v2 * fv1;
                    Vector3 p2 = v0 * (1 - fu2 - fv2) + v1 * fu2 + v2 * fv2;

                    int i0 = verts.Count;
                    verts.Add(p0);
                    verts.Add(p1);
                    verts.Add(p2);

                    tris.Add(i0);
                    tris.Add(i0 + 1);
                    tris.Add(i0 + 2);
                }
            }
        }

        newMesh.SetVertices(verts);
        newMesh.SetTriangles(tris, 0);
        newMesh.RecalculateNormals();
        return newMesh;
    }
    }