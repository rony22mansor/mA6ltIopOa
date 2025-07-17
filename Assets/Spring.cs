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
        Stiffness = stiffness;
        RestLength = Vector3.Distance(a.Position, b.Position);
    }

  
    public void SolveConstraint()
    {
        Vector3 delta = B.Position - A.Position;
        float currentLength = delta.magnitude;

        if (currentLength == 0) return;

        
        float diff = (currentLength - RestLength) / currentLength;

      
        Vector3 correction = delta * 0.5f * diff * Stiffness;

        A.ApplyCorrection(correction);
        B.ApplyCorrection(-correction);
    }
}