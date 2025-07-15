using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MassSpringSystem : MonoBehaviour
{
    [Header("Collision")]
    public CollisionPlane wall;
    [Range(0, 1)] public float restitution = 0.8f;
    public float pointRadius = 0.1f;
    public float collisionPointRadius = 0.25f;

    [Header("Simulation Parameters")]
    public float stiffness = 1f;
    public float bendingStiffness = 0.5f;
    public float volumeStiffness = 1.0f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixedTop = false;
    [Range(0, 1)] public float fixedTopRatio = 0.2f;

    [Header("Rendering")]
    public float collisionColorIntensity = 50f;
    public float pointSize = 0.05f;

    [Header("Physics")]
    public int solverIterations = 10;
    public float groundLevel = 0f;
    public float groundStiffness = 1000f;

    [Header("Mesh Resolution")]
    public int resolution = 1;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Spring> springs = new List<Spring>();
    private HashSet<(int, int)> springPairs = new HashSet<(int, int)>();

    private Mesh deformableMesh;
    private Vector3[] updatedVertices;
    private int[] vertexMap;


    void Start()
    {
        deformableMesh = GetComponent<MeshFilter>().mesh;

        if (resolution > 1)
        {
            deformableMesh = SubdivideMesh(deformableMesh, resolution);
            GetComponent<MeshFilter>().mesh = deformableMesh;
        }

        InitializeWeldedMassSpringSystem();

        var renderer = gameObject.AddComponent<MassSpringRenderer>();
        renderer.massPoints = massPoints;
        renderer.springs = springs;

        var sharedMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        renderer.springMaterial = sharedMaterial;
        renderer.pointMaterial = sharedMaterial;
        renderer.pointSize = pointSize;

        // ????? ??? ?????? ?? ???? ?????????
        CollisionManager.Instance.RegisterSystem(this);
    }

    void OnDestroy()
    {
        // ????? ????? ?????? ??? ??????
        if (CollisionManager.Instance != null)
        {
            CollisionManager.Instance.UnregisterSystem(this);
        }
    }

    public List<MassPoint> GetMassPoints()
    {
        return massPoints;
    }

    void InitializeWeldedMassSpringSystem()
    {
        massPoints.Clear();
        springs.Clear();
        springPairs.Clear();

        Vector3[] originalVertices = deformableMesh.vertices;
        int[] originalTriangles = deformableMesh.triangles;

        updatedVertices = new Vector3[originalVertices.Length];
        vertexMap = new int[originalVertices.Length];

        Dictionary<Vector3, int> positionToUniqueIndex = new Dictionary<Vector3, int>();

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 localPos = originalVertices[i];
            if (!positionToUniqueIndex.TryGetValue(localPos, out int uniqueIndex))
            {
                uniqueIndex = massPoints.Count;
                Vector3 worldPos = transform.TransformPoint(localPos);
                // ????? ??? ??? ??????? ??? ????
                massPoints.Add(new MassPoint(worldPos, 1f, collisionPointRadius, false, gameObject.GetInstanceID()));
                positionToUniqueIndex.Add(localPos, uniqueIndex);
            }
            vertexMap[i] = uniqueIndex;
        }

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int i0 = originalTriangles[i], i1 = originalTriangles[i + 1], i2 = originalTriangles[i + 2];
            int unique_i0 = vertexMap[i0], unique_i1 = vertexMap[i1], unique_i2 = vertexMap[i2];

            if (unique_i0 != unique_i1) TryAddSpring(unique_i0, unique_i1, stiffness);
            if (unique_i1 != unique_i2) TryAddSpring(unique_i1, unique_i2, stiffness);
            if (unique_i2 != unique_i0) TryAddSpring(unique_i2, unique_i0, stiffness);
        }

        AddBendingSprings();
        AddVolumeSprings();

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

    void LateUpdate()
    {
        for (int i = 0; i < updatedVertices.Length; i++)
        {
            int massPointIndex = vertexMap[i];
            MassPoint mp = massPoints[massPointIndex];
            updatedVertices[i] = transform.InverseTransformPoint(mp.Position);
        }
        deformableMesh.vertices = updatedVertices;
        deformableMesh.RecalculateNormals();
        deformableMesh.RecalculateBounds();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        foreach (var mp in massPoints)
        {
            mp.Integrate(dt, gravity, damping);
        }

        for (int i = 0; i < solverIterations; i++)
        {
            foreach (var spring in springs)
            {
                spring.SolveConstraint();
            }

            // ??????? ?????? ???????? (???? ???? ??????)
            foreach (var mp in massPoints)
            {
                if (mp.Position.y < groundLevel)
                {
                    mp.Position = new Vector3(mp.Position.x, groundLevel, mp.Position.z);
                }

                if (wall != null && wall.IsPointOnBounds(mp.Position))
                {
                    Vector3 planeNormal = wall.GetNormal();
                    Vector3 planePoint = wall.GetPoint();
                    float distance = Vector3.Dot(mp.Position - planePoint, planeNormal);

                    if (distance < pointRadius)
                    {
                        mp.Position += planeNormal * (pointRadius - distance);
                        Vector3 correctedVelocity = mp.Position - mp.OldPosition;
                        float normalVelocityComponent = Vector3.Dot(correctedVelocity, planeNormal);
                        Vector3 reflectedVelocity = correctedVelocity - planeNormal * (1 + restitution) * normalVelocityComponent;
                        mp.OldPosition = mp.Position - reflectedVelocity;
                    }
                }
            }
        }
        // ?? ??? ???? ??????? ??? ???????? ?? ??? ??? CollisionManager ??????? ????
    }

    void AddVolumeSprings()
    {
        for (int i = 0; i < massPoints.Count; i++)
        {
            float maxDist = 0;
            int farthestIndex = -1;
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
            if (farthestIndex != -1)
            {
                TryAddSpring(i, farthestIndex, volumeStiffness);
            }
        }
    }

    void AddBendingSprings()
    {
        Dictionary<int, List<int>> neighborMap = new Dictionary<int, List<int>>();
        for (int i = 0; i < massPoints.Count; i++) neighborMap[i] = new List<int>();

        foreach (var spring in springs)
        {
            int indexA = massPoints.IndexOf(spring.A);
            int indexB = massPoints.IndexOf(spring.B);
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

    Mesh SubdivideMesh(Mesh mesh, int resolution)
    {
        var midpointCache = new Dictionary<(int, int), int>();
        var oldVerts = new List<Vector3>(mesh.vertices);
        var oldTris = new List<int>(mesh.triangles);
        for (int i = 0; i < resolution; i++)
        {
            var newTris = new List<int>();
            midpointCache.Clear();
            for (int j = 0; j < oldTris.Count; j += 3)
            {
                int i0 = oldTris[j], i1 = oldTris[j + 1], i2 = oldTris[j + 2];
                int m01 = GetMidpoint(i0, i1, oldVerts, midpointCache);
                int m12 = GetMidpoint(i1, i2, oldVerts, midpointCache);
                int m20 = GetMidpoint(i2, i0, oldVerts, midpointCache);
                newTris.AddRange(new[] { i0, m01, m20 });
                newTris.AddRange(new[] { i1, m12, m01 });
                newTris.AddRange(new[] { i2, m20, m12 });
                newTris.AddRange(new[] { m01, m12, m20 });
            }
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
        var key = (Mathf.Min(indexA, indexB), Mathf.Max(indexA, indexB));
        if (cache.TryGetValue(key, out int existingIndex))
        {
            return existingIndex;
        }
        Vector3 newVert = (vertices[indexA] + vertices[indexB]) * 0.5f;
        int newIndex = vertices.Count;
        vertices.Add(newVert);
        cache.Add(key, newIndex);
        return newIndex;
    }
}