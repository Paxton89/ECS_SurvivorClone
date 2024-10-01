using Unity.Entities;
using Unity.Mathematics;

public struct CollisionComponent : IComponentData
{
    public float Radius;
    public float3 Position;
}