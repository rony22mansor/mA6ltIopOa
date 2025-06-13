using UnityEngine;
using System.Collections.Generic;

public class MassSpringCube : MonoBehaviour
{
    public int resolution = 3;
    public float spacing = 0.5f;
    public float stiffness = 500f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixed;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Vector3Int> springs = new List<Vector3Int>();

    void Start()
    {
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 pos = transform.position + new Vector3(
                        (x - resolution / 2f) * spacing,
                        (y - resolution / 2f) * spacing + 2f,
                        (z - resolution / 2f) * spacing
                    );

                    bool fixedPoint = isFixed &&
                        y == resolution - 1 &&
                        (x == 0 || x == resolution - 1) &&
                        (z == 0 || z == resolution - 1);

                    MassPoint mp = new MassPoint(pos, 3f, fixedPoint);
                    massPoints.Add(mp);
                }
            }
        }

        CreateSprings();

        var renderer = gameObject.AddComponent<MassSpringRenderer>();
        renderer.massPoints = massPoints;
        renderer.springs = springs;
        renderer.spacing = spacing;
        renderer.sphereMesh = CreateSphereMesh();
        renderer.sphereMaterial = new Material(Shader.Find("Standard"));
        renderer.springMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
    }

    void CreateSprings()
    {
        int[] dx = { 1, 0, 0 };
        int[] dy = { 0, 1, 0 };
        int[] dz = { 0, 0, 1 };

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    int currentIndex = Index(x, y, z);

                    for (int dir = 0; dir < 3; dir++)
                    {
                        int nx = x + dx[dir];
                        int ny = y + dy[dir];
                        int nz = z + dz[dir];

                        if (nx < resolution && ny < resolution && nz < resolution)
                        {
                            int neighborIndex = Index(nx, ny, nz);
                            springs.Add(new Vector3Int(currentIndex, neighborIndex, 0));
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var s in springs)
        {
            MassPoint a = massPoints[s.x];
            MassPoint b = massPoints[s.y];
            Vector3 delta = b.Position - a.Position;
            float currentLength = delta.magnitude;
            Vector3 force = stiffness * (currentLength - spacing) * delta.normalized;

            a.ApplyForce(force, dt);
            b.ApplyForce(-force, dt);
        }

        foreach (var mp in massPoints)
        {
            mp.ApplyForce(Vector3.up * gravity * mp.Mass, dt);
            mp.ApplyDamping(damping);

            if (mp.Position.y < 0.1f)
            {
                mp.Position.y = 0.1f;
                mp.Velocity.y *= -0.5f;
            }
        }
    }

    int Index(int x, int y, int z)
    {
        return x * resolution * resolution + y * resolution + z;
    }

    Mesh CreateSphereMesh()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = sphere.GetComponent<MeshFilter>().mesh;
        Destroy(sphere);
        return mesh;
    }
}
