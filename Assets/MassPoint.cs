using UnityEngine;

public class MassPoint
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Mass;
    public bool IsFixed;

    public MassPoint(Vector3 position, float mass, bool isFixed = false)
    {
        Position = position;
        Velocity = Vector3.zero;
        Mass = mass;
        IsFixed = isFixed;
    }

    public void ApplyForce(Vector3 force, float deltaTime)
    {
        if (IsFixed) return;

        Vector3 acceleration = force / Mass;
        Velocity += acceleration * deltaTime;
        Position += Velocity * deltaTime;
    }

    public void ApplyDamping(float damping)
    {
        Velocity *= damping;
    }
}