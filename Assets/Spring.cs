// Spring.cs

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

    // CHANGED: Now uses AddForce to accumulate
    // Delete the entire ApplyForce() method and replace it with this:
    public void SolveConstraint()
    {
        Vector3 delta = B.Position - A.Position;
        float currentLength = delta.magnitude;

        if (currentLength == 0) return;

        // Calculate the difference from the rest length and how much to correct
        float diff = (currentLength - RestLength) / currentLength;

        // Use Stiffness as a multiplier (0 to 1 is a good range)
        Vector3 correction = delta * 0.5f * diff * Stiffness;

        A.ApplyCorrection(correction);
        B.ApplyCorrection(-correction);

 
    }
}