using UnityEngine;
using System.Collections.Generic;

public class MassSpringRenderer : MonoBehaviour
{
    public List<MassPoint> massPoints;
    public List<Spring> springs;

    public Material springMaterial;
   
    public Material pointMaterial;
    public float pointSize;

    

    void OnRenderObject()
    {
        
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

        
        if (massPoints != null && pointMaterial != null)
        {
            pointMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(Color.red); 

            
            Transform camTransform = Camera.main.transform;
            Vector3 camRight = camTransform.right * pointSize;
            Vector3 camUp = camTransform.up * pointSize;

            foreach (var point in massPoints)
            {
                Vector3 p = point.Position;

               
                GL.Vertex(p - camRight - camUp);
                GL.Vertex(p + camRight - camUp);
                GL.Vertex(p + camRight + camUp); 
                GL.Vertex(p - camRight + camUp); 
            }

            GL.End();
        }
    }
}