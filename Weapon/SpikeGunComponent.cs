using Unity.Entities;
using Unity.Mathematics;

public struct SpikeGunComponent : IComponentData
{
    public float Damage;
    public float Range;
    public float ProjectileSpeed;
    public int ProjectileAmount;
}
