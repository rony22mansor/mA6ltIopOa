using UnityEngine;
using System.Collections.Generic;

public class MassSpringRenderer : MonoBehaviour
{
    public List<MassPoint> massPoints;
    public List<Vector3Int> springs;
    public Mesh sphereMesh;
    public Material sphereMaterial;
    public Material springMaterial;
    public float spacing = 0.5f; // الطول الطبيعي للنابض

    private void OnRenderObject()
    {
        if (massPoints == null || springs == null || sphereMesh == null || sphereMaterial == null || springMaterial == null)
            return;

        // رسم النوابض
        springMaterial.SetPass(0); // تأكد من أن الـ Material يستخدم "Hidden/Internal-Colored"
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        foreach (var s in springs)
        {
            MassPoint a = massPoints[s.x];
            MassPoint b = massPoints[s.y];

            Vector3 delta = b.Position - a.Position;
            float currentLength = delta.magnitude;

            float stretch = Mathf.Clamp01(Mathf.Abs(currentLength - spacing) / spacing);
            Color color = GetSpringColor(stretch);

            GL.Color(color);
            GL.Vertex(a.Position);
            GL.Vertex(b.Position);
        }

        GL.End();
        GL.PopMatrix();

        // رسم الكرات بلون ثابت
        sphereMaterial.color = Color.red;
        for (int i = 0; i < massPoints.Count; i++)
        {
            sphereMaterial.SetPass(0);
            Matrix4x4 matrix = Matrix4x4.TRS(massPoints[i].Position, Quaternion.identity, Vector3.one * 0.1f);
            Graphics.DrawMeshNow(sphereMesh, matrix);
        }
    }

    private Color GetSpringColor(float t)
    {
        // من أزرق → أصفر → أحمر حسب مقدار الشد
        if (t < 0.5f)
            return Color.Lerp(Color.blue, Color.yellow, t * 2f);
        else
            return Color.Lerp(Color.yellow, Color.red, (t - 0.5f) * 2f);
    }
}
