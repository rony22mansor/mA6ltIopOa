using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;

public class MassSpringFromMesh : MonoBehaviour
{
    public Mesh sourceMesh;
    public float spacing = 1f;
    public float stiffness = 500f;
    public float damping = 0.98f;
    public float gravity = -9.81f;
    public bool isFixed = false;

    private List<MassPoint> massPoints = new List<MassPoint>();
    private List<Spring> springs = new List<Spring>();
    private MassSpringRenderer msRenderer;

    void Start()
    {
        if (sourceMesh == null)
        {
            Debug.LogError("❌ لا يوجد Mesh محدد في MassSpringFromMesh.");
            return;
        }

        Debug.Log("✅ بدأ إنشاء نظام Mass-Spring من Mesh: " + sourceMesh.name);
        Debug.Log("عدد الرؤوس في الـ Mesh: " + sourceMesh.vertexCount);

        GenerateMassSpringFromMesh(sourceMesh);

        msRenderer = gameObject.AddComponent<MassSpringRenderer>();
        msRenderer.massPoints = massPoints;
        msRenderer.springs = springs;

        Debug.Log($"🧪 عدد النقاط: {massPoints.Count}, عدد النوابض: {springs.Count}");
        if (massPoints.Count == 0 || springs.Count == 0)
            Debug.LogWarning("⚠️ لم يتم توليد أي نقاط أو نوابض. تأكد من أن الـ Mesh يحتوي على رؤوس ومثلثات.");
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // تطبيق قوى النوابض بين النقاط
        foreach (var spring in springs)
        {
            spring.ApplyForce();
        }

        // تطبيق قوى الجاذبية والتخميد وتحديث المواقع
        foreach (var mp in massPoints)
        {
            mp.ApplyForce(Vector3.up * gravity * mp.Mass, dt);
            mp.ApplyDamping(damping);

            // منع النقاط من النزول تحت مستوى الأرض (y = 0.1)
            if (mp.Position.y < 0.1f)
            {
                mp.Position.y = 0.1f;
                if (mp.Velocity.y < 0) mp.Velocity.y *= -0.5f;
            }
        }
    }

    void GenerateMassSpringFromMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Dictionary<Vector3, int> uniqueVerts = new Dictionary<Vector3, int>(new Vector3Comparer());

        Debug.Log("📦 إنشاء MassPoints من الرؤوس");

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i] * spacing);

            if (!uniqueVerts.ContainsKey(worldPos))
            {
                bool fixedPoint = isFixed && worldPos.y >= transform.position.y;
                MassPoint mp = new MassPoint(worldPos, 1f, fixedPoint);
                uniqueVerts.Add(worldPos, massPoints.Count);
                massPoints.Add(mp);
            }
        }

        Debug.Log($"✅ تم إنشاء {massPoints.Count} نقطة.");

        int[] triangles = mesh.triangles;

        Debug.Log("🔧 إنشاء النوابض من المثلثات");

        int springCountBefore = springs.Count;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = transform.TransformPoint(vertices[triangles[i]] * spacing);
            Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]] * spacing);
            Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]] * spacing);

            TryCreateSpring(uniqueVerts, v0, v1);
            TryCreateSpring(uniqueVerts, v1, v2);
            TryCreateSpring(uniqueVerts, v2, v0);
        }

        Debug.Log($"✅ تم إنشاء {springs.Count - springCountBefore} نابض.");
    }

    void TryCreateSpring(Dictionary<Vector3, int> indexMap, Vector3 a, Vector3 b)
    {
        int ia = indexMap[a];
        int ib = indexMap[b];

        if (!Spring.Exists(springs, massPoints[ia], massPoints[ib]))
        {
            springs.Add(new Spring(massPoints[ia], massPoints[ib], stiffness));
        }
    }

    // يستخدم مقارنة دقيقة للمواقع
    class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 a, Vector3 b)
        {
            return Vector3.SqrMagnitude(a - b) < 1e-6f;
        }

        public int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }
}
