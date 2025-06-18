using UnityEngine;
using System.Collections.Generic;

public class MassSpringRenderer : MonoBehaviour
{
    public List<MassPoint> massPoints;
    public List<Spring> springs;

    private Material lineMaterial;
    private Material sphereMaterial;
    private Mesh sphereMesh;

    void Awake()
    {
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        sphereMaterial = new Material(Shader.Find("Standard"));
        sphereMesh = CreateMiniSphere();
    }

    private void OnRenderObject()
    {
        if (massPoints == null || springs == null) return;

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.Begin(GL.LINES);

        foreach (var spring in springs)
        {
            float stretch = Mathf.Abs(Vector3.Distance(spring.A.Position, spring.B.Position) - spring.RestLength) / spring.RestLength;
            GL.Color(GetSpringColor(stretch));
            GL.Vertex(spring.A.Position);
            GL.Vertex(spring.B.Position);
        }

        GL.End();
        GL.PopMatrix();

        // رسم النقاط
        for (int i = 0; i < massPoints.Count; i++)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(massPoints[i].Position, Quaternion.identity, Vector3.one * 0.05f);
            Graphics.DrawMesh(sphereMesh, matrix, sphereMaterial, 0);
        }
    }

    private Color GetSpringColor(float t)
    {
        if (t < 0.5f)
            return Color.Lerp(Color.blue, Color.yellow, t * 2f);
        else
            return Color.Lerp(Color.yellow, Color.red, (t - 0.5f) * 2f);
    }

    private Mesh CreateMiniSphere()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        return mesh;
    }
}
