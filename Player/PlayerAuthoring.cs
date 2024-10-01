using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    public float speed = 5f;
    public float collisionRadius = 0.25f;
    public float maxhealth = 10;
}

public class PlayerBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        AddComponent(new PlayerComponent {MovementSpeed = authoring.speed});
        AddComponent(new CollisionComponent { Radius = authoring.collisionRadius, Position = authoring.transform.position });
        AddComponent(new HealthComponent { MaxHealth = authoring.maxhealth, CurrentHealth = authoring.maxhealth });
        AddComponent(new EquippedWeaponsComponent
        {
            // Initially, the player may have no weapons, but you can assign them later
            weapon1 = Entity.Null,
            weapon2 = Entity.Null,
            weapon3 = Entity.Null,
            weapon4 = Entity.Null,
            weapon5 = Entity.Null,
            weapon6 = Entity.Null
        });

        //Mark this entity as an entity prefab
        AddComponent<EntityPrefab>();
    }
}
//Marker component to indicate that this entity is a prefab
public struct EntityPrefab : IComponentData { }
