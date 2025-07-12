using UnityEngine;
using System.Collections.Generic;

public class MassSpringRenderer : MonoBehaviour
{
    public List<MassPoint> massPoints;
    public List<Spring> springs;

    public Material springMaterial;
    public Material sphereMaterial;
    public Mesh sphereMesh;
    public float pointSize;

    void OnRenderObject()
    {
        // Draw Springs
        if (springs != null && springMaterial != null)
        {
            springMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            foreach (var spring in springs)
            {
                GL.Vertex(spring.A.Position);
                GL.Vertex(spring.B.Position);
            }
            GL.End();
        }

        // Draw Mass Points (as spheres)
        if (massPoints != null && sphereMaterial != null && sphereMesh != null)
        {
            foreach (var point in massPoints)
            {
                Graphics.DrawMesh(
                    sphereMesh,
                    point.Position,
                    Quaternion.identity,
                    sphereMaterial,
                    0
                );
            }
        }
    }
}