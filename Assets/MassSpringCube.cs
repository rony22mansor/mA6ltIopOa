

using UnityEngine;
using System.Collections.Generic;

public class MassSpringCube : MonoBehaviour
{
    public int resolution = 3;
    public float spacing = 0.5f;
    public float stiffness = 500f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    //public Material springMaterial;

    public bool isFixed;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Vector3Int> springs = new List<Vector3Int>(); // x = a, y = b, z unused
    private List<GameObject> sphereVisuals = new List<GameObject>();
    private List<LineRenderer> springLines = new List<LineRenderer>();

    void Start()
    {
        // Create mass points and visuals
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

                    bool isFixed;
                    if (this.isFixed)
                    {
                        isFixed = (y == resolution - 1) &&
                        (x == 0 || x == resolution - 1) &&
                       (z == 0 || z == resolution - 1);
                    }
                    else
                    {
                        isFixed = false;
                    }

                    MassPoint mp = new MassPoint(pos, 3f, isFixed);
                    massPoints.Add(mp);

                    // Visual sphere
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.localScale = Vector3.one * 0.1f;
                    sphere.transform.position = pos;
                    Renderer renderer = sphere.GetComponent<Renderer>();
                    renderer.material.color = Color.red;
                    sphereVisuals.Add(sphere);
                }
            }
        }

        // Create springs and line renderers
        CreateSprings();
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

                            // LineRenderer for the spring
                            GameObject lineObj = new GameObject("Spring");
                            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                            lr.material =  new Material(Shader.Find("Sprites/Default"));
                            lr.startWidth = 0.02f;
                            lr.endWidth = 0.02f;
                            lr.positionCount = 2;
                            lr.useWorldSpace = true;
                            springLines.Add(lr);
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Apply spring forces
        foreach (var s in springs)
        {
            MassPoint a = massPoints[s.x];
            MassPoint b = massPoints[s.y];
            Vector3 delta = b.Position - a.Position;
            float currentLength = delta.magnitude;
            float restLength = spacing;
            Vector3 force = stiffness * (currentLength - restLength) * delta.normalized;

            a.ApplyForce(force, dt);
            b.ApplyForce(-force, dt);
        }

        // Apply gravity and damping
        foreach (var mp in massPoints)
        {
            mp.ApplyForce(Vector3.up * gravity * mp.Mass, dt);
            mp.ApplyDamping(damping);

            // Ground collision
            if (mp.Position.y < 0.1f)
            {
                mp.Position.y = 0.1f;
                mp.Velocity.y *= -0.5f;
            }
        }

        // Update visuals
        for (int i = 0; i < massPoints.Count; i++)
        {
            sphereVisuals[i].transform.position = massPoints[i].Position;
        }

        for (int i = 0; i < springs.Count; i++)
        {
            var s = springs[i];
            LineRenderer lr = springLines[i];
            Vector3 posA = massPoints[s.x].Position;
            Vector3 posB = massPoints[s.y].Position;
            lr.SetPosition(0, posA);
            lr.SetPosition(1, posB);

            float currentLength = Vector3.Distance(posA, posB);
            float stretchRatio = (currentLength - spacing) / spacing;
            stretchRatio = Mathf.Max(0f, stretchRatio); // ignore compression

            Color springColor;

            if (stretchRatio < 0.5f)
            {
                // Blue to Yellow
                springColor = Color.Lerp(Color.blue, Color.yellow, stretchRatio / 0.5f);
            }
            else
            {
                // Yellow to Red
                springColor = Color.Lerp(Color.yellow, Color.red, (stretchRatio - 0.5f) / 0.5f);
            }

            lr.startColor = springColor;
            lr.endColor = springColor;
        }
    }

    int Index(int x, int y, int z)
    {
        return x * resolution * resolution + y * resolution + z;
    }
}