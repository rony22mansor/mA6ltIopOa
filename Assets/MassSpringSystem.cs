using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MassSpringSystem : MonoBehaviour
{
    [Header("Simulation Parameters")]
    public float stiffness = 1f;
    public float bendingStiffness = 0.5f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixedTop = false;
    [Range(0, 1)] public float fixedTopRatio = 0.2f;

    [Header("Rendering")]
    public float pointSize = 0.05f;

    [Header("Physics")]
    public int solverIterations = 10;
    public float groundLevel = 0f;
    public float groundStiffness = 1000f;

    [Header("Mesh Resolution")]
    public int resolution = 1;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Spring> springs = new List<Spring>();

    private Mesh originalMesh;
    private Vector3[] originalVertices;
    private int[] originalTriangles;
    private HashSet<(int, int)> springPairs = new HashSet<(int, int)>();

    void Start()
    {
        originalMesh = GetComponent<MeshFilter>().mesh;

        if (resolution > 1)
            originalMesh = SubdivideMesh(originalMesh, resolution);

        originalVertices = originalMesh.vertices;
        originalTriangles = originalMesh.triangles;

        InitializeWeldedMassSpringSystem();

        var renderer = gameObject.AddComponent<MassSpringRenderer>();
        renderer.massPoints = massPoints;
        renderer.springs = springs;

        renderer.springMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        renderer.sphereMaterial = new Material(Shader.Find("Standard"));
        renderer.sphereMesh = CreateSphereMesh();
        renderer.pointSize = pointSize;
    }

    void InitializeWeldedMassSpringSystem()
    {
        massPoints.Clear();
        springs.Clear();
        springPairs.Clear();

        Dictionary<Vector3, int> positionToUniqueIndex = new Dictionary<Vector3, int>();
        int[] vertexMap = new int[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 localPos = originalVertices[i];

            if (!positionToUniqueIndex.TryGetValue(localPos, out int uniqueIndex))
            {
                uniqueIndex = massPoints.Count;
                Vector3 worldPos = transform.TransformPoint(localPos);
                //massPoints.Add(new MassPoint(worldPos, 1f));
                massPoints.Add(new MassPoint(worldPos, 1f, false, gameObject.GetInstanceID()));

                positionToUniqueIndex.Add(localPos, uniqueIndex);
            }

            vertexMap[i] = uniqueIndex;
        }

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int i0 = originalTriangles[i];
            int i1 = originalTriangles[i + 1];
            int i2 = originalTriangles[i + 2];

            int unique_i0 = vertexMap[i0];
            int unique_i1 = vertexMap[i1];
            int unique_i2 = vertexMap[i2];

            if (unique_i0 != unique_i1) TryAddSpring(unique_i0, unique_i1, stiffness);
            if (unique_i1 != unique_i2) TryAddSpring(unique_i1, unique_i2, stiffness);
            if (unique_i2 != unique_i0) TryAddSpring(unique_i2, unique_i0, stiffness);
        }

        AddBendingSprings();

        if (isFixedTop)
        {
            float maxY = float.MinValue;
            foreach (var mp in massPoints)
                if (mp.Position.y > maxY) maxY = mp.Position.y;

            float thresholdY = maxY - (maxY * fixedTopRatio);
            foreach (var mp in massPoints)
                if (mp.Position.y >= thresholdY) mp.IsFixed = true;
        }
    }

    void TryAddSpring(int indexA, int indexB, float springStiffness)
    {
        int min = Mathf.Min(indexA, indexB);
        int max = Mathf.Max(indexA, indexB);
        var pair = (min, max);

        if (!springPairs.Contains(pair))
        {
            springPairs.Add(pair);
            springs.Add(new Spring(massPoints[min], massPoints[max], springStiffness));
        }
    }

    void AddBendingSprings()
    {
        Dictionary<int, List<int>> neighborMap = new Dictionary<int, List<int>>();
        for (int i = 0; i < massPoints.Count; i++)
            neighborMap[i] = new List<int>();

        foreach (var spring in springs)
        {
            int indexA = -1, indexB = -1;
            for (int i = 0; i < massPoints.Count; i++)
            {
                if (massPoints[i] == spring.A) indexA = i;
                if (massPoints[i] == spring.B) indexB = i;
                if (indexA != -1 && indexB != -1) break;
            }

            if (indexA != -1 && indexB != -1)
            {
                neighborMap[indexA].Add(indexB);
                neighborMap[indexB].Add(indexA);
            }
        }

        for (int i = 0; i < massPoints.Count; i++)
        {
            var neighbors = neighborMap[i];
            for (int j = 0; j < neighbors.Count; j++)
            {
                for (int k = j + 1; k < neighbors.Count; k++)
                {
                    TryAddSpring(neighbors[j], neighbors[k], bendingStiffness);
                }
            }
        }
    }
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Step 1: Integrate (Verlet)
        foreach (var mp in massPoints)
        {
            mp.Integrate(dt, gravity, damping);
        }

        // Step 2: Constraint solving
        for (int i = 0; i < solverIterations; i++)
        {
            // Ground constraint
            foreach (var mp in massPoints)
            {
                if (mp.Position.y < groundLevel)
                {
                    mp.Position = new Vector3(mp.Position.x, groundLevel, mp.Position.z);
                }
            }

            // Spring constraint
            foreach (var spring in springs)
            {
                spring.SolveConstraint();
            }
        }

        // Step 3: AABB collision with reflection
        float aabbRadius = 0.25f; // You can tweak this

        List<MassPoint> allPoints = MassPoint.AllPoints;

        for (int i = 0; i < allPoints.Count; i++)
        {
            for (int j = i + 1; j < allPoints.Count; j++)
            {
                MassPoint mpA = allPoints[i];
                MassPoint mpB = allPoints[j];

                // Skip same object
                if (mpA.ObjectID == mpB.ObjectID) continue;

                Vector3 diff = mpB.Position - mpA.Position;

                Bounds aabbA = new Bounds(mpA.Position, Vector3.one * aabbRadius * 2);
                Bounds aabbB = new Bounds(mpB.Position, Vector3.one * aabbRadius * 2);

                if (aabbA.Intersects(aabbB))
                {
                    Debug.Log($"✅ COLLISION: Point A (Object {mpA.ObjectID}) ↔ Point B (Object {mpB.ObjectID})");

                    if (diff == Vector3.zero)
                        diff = Vector3.right * 0.001f;

                    Vector3 normal = diff.normalized;

                    Vector3 velA = mpA.GetVelocity();
                    Vector3 velB = mpB.GetVelocity();
                    Vector3 relativeVelocity = velB - velA;

                    float separatingVelocity = Vector3.Dot(relativeVelocity, normal);

                    // Skip if moving apart
                    if (separatingVelocity > 0) continue;

                    float restitution = 1.0f; // Elastic bounce
                    float impulse = -(1 + restitution) * separatingVelocity * 0.5f;
                    Vector3 impulseVec = impulse * normal;

                    if (!mpA.IsFixed)
                        mpA.OldPosition -= impulseVec;

                    if (!mpB.IsFixed)
                        mpB.OldPosition += impulseVec;

                    // Push apart visually
                    float overlap = Mathf.Max(0, (aabbRadius * 2 - diff.magnitude)) * 0.5f;

                    if (!mpA.IsFixed)
                        mpA.Position -= normal * overlap;

                    if (!mpB.IsFixed)
                        mpB.Position += normal * overlap;
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
