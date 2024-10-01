using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileComponent : IComponentData
{
    public float Speed;
    public float Damage;
    public float3 TargetPos;
    public float3 TargetDir;
    public float Radius;
    public bool IsHoming;
}
