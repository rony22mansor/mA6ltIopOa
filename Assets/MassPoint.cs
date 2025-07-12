using UnityEngine;

public class MassPoint
{
    public Vector3 Position;
    public Vector3 OldPosition;
    public Vector3 InitialPosition { get; private set; }
    public Vector3 Acceleration;
    public float Mass;
    public float Radius;
    public bool IsFixed;

    public MassPoint(Vector3 position, float mass)
    {
        Position = position;
        OldPosition = position;
        Acceleration = Vector3.zero;
        InitialPosition = position;
        Mass = mass;
        IsFixed = false;
        Radius = 0.1f;
    }

    public void Integrate(float deltaTime, float gravityY, float damping)
    {
        if (IsFixed) return;

        // Apply gravity
        Acceleration = new Vector3(0, gravityY, 0);

        // Verlet integration
        Vector3 currentPosition = Position;
        Position += (Position - OldPosition) * damping + Acceleration * deltaTime * deltaTime;
        OldPosition = currentPosition;
    }

    public void ApplyCorrection(Vector3 correction)
    {
        if (!IsFixed)
        {
            Position += correction;
        }
    }
}