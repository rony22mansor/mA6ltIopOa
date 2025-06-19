// MassPoint.cs

using UnityEngine;

public class MassPoint
{
    public Vector3 Position;
    public Vector3 OldPosition;      // REPLACES Velocity
    public Vector3 AccumulatedForce;
    public float Mass;
    public bool IsFixed;

    public MassPoint(Vector3 position, float mass, bool isFixed = false)
    {
        Position = position;
        OldPosition = position; // Initialize OldPosition to the current position
        AccumulatedForce = Vector3.zero;
        Mass = mass;
        IsFixed = isFixed;
    }

    // ResetForce() and AddForce() remain exactly the same.
    public void ResetForce()
    {
        AccumulatedForce = Vector3.zero;
    }

    public void AddForce(Vector3 force)
    {
        if (IsFixed) return;
        AccumulatedForce += force;
    }

    // The Integrate method is completely replaced with the Verlet formula.
    // Damping is now handled inside.
    // The Integrate method now just applies gravity.
    public void Integrate(float deltaTime, float gravity, float damping) // Add damping here
    {
        if (IsFixed) return;

        Vector3 velocity = (Position - OldPosition) * damping; // Apply damping to the velocity
        OldPosition = Position;
        Position += velocity + Vector3.up * gravity * (deltaTime * deltaTime);
    }

    // ADD this new method to handle corrections from springs
    public void ApplyCorrection(Vector3 correction)
    {
        if (IsFixed) return;
        Position += correction;
    }

    // The old ApplyDamping() method is no longer needed and can be deleted.
}