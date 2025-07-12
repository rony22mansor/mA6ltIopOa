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
        Vector3 direction = B.Position - A.Position;
        float currentLength = direction.magnitude;

        if (currentLength == 0) return;

        direction.Normalize();

        float delta = currentLength - RestLength;
        float correction = delta * Stiffness;

        // Distribute correction based on mass (or equally if fixed)
        if (!A.IsFixed && !B.IsFixed)
        {
            A.Position += direction * correction * 0.5f;
            B.Position -= direction * correction * 0.5f;
        }
        else if (!A.IsFixed)
        {
            A.Position += direction * correction;
        }
        else if (!B.IsFixed)
        {
            B.Position -= direction * correction;
        }
    }
}