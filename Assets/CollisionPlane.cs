using UnityEngine;

public class CollisionPlane : MonoBehaviour
{
    public Vector3 normal = Vector3.up;
    public Vector2 size = new Vector2(10, 10); // Defines the width and length
    public Color gizmoColor = new Color(0, 1, 0, 0.5f);

    public Vector3 GetPoint()
    {
        return transform.position;
    }

    public Vector3 GetNormal()
    {
        return normal.normalized;
    }

    // Checks if a point is within our rectangular bounds
    public bool IsPointOnBounds(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        return Mathf.Abs(localPoint.x) <= size.x / 2f &&
               Mathf.Abs(localPoint.z) <= size.y / 2f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(GetPoint(), Quaternion.LookRotation(GetNormal()), new Vector3(size.x, size.y, 0.001f));
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = oldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(GetPoint(), GetNormal() * 2);
    }
}