using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileAuthoring : MonoBehaviour
{
    [NonSerialized] public float speed;
    [NonSerialized] public float damage;
    [NonSerialized] public float radius;
}

public class ProjectileBaker : Baker<ProjectileAuthoring>
{
    public override void Bake(ProjectileAuthoring authoring)
    {
        // Add the ProjectileComponent with data from the MonoBehaviour
        AddComponent(new ProjectileComponent
        {
            Speed = 0,
            Damage = 0,
            Radius = 0,
            IsHoming = false
        });

        // Optionally, mark this as a prefab
        AddComponent<EntityPrefab>();
    }
}
