using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MassSpringSystem : MonoBehaviour
{

    [Header("Collision")]
    public CollisionPlane wall;
    [Range(0, 1)] public float restitution = 0.8f;
    public float pointRadius = 0.1f;

    [Header("Simulation Parameters")]
    public float stiffness = 1f;
    public float bendingStiffness = 0.5f; // NEW: For our new springs
    public float volumeStiffness = 1.0f; // <-- ADD THIS LINE
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

    // ---- NEW VARIABLES TO ADD ----
    private Mesh deformableMesh;
    private Vector3[] updatedVertices;
    private int[] vertexMap; // This will store the mapping from original vertices to unique mass points
    // ---- END OF NEW VARIABLES ----


    void Start()
    {
        // Get an INSTANCE of the mesh from the filter. This is important.
        deformableMesh = GetComponent<MeshFilter>().mesh;

        if (resolution > 1)
        {
            // Subdivide the mesh...
            deformableMesh = SubdivideMesh(deformableMesh, resolution);

            // ---- THE CRITICAL FIX ----
            // Tell the MeshFilter to use this new mesh for rendering!
            GetComponent<MeshFilter>().mesh = deformableMesh;
        }

        // This single method now handles vertex welding and spring creation correctly.
        InitializeWeldedMassSpringSystem();

        //// The rest of this is for drawing gizmos. You can disable this
        //// once the main mesh is deforming correctly.
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

        // CHANGE THIS: Use the deformable mesh's vertices
        Vector3[] originalVertices = deformableMesh.vertices;
        int[] originalTriangles = deformableMesh.triangles;

        // ---- NEW: Initialize our vertex arrays ----
        updatedVertices = new Vector3[originalVertices.Length];
        // CHANGE THIS: Initialize the class-level vertexMap instead of a local one
        vertexMap = new int[originalVertices.Length];
        // ----

        // --- Step 1: Weld Vertices ---
        Dictionary<Vector3, int> positionToUniqueIndex = new Dictionary<Vector3, int>();

        // REMOVE THIS LINE: We are now using the class-level variable
        // int[] vertexMap = new int[originalVertices.Length]; 

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

        AddVolumeSprings(); // <-- ADD THIS CALL

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

    void LateUpdate()
    {
        // 1. Loop through all original vertices
        for (int i = 0; i < updatedVertices.Length; i++)
        {
            // 2. Find the corresponding mass point using our map
            int massPointIndex = vertexMap[i];
            MassPoint mp = massPoints[massPointIndex];

            // 3. Convert the mass point's world position back to the object's local space
            updatedVertices[i] = transform.InverseTransformPoint(mp.Position);
        }

        // 4. Apply the new vertex positions to the mesh
        deformableMesh.vertices = updatedVertices;

        // 5. Recalculate normals and bounds for correct lighting and rendering
        deformableMesh.RecalculateNormals();
        deformableMesh.RecalculateBounds();
    }
    
    void AddVolumeSprings()
    {
        // This method adds internal springs to resist compression.
        // It connects each point to the point that is initially farthest away.
        for (int i = 0; i < massPoints.Count; i++)
        {
            float maxDist = 0;
            int farthestIndex = -1;

            // Find the point that is farthest away from the current one
            for (int j = 0; j < massPoints.Count; j++)
            {
                if (i == j) continue;

                float dist = Vector3.Distance(massPoints[i].InitialPosition, massPoints[j].InitialPosition);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthestIndex = j;
                }
            }

            // Add a spring connecting to the farthest point, if found
            if (farthestIndex != -1)
            {
                TryAddSpring(i, farthestIndex, volumeStiffness);
            }
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

            // Now, iterate through each mass point to apply gravity and collision
            foreach (var mp in massPoints)
            {
                if (mp.IsFixed) continue;

                // --- 1. Verlet Integration (Gravity) ---
                // (This is an example, make sure it matches your Verlet logic)
                Vector3 velocity = mp.Position - mp.OldPosition;
                mp.OldPosition = mp.Position;
                Vector3 gravityForce = Vector3.up * gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
                mp.Position += velocity * damping + gravityForce;


                // --- 2. Collision Detection & Response ---
                if (wall != null)
                {
                    Vector3 planeNormal = wall.GetNormal();
                    Vector3 planePoint = wall.GetPoint();

                    // Calculate distance from the point to the plane
                    float distance = Vector3.Dot(mp.Position - planePoint, planeNormal);

                    // Check for collision (if distance is less than the point's radius)
                    if (distance < pointRadius)
                    {
                        // --- Position Correction ---
                        // Move the point back to the surface of the plane
                        mp.Position += planeNormal * (pointRadius - distance);

                        // --- Velocity Correction (The Bounce) ---
                        // Recalculate velocity after position correction
                        Vector3 correctedVelocity = mp.Position - mp.OldPosition;

                        // Calculate the component of velocity that is perpendicular to the plane
                        float normalVelocityComponent = Vector3.Dot(correctedVelocity, planeNormal);

                        // Reflect the velocity and apply restitution (bounciness)
                        Vector3 reflectedVelocity = correctedVelocity - planeNormal * (1 + restitution) * normalVelocityComponent;

                        // Update the OldPosition to apply the new velocity
                        mp.OldPosition = mp.Position - reflectedVelocity;
                    }
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

    Mesh SubdivideMesh(Mesh mesh, int resolution)
    {
        // A dictionary to store the index of newly created vertices on an edge.
        // The key is a sorted pair of original vertex indices.
        var midpointCache = new Dictionary<(int, int), int>();
        var oldVerts = new List<Vector3>(mesh.vertices);
        var oldTris = new List<int>(mesh.triangles);

        for (int i = 0; i < resolution; i++)
        {
            var newTris = new List<int>();
            midpointCache.Clear();

            for (int j = 0; j < oldTris.Count; j += 3)
            {
                int i0 = oldTris[j];
                int i1 = oldTris[j + 1];
                int i2 = oldTris[j + 2];

                // Get or create the midpoints of the triangle's edges
                int m01 = GetMidpoint(i0, i1, oldVerts, midpointCache);
                int m12 = GetMidpoint(i1, i2, oldVerts, midpointCache);
                int m20 = GetMidpoint(i2, i0, oldVerts, midpointCache);

                // Create the 4 new triangles
                newTris.AddRange(new[] { i0, m01, m20 });
                newTris.AddRange(new[] { i1, m12, m01 });
                newTris.AddRange(new[] { i2, m20, m12 });
                newTris.AddRange(new[] { m01, m12, m20 }); // The central triangle
            }
            // The newly created triangles are now the old triangles for the next iteration
            oldTris = newTris;
        }

        Mesh newMesh = new Mesh();
        newMesh.SetVertices(oldVerts);
        newMesh.SetTriangles(oldTris, 0);
        newMesh.RecalculateNormals();
        return newMesh;
    }

    private int GetMidpoint(int indexA, int indexB, List<Vector3> vertices, Dictionary<(int, int), int> cache)
    {
        // Create a sorted key to ensure the same edge always produces the same key
        var key = (Mathf.Min(indexA, indexB), Mathf.Max(indexA, indexB));

        // If we've already created a vertex for this edge, return its index
        if (cache.TryGetValue(key, out int existingIndex))
        {
            return existingIndex;
        }

        // Otherwise, create the new vertex
        Vector3 newVert = (vertices[indexA] + vertices[indexB]) * 0.5f;
        int newIndex = vertices.Count;
        vertices.Add(newVert);

        // Add the new index to the cache for future lookups
        cache.Add(key, newIndex);
        return newIndex;
    }
}