using UnityEngine;
using System.Collections.Generic;

public class MassSpringBall : MonoBehaviour
{
    public int resolution = 6;
    public float spacing = 0.5f;
    public float stiffness = 500f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixed;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Vector3Int> springs = new List<Vector3Int>();

    void Start()
    {
        float radius = resolution / 2f;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 offset = new Vector3(
                        x - resolution / 2f,
                        y - resolution / 2f,
                        z - resolution / 2f
                    );

                    if (offset.magnitude > radius)
                        continue;

                    Vector3 pos = transform.position + offset * spacing;
                    bool fixedPoint = isFixed && y == resolution - 1;

                    var mp = new MassPoint(pos, 3f, fixedPoint);
                    massPoints.Add(mp);
                }
            }
        }

        float maxSpringLength = spacing * 1.05f;
        for (int i = 0; i < massPoints.Count; i++)
        {
            for (int j = i + 1; j < massPoints.Count; j++)
            {
                float dist = Vector3.Distance(massPoints[i].Position, massPoints[j].Position);
                if (dist <= maxSpringLength)
                {
                    springs.Add(new Vector3Int(i, j, 0));
                }
            }
        }

        var renderer = gameObject.AddComponent<MassSpringRenderer>();
        renderer.massPoints = massPoints;
        renderer.springs = springs;
        renderer.spacing = spacing;
        renderer.sphereMesh = CreateSphereMesh();
        renderer.sphereMaterial = new Material(Shader.Find("Standard"));
        renderer.springMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
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
                if (mp.Velocity.y < 0)
                    mp.Velocity.y *= -0.5f;
            }
        }
    }

    Mesh CreateSphereMesh()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = sphere.GetComponent<MeshFilter>().mesh;
        Destroy(sphere);
        return mesh;
    }
}
