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
    private List<GameObject> sphereVisuals = new List<GameObject>();
    private List<LineRenderer> springVisuals = new List<LineRenderer>();

    void Start()
    {
        float radius = resolution / 2f;

        // Generate mass points inside a sphere
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
                    bool isFixed;
                    if (this.isFixed)
                    {
                         isFixed =
                            y == resolution - 1;
                    }
                    else {
                        isFixed = false;
                    }
                        var mp = new MassPoint(pos, 3f, isFixed);
                    massPoints.Add(mp);

                    // Create visible sphere
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.localScale = Vector3.one * 0.2f;
                    sphere.transform.position = pos;
                    sphereVisuals.Add(sphere);
                    sphere.GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }

        // Create springs between nearby points
        float maxSpringLength = spacing * 1.05f;
        for (int i = 0; i < massPoints.Count; i++)
        {
            for (int j = i + 1; j < massPoints.Count; j++)
            {
                float dist = Vector3.Distance(massPoints[i].Position, massPoints[j].Position);
                if (dist <= maxSpringLength)
                {
                    springs.Add(new Vector3Int(i, j, 0));

                    // Create LineRenderer
                    GameObject lineObj = new GameObject("Spring");
                    LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.positionCount = 2;
                    lr.startWidth = 0.03f;
                    lr.endWidth = 0.03f;
                    springVisuals.Add(lr);
                }
            }
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Apply spring forces
        for (int i = 0; i < springs.Count; i++)
        {
            var s = springs[i];
            MassPoint a = massPoints[s.x];
            MassPoint b = massPoints[s.y];
            Vector3 delta = b.Position - a.Position;
            float currentLength = delta.magnitude;
            float restLength = spacing;
            Vector3 force = stiffness * (currentLength - restLength) * delta.normalized;

            a.ApplyForce(force, dt);
            b.ApplyForce(-force, dt);

            // Update line renderer position and color
            LineRenderer lr = springVisuals[i];
            lr.SetPosition(0, a.Position);
            lr.SetPosition(1, b.Position);

            float stretch = Mathf.Clamp01((currentLength - restLength) / restLength);
            lr.startColor = lr.endColor = StretchColor(stretch);
        }

        // Apply gravity, damping, and ground collision
        foreach (var mp in massPoints)
        {
            mp.ApplyForce(Vector3.up * gravity * mp.Mass, dt);
            mp.ApplyDamping(damping);

            // Ground collision
            if (mp.Position.y < 0.1f)
            {
                mp.Position.y = 0.1f;
                if (mp.Velocity.y < 0)
                    mp.Velocity.y *= -0.5f; // bounce with energy loss
            }
        }


        // Update mass point visuals
        for (int i = 0; i < massPoints.Count; i++)
        {
            sphereVisuals[i].transform.position = massPoints[i].Position;
        }
    }

    Color StretchColor(float t)
    {
        // t ∈ [0, 1], blue → yellow → red
        if (t < 0.5f)
        {
            return Color.Lerp(Color.blue, Color.yellow, t * 2f); // 0 → 0.5
        }
        else
        {
            return Color.Lerp(Color.yellow, Color.red, (t - 0.5f) * 2f); // 0.5 → 1
        }
    }
}



