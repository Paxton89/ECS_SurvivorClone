using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;

[BurstCompile]
public partial struct ProjectileSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) {}

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get the delta time for movement calculation
        float deltaTime = SystemAPI.Time.DeltaTime;

        // Create an EntityCommandBuffer to handle structural changes like adding/removing components
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Iterate over all projectiles
        foreach (var (projectile, projectileTransform, projectileEntity) in SystemAPI.Query<RefRO<ProjectileComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            //float3 direction = projectile.ValueRO.TargetPos - projectileTransform.ValueRW.Position;
            float3 direction = projectile.ValueRO.TargetDir;

            // Move projectile towards target
            projectileTransform.ValueRW.Position += direction * projectile.ValueRO.Speed * deltaTime;
                // Check for collision with enemies
                var enemyQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CollisionComponent>(), ComponentType.ReadOnly<MonsterComponent>(), ComponentType.ReadOnly<ActiveMonsterTag>());
                var enemyEntities = enemyQuery.ToEntityArray(Allocator.TempJob);
                var enemyCollisions = enemyQuery.ToComponentDataArray<CollisionComponent>(Allocator.TempJob);

                for (int i = 0; i < enemyCollisions.Length; i++)
                {
                    var enemyCollision = enemyCollisions[i];
                    if (CheckCollision(projectileTransform.ValueRW.Position, projectile.ValueRO.Radius, enemyCollision.Position, enemyCollision.Radius))
                    {
                        var hitEnemy = enemyEntities[i];
                        var damage = state.EntityManager.GetComponentData<ProjectileComponent>(projectileEntity).Damage;
                        var healthComponent = state.EntityManager.GetComponentData<HealthComponent>(hitEnemy);
                        healthComponent.ApplyDamage(damage);

                        if (healthComponent.IsDead())
                        {
                            ecb.DestroyEntity(hitEnemy);
                        }

                        // Destroy the projectile after the collision
                        ecb.DestroyEntity(projectileEntity);
                        break;
                    }
                }

                // Dispose of the enemy collision data
                enemyCollisions.Dispose();
                enemyEntities.Dispose();
        }

        // Play back all commands and execute structural changes
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    // Simple function to check if two entities are colliding
    private bool CheckCollision(float3 positionA, float radiusA, float3 positionB, float radiusB)
    {
        float3 delta = positionA - positionB;
        float distanceSquared = math.lengthsq(delta);
        float combinedRadius = radiusA + radiusB;
        return distanceSquared < combinedRadius * combinedRadius;
    }
}
