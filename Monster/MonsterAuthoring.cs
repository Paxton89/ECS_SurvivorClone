using Unity.Entities;

using UnityEngine;

[DisallowMultipleComponent]
public class MonsterAuthoring : MonoBehaviour
{
    public float speed = 2f;
    public float collisionRadius = 0.25f;
    public float maxhealth = 1;
}

public class MonsterBaker : Baker<MonsterAuthoring>
{
    public override void Bake(MonsterAuthoring authoring)
    {
        AddComponent(new MonsterComponent {MovementSpeed = authoring.speed});
        AddComponent(new CollisionComponent { Radius = authoring.collisionRadius, Position = authoring.transform.position });
        AddComponent(new HealthComponent { MaxHealth = authoring.maxhealth, CurrentHealth = authoring.maxhealth });
        //Mark this entity as an entity prefab
        AddComponent<EntityPrefab>();
    }
}
