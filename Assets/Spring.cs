using UnityEngine;
using System.Collections.Generic;

public class Spring
{
    public MassPoint A;
    public MassPoint B;
    public float RestLength;
    public float Stiffness;

    public Spring(MassPoint a, MassPoint b, float stiffness)
    {
        A = a;
        B = b;
        RestLength = Vector3.Distance(a.Position, b.Position);
        Stiffness = stiffness;
    }

    public void ApplyForce()
    {
        Vector3 delta = B.Position - A.Position;
        float currentLength = delta.magnitude;
        float displacement = currentLength - RestLength;
        Vector3 force = Stiffness * displacement * delta.normalized;

        A.ApplyForce(force, Time.deltaTime);
        B.ApplyForce(-force, Time.deltaTime);
    }

    public static bool Exists(List<Spring> springs, MassPoint a, MassPoint b)
    {
        foreach (var s in springs)
        {
            if ((s.A == a && s.B == b) || (s.A == b && s.B == a))
                return true;
        }
        return false;
    }
}
