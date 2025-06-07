using UnityEngine;

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
}
