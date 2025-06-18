using UnityEngine;
using System.Collections.Generic;

public class MassSpringRenderer : MonoBehaviour
{
    public List<MassPoint> massPoints;
    public List<Spring> springs;

    public Material springMaterial;
    public Material sphereMaterial;
    public Mesh sphereMesh;

    public float pointSize = 0.1f;

    private void OnRenderObject()
    {
        if (massPoints == null || springs == null || springMaterial == null || sphereMaterial == null || sphereMesh == null)
            return;

        DrawSprings();
        DrawSpheres();
    }

    void DrawSprings()
    {
        springMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        GL.Begin(GL.LINES);

        foreach (var spring in springs)
        {
            Color color = GetSpringColor(spring);
            GL.Color(color);
            GL.Vertex(spring.A.Position);
            GL.Vertex(spring.B.Position);
        }

        GL.End();
        GL.PopMatrix();
    }

    void DrawSpheres()
    {
        for (int i = 0; i < massPoints.Count; i++)
        {
            MassPoint mp = massPoints[i];

            Matrix4x4 matrix = Matrix4x4.TRS(
                mp.Position,
                Quaternion.identity,
                Vector3.one * pointSize
            );

            sphereMaterial.SetPass(0);
            Graphics.DrawMeshNow(sphereMesh, matrix);
        }
    }

    private Color GetSpringColor(Spring spring)
    {
        float currentLength = Vector3.Distance(spring.A.Position, spring.B.Position);
        float stretchRatio = Mathf.Abs(currentLength - spring.RestLength) / spring.RestLength;

        if (stretchRatio < 0.1f)
            return Color.Lerp(Color.blue, Color.yellow, stretchRatio * 10f);
        else
            return Color.Lerp(Color.yellow, Color.red, (stretchRatio - 0.1f) * 5f);
    }
}