using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct WeaponPositionSysytem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Initialize if necessary
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        // Cleanup if necessary
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Iterate over all entities with these components
        foreach (var (playerComponent, playerTransform, collisionComponent, spawnedEvent, equippedWeapons) in SystemAPI.Query<RefRO<PlayerComponent>, RefRW<LocalTransform>, RefRW<CollisionComponent>, RefRW<PlayerSpawnedEvent>, RefRO<EquippedWeaponsComponent>>())
        {
            // Sync each equipped weapon with the player's position
            SyncWeaponWithPlayer(equippedWeapons.ValueRO.weapon1, playerTransform.ValueRO.Position, ref state);
            SyncWeaponWithPlayer(equippedWeapons.ValueRO.weapon2, playerTransform.ValueRO.Position, ref state);
            SyncWeaponWithPlayer(equippedWeapons.ValueRO.weapon3, playerTransform.ValueRO.Position, ref state);
            SyncWeaponWithPlayer(equippedWeapons.ValueRO.weapon4, playerTransform.ValueRO.Position, ref state);
            SyncWeaponWithPlayer(equippedWeapons.ValueRO.weapon5, playerTransform.ValueRO.Position, ref state);
            SyncWeaponWithPlayer(equippedWeapons.ValueRO.weapon6, playerTransform.ValueRO.Position, ref state);
        }
    }

    private void SyncWeaponWithPlayer(Entity weaponEntity, float3 playerPosition, ref SystemState state)
    {
        if (weaponEntity == Entity.Null) return;
        if (state.EntityManager.HasComponent<LocalTransform>(weaponEntity))
        {
            // Set weapon's position to the player's position
            var weaponTransform = state.EntityManager.GetComponentData<LocalTransform>(weaponEntity);
            weaponTransform.Position = playerPosition;  // Sync weapon's position with the player's position

            // Update the weapon's transform component in ECS
            state.EntityManager.SetComponentData(weaponEntity, weaponTransform);
        }
    }
}
