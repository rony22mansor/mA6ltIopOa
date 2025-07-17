using UnityEngine;
using System.Collections.Generic;

public class MassPoint
{
    private static int nextId = 0;
    public int UniqueId { get; private set; }

  

    public Vector3 Position;
    public Vector3 OldPosition;
    public Vector3 InitialPosition { get; private set; }
    public Vector3 Acceleration;
    public float Mass;
    public float Radius; 
    public bool IsFixed;
    public int ObjectID;

    public MassPoint(Vector3 position, float mass, float radius, bool isFixed = false, int objectId = 0)
    {
        UniqueId = nextId++;

        Position = position;
        OldPosition = position;
        Acceleration = Vector3.zero;
        InitialPosition = position;
        Mass = mass;
        IsFixed = isFixed;
        Radius = radius; 
        ObjectID = objectId;
    }

    public void Integrate(float deltaTime, float gravityY, float damping)
    {
        if (IsFixed) return;

        Acceleration = new Vector3(0, gravityY, 0);

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

    public Vector3 GetVelocity()
    {
        return Position - OldPosition;
    }
}