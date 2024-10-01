using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponAuthoring : MonoBehaviour
{
    public float cooldown;
    public float damage;
    public float range;
    public float projectileSpeed;
    public int projectileAmount;
}

public class WeaponBaker : Baker<WeaponAuthoring>
{
    public override void Bake(WeaponAuthoring authoring)
    {
        // Convert the MonoBehaviour into an ECS component (SpikeGunComponent in this case)
        AddComponent(new BaseWeaponComponent
        {
            Cooldown = authoring.cooldown,
        });

        AddComponent(new SpikeGunComponent
        {
            Damage              = authoring.damage,
            Range               = authoring.range,
            ProjectileSpeed     = authoring.projectileSpeed,
            ProjectileAmount    = authoring.projectileAmount
        });

        // Optionally, mark this entity as an entity prefab
        AddComponent<EntityPrefab>();
    }
}
