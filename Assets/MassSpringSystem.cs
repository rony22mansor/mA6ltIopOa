using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MassSpringSystem : MonoBehaviour
{
    [Header("Simulation Parameters")]
    public float stiffness = 1f;
    public float bendingStiffness = 0.5f; // NEW: For our new springs
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixedTop = false;
    [Range(0, 1)] public float fixedTopRatio = 0.2f;

    [Header("Rendering")]
    public float pointSize = 0.05f;

    [Header("Physics")]
    public int solverIterations = 10; // NEW
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

        // This single method now handles vertex welding and spring creation correctly.
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

        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        // --- Step 1: Weld Vertices ---
        // This logic ensures we only create ONE MassPoint for any unique position.

        // Maps a unique position to the index of the MassPoint for that position.
        Dictionary<Vector3, int> positionToUniqueIndex = new Dictionary<Vector3, int>();
        // Maps each original vertex index to the index of its corresponding UNIQUE MassPoint.
        int[] vertexMap = new int[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            // Use local space for the dictionary key to handle floating point precision
            Vector3 localPos = originalVertices[i];

            // If we haven't seen this position before...
            if (!positionToUniqueIndex.TryGetValue(localPos, out int uniqueIndex))
            {
                // ...it's a new unique vertex. Create a MassPoint for it.
                uniqueIndex = massPoints.Count;
                Vector3 worldPos = transform.TransformPoint(localPos);
                massPoints.Add(new MassPoint(worldPos, 1f));
                positionToUniqueIndex.Add(localPos, uniqueIndex);
            }

            // Map the original vertex 'i' to its unique MassPoint index.
            vertexMap[i] = uniqueIndex;
        }

        // --- Step 2: Create Springs using the Welded Vertices ---
        // Now we connect the triangles using our map to the unique points.

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int i0 = originalTriangles[i];
            int i1 = originalTriangles[i + 1];
            int i2 = originalTriangles[i + 2];

            // Use the map to get the welded indices
            int unique_i0 = vertexMap[i0];
            int unique_i1 = vertexMap[i1];
            int unique_i2 = vertexMap[i2];

            // Add structural springs (don't add a spring if it's connecting a point to itself)
            if (unique_i0 != unique_i1) TryAddSpring(unique_i0, unique_i1, stiffness);
            if (unique_i1 != unique_i2) TryAddSpring(unique_i1, unique_i2, stiffness);
            if (unique_i2 != unique_i0) TryAddSpring(unique_i2, unique_i0, stiffness);
        }

        // --- Step 3: Add Bending Springs (this method will now work correctly) ---
        AddBendingSprings();

        // --- Step 4: Set Fixed Points (this logic is now cleaner) ---
        if (isFixedTop)
        {
            float maxY = float.MinValue;
            foreach (var mp in massPoints)
            {
                if (mp.Position.y > maxY)
                    maxY = mp.Position.y;
            }

            float thresholdY = maxY - (maxY * fixedTopRatio);
            foreach (var mp in massPoints)
            {
                if (mp.Position.y >= thresholdY)
                    mp.IsFixed = true;
            }
        }
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
        springPairs.Clear(); // Clear the set for a fresh start

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int i0 = originalTriangles[i];
            int i1 = originalTriangles[i + 1];
            int i2 = originalTriangles[i + 2];

            // Use the new TryAddSpring with the main stiffness parameter
            TryAddSpring(i0, i1, stiffness);
            TryAddSpring(i1, i2, stiffness);
            TryAddSpring(i2, i0, stiffness);
        }
    }

    void AddBendingSprings()
    {
        // This method adds springs to resist folding by connecting vertices
        // that are two steps away from each other on the mesh (neighbors of neighbors).

        Dictionary<int, List<int>> neighborMap = new Dictionary<int, List<int>>();
        for (int i = 0; i < massPoints.Count; i++)
        {
            neighborMap[i] = new List<int>();
        }

        // First, find all direct neighbors for each vertex from the structural springs
        foreach (var spring in springs)
        {
            // We need to find the original indices of the mass points
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

        // Now, for each vertex 'i', connect its neighbors to each other.
        // This forms a reinforcing triangle (i -> neighbor_j -> neighbor_k)
        // and the new spring is the base of that triangle.
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

    // Modify the method to accept a stiffness value
    void TryAddSpring(int indexA, int indexB, float springStiffness)
    {
        int min = Mathf.Min(indexA, indexB);
        int max = Mathf.Max(indexA, indexB);
        var pair = (min, max);

        // CORRECTED: Use 'springPairs' instead of 'pairs'
        if (!springPairs.Contains(pair))
        {
            springPairs.Add(pair); // CORRECTED: Use 'springPairs'
            springs.Add(new Spring(massPoints[min], massPoints[max], springStiffness));
        }
    }



    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // In MassSpringSystem.cs -> FixedUpdate()
        foreach (var mp in massPoints)
        {
            mp.Integrate(dt, gravity, damping); // Pass damping in
        }

        // 2. Iteratively solve constraints
        // This loop is the key to rigidity. It enforces the spring lengths multiple times.
        for (int i = 0; i < solverIterations; i++)
        {
            // Solve ground constraint
            foreach (var mp in massPoints)
            {
                if (mp.Position.y < groundLevel)
                {
                    // Directly correct the position
                    mp.Position = new Vector3(mp.Position.x, groundLevel, mp.Position.z);
                }
            }

            // Solve spring constraints
            foreach (var spring in springs)
            {
                spring.SolveConstraint();
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