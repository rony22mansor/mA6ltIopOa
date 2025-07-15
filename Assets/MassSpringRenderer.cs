using UnityEngine;
using System.Collections.Generic;

public class MassSpringRenderer : MonoBehaviour
{
    public List<MassPoint> massPoints;
    public List<Spring> springs;

    public Material springMaterial;
    // تمت الإضافة: مادة خاصة بالنقاط
    public Material pointMaterial;
    public float pointSize;

    // تم الحذف: لم نعد بحاجة لمادة وشبكة الكرة
    // public Material sphereMaterial;
    // public Mesh sphereMesh;

    void OnRenderObject()
    {
        // 1. رسم النوابض (بدون تغيير)
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

        // 2. رسم نقاط الكتلة باستخدام GL (الجزء المُعدَّل)
        if (massPoints != null && pointMaterial != null)
        {
            pointMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(Color.red); // يمكنك تغيير لون النقاط

            // الحصول على اتجاهات الكاميرا لجعل المربعات تواجهها
            Transform camTransform = Camera.main.transform;
            Vector3 camRight = camTransform.right * pointSize;
            Vector3 camUp = camTransform.up * pointSize;

            foreach (var point in massPoints)
            {
                Vector3 p = point.Position;

                // حساب رؤوس المربع الأربعة حول كل نقطة
                GL.Vertex(p - camRight - camUp); // الرأس السفلي الأيسر
                GL.Vertex(p + camRight - camUp); // الرأس السفلي الأيمن
                GL.Vertex(p + camRight + camUp); // الرأس العلوي الأيمن
                GL.Vertex(p - camRight + camUp); // الرأس العلوي الأيسر
            }

            GL.End();
        }
    }
}