using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct WeaponFireSystem : ISystem
{
    private Entity projectilePrefabEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        // Cleanup if necessary
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Ensure we have the projectile prefab by querying the singleton entity
        if (projectilePrefabEntity == Entity.Null)
        {
            var projectilePrefabQuery = state.EntityManager.CreateEntityQuery(typeof(ProjectilePrefab));
            if (!projectilePrefabQuery.IsEmpty)
            {
                var singletonEntity = projectilePrefabQuery.GetSingletonEntity();
                projectilePrefabEntity = state.EntityManager.GetComponentData<ProjectilePrefab>(singletonEntity).PrefabEntity;
            }
        }

        if (projectilePrefabEntity == Entity.Null) return;  // Ensure the prefab is available

        var deltaTime = SystemAPI.Time.DeltaTime;

        // Iterate over all entities with EquippedWeaponsComponent
        foreach (var (equippedWeapons, entity) in SystemAPI.Query<RefRO<EquippedWeaponsComponent>>().WithEntityAccess())
        {
            HandleWeapon(entity, equippedWeapons.ValueRO.weapon1, ref state, deltaTime);
            HandleWeapon(entity, equippedWeapons.ValueRO.weapon2, ref state, deltaTime);
            HandleWeapon(entity, equippedWeapons.ValueRO.weapon3, ref state, deltaTime);
            HandleWeapon(entity, equippedWeapons.ValueRO.weapon4, ref state, deltaTime);
            HandleWeapon(entity, equippedWeapons.ValueRO.weapon5, ref state, deltaTime);
            HandleWeapon(entity, equippedWeapons.ValueRO.weapon6, ref state, deltaTime);
        }
    }

    private void HandleWeapon(Entity entity, Entity weaponEntity, ref SystemState state, float deltaTime)
    {
        if (weaponEntity == Entity.Null) return;

        if (state.EntityManager.HasComponent<BaseWeaponComponent>(weaponEntity))
        {
            var weapon = state.EntityManager.GetComponentData<BaseWeaponComponent>(weaponEntity);

            // Reduce the cooldown timer
            weapon.CooldownTimer -= deltaTime;

            // Check if the weapon is ready to fire
            if (weapon.IsReadyToFire())
            {
                // Handle specific weapon firing
                if (state.EntityManager.HasComponent<SpikeGunComponent>(weaponEntity))
                {
                    var spikeGunComponent = state.EntityManager.GetComponentData<SpikeGunComponent>(weaponEntity);
                    var weaponPos = state.EntityManager.GetComponentData<LocalTransform>(weaponEntity).Position;
                    if (EnemiesWithinRange(entity, spikeGunComponent.Range, ref state))
                    {
                        FireSpikeGun(weaponEntity, weaponPos, ref state);
                        weapon.ResetCooldown();
                    }
                }
            }

            // Write the updated weapon component back to the entity
            state.EntityManager.SetComponentData(weaponEntity, weapon);
        }
    }

    private Entity findClosestEnemy(float3 weaponPos, ref SystemState state)
    {
        float minDistance = float.MaxValue;
        Entity closestEnemy = Entity.Null;

        foreach (var (monsterComponent, transform, entity) in SystemAPI.Query<RefRO<MonsterComponent>, RefRO<LocalTransform>>().WithAll<ActiveMonsterTag>().WithEntityAccess())
        {
            float distance = math.distance(weaponPos, transform.ValueRO.Position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = entity;
            }
        }

        return closestEnemy;
    }

        private bool EnemiesWithinRange(Entity playerEntity, float range, ref SystemState state)
    {
        var playerTransform = state.EntityManager.GetComponentData<LocalTransform>(playerEntity);
        float3 playerPosition = playerTransform.Position;

        // Iterate over all active enemies and check if they are within range
        foreach (var (monsterTransform, activeMonster) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ActiveMonsterTag>>())
        {
            float distanceToMonster = math.distance(playerPosition, monsterTransform.ValueRO.Position);

            if (distanceToMonster <= range)
            {
                return true;  // Return true as soon as we find an enemy within range
            }
        }

        return false;  // No enemies within range
    }

    private void FireSpikeGun(Entity weaponEntity, float3 weaponPos, ref SystemState state)
    {
        // Ensure we have the projectile prefab loaded
        if (projectilePrefabEntity == Entity.Null)
        {
            Debug.LogError("Projectile prefab not set!");
            return;
        }

        Entity closestEnemy = findClosestEnemy(weaponPos, ref state);
        if (closestEnemy == Entity.Null) return;

        // get closestEnemyPos
        float3 enemyPos = state.EntityManager.GetComponentData<LocalTransform>(closestEnemy).Position;

        //SpawnProjectile
        Entity projectileEntity = state.EntityManager.Instantiate(projectilePrefabEntity);
        state.EntityManager.SetComponentData(projectileEntity, new ProjectileComponent 
        {
            Speed       = state.EntityManager.GetComponentData<SpikeGunComponent>(weaponEntity).ProjectileSpeed,
            Damage      = state.EntityManager.GetComponentData<SpikeGunComponent>(weaponEntity).Damage,
            Radius      = 0.1f,
            TargetPos   = enemyPos,
            TargetDir   = math.normalize(enemyPos - weaponPos),
            IsHoming    = false
        });

        //Set Projectile Initial Pos
        state.EntityManager.SetComponentData(projectileEntity, new LocalTransform
        {
            Position    = weaponPos,
            Rotation    = quaternion.identity,
            Scale       = 1f
        });
    }
}
