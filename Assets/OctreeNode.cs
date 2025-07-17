using UnityEngine;
using System.Collections.Generic;

// ???? ?? ??? Octree
public class OctreeNode
{
    public Bounds nodeBounds;
    public List<MassPoint> points = new List<MassPoint>();
    public OctreeNode[] children = null;
    private float minSize;
    private const int MAX_POINTS_PER_NODE = 8;

    public OctreeNode(Bounds b, float minNodeSize)
    {
        nodeBounds = b;
        minSize = minNodeSize;
    }

    public void Insert(MassPoint point)
    {
        
        if (children == null)
        {
            points.Add(point);

            
            if (points.Count > MAX_POINTS_PER_NODE && nodeBounds.size.x > minSize)
            {
                Subdivide();
            }
        }
        else 
        {
            int index = GetChildIndex(point.Position);
            if (index != -1)
            {
                children[index].Insert(point);
            }
        }
    }

    
    private void Subdivide()
    {
        children = new OctreeNode[8];
        float childSize = nodeBounds.size.x / 2.0f;
        Vector3 parentCenter = nodeBounds.center;

        for (int i = 0; i < 8; i++)
        {
            Vector3 childCenter = parentCenter;
            childCenter.x += (i & 4) == 0 ? -childSize / 2.0f : childSize / 2.0f;
            childCenter.y += (i & 2) == 0 ? -childSize / 2.0f : childSize / 2.0f;
            childCenter.z += (i & 1) == 0 ? -childSize / 2.0f : childSize / 2.0f;
            children[i] = new OctreeNode(new Bounds(childCenter, Vector3.one * childSize), minSize);
        }

       
        List<MassPoint> pointsToMove = new List<MassPoint>(points);
        points.Clear();
        foreach (var point in pointsToMove)
        {
            int index = GetChildIndex(point.Position);
            if (index != -1)
            {
                children[index].Insert(point);
            }
        }
    }

    
    private int GetChildIndex(Vector3 pointPosition)
    {
        int index = 0;
        if (pointPosition.x > nodeBounds.center.x) index |= 4;
        if (pointPosition.y > nodeBounds.center.y) index |= 2;
        if (pointPosition.z > nodeBounds.center.z) index |= 1;
        return index;
    }

    
    public void RetrievePoints(List<MassPoint> returnPoints, Vector3 position, float radius)
    {
        
        if (!nodeBounds.Intersects(new Bounds(position, Vector3.one * radius * 2)))
        {
            return;
        }

        
        foreach (var p in points)
        {
            returnPoints.Add(p);
        }

        
        if (children != null)
        {
            foreach (var child in children)
            {
                child.RetrievePoints(returnPoints, position, radius);
            }
        }
    }

    public void DrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);

        if (children != null)
        {
            foreach (var child in children)
            {
                child.DrawGizmos();
            }
        }
    }
}

public class Octree
{
    private OctreeNode root;

    public Octree(Bounds worldBounds, float minNodeSize)
    {
        root = new OctreeNode(worldBounds, minNodeSize);
    }

    public void Insert(MassPoint point)
    {
        root.Insert(point);
    }

    public List<MassPoint> Retrieve(MassPoint point)
    {
        List<MassPoint> points = new List<MassPoint>();
        root.RetrievePoints(points, point.Position, point.Radius);
        return points;
    }

    public void DrawGizmos()
    {
        if (root != null)
        {
            root.DrawGizmos();
        }
    }
}