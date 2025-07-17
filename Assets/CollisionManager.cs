using UnityEngine;
using System.Collections.Generic;

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager Instance;

    private List<MassSpringSystem> deformableSystems = new List<MassSpringSystem>();
    private Octree octree;
    public float worldSize = 100f; 
    public float minOctreeNodeSize = 1.0f; 
    public bool drawOctreeGizmos = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterSystem(MassSpringSystem system)
    {
        if (!deformableSystems.Contains(system))
        {
            deformableSystems.Add(system);
        }
    }

    public void UnregisterSystem(MassSpringSystem system)
    {
        if (deformableSystems.Contains(system))
        {
            deformableSystems.Remove(system);
        }
    }

    void FixedUpdate()
    {
       
        Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one * worldSize);
        octree = new Octree(worldBounds, minOctreeNodeSize);

        foreach (var system in deformableSystems)
        {
            foreach (var mp in system.GetMassPoints())
            {
                octree.Insert(mp);
            }
        }

        
        foreach (var system in deformableSystems)
        {
            foreach (var mpA in system.GetMassPoints())
            {
                
                var potentialColliders = octree.Retrieve(mpA);

                foreach (var mpB in potentialColliders)
                {
                    
                    if (mpA.UniqueId == mpB.UniqueId) continue;

                    
                    if (mpA.UniqueId > mpB.UniqueId) continue;

                   
                    ResolveCollision(mpA, mpB, system.collisionPointRadius, system.restitution);
                }
            }
        }
    }

    private void ResolveCollision(MassPoint mpA, MassPoint mpB, float radius, float restitution)
    {
        Vector3 diff = mpB.Position - mpA.Position;
        float dist = diff.magnitude;

       
        if (dist < radius * 2 && dist > 0.0001f)
        {
            Vector3 normal = diff / dist;

           
            float overlap = (radius * 2 - dist) * 0.5f; 
            if (!mpA.IsFixed) mpA.ApplyCorrection(-normal * overlap);
            if (!mpB.IsFixed) mpB.ApplyCorrection(normal * overlap);

            
            Vector3 velA = mpA.GetVelocity();
            Vector3 velB = mpB.GetVelocity();
            Vector3 relativeVelocity = velB - velA;
            float separatingVelocity = Vector3.Dot(relativeVelocity, normal);

            
            if (separatingVelocity > 0) return;

            float impulseMagnitude = -(1 + restitution) * separatingVelocity;
            Vector3 impulse = impulseMagnitude * normal * 0.5f;

            if (!mpA.IsFixed) mpA.OldPosition -= impulse;
            if (!mpB.IsFixed) mpB.OldPosition += impulse;
        }
    }

    void OnDrawGizmos()
    {
        if (drawOctreeGizmos && octree != null)
        {
            octree.DrawGizmos();
        }
    }
}