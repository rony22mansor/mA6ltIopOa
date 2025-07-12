using UnityEngine;

public static class MassPointExtensions
{
    public static Bounds GetAABB(this MassPoint mp, float radius)
    {
        return new Bounds(mp.Position, Vector3.one * radius * 2);
    }
}
