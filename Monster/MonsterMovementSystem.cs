using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct MonsterMovementSystem : ISystem
{
    private EntityQuery _playerQuery;

    public void OnCreate(ref SystemState state)
    {
        _playerQuery = state.GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<PlayerComponent>(), ComponentType.ReadOnly<PlayerSpawnedEvent>());
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (_playerQuery.IsEmptyIgnoreFilter)
            return;

        // Retrieve the player's position using a native array
        var playerTransforms = _playerQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        // Ensure that there is at least one player entity in the array
        if (playerTransforms.Length == 0)
        {
            playerTransforms.Dispose();
            return;
        }

        float3 playerPosition = playerTransforms[0].Position;
        playerTransforms.Dispose();

        float deltaTime = SystemAPI.Time.DeltaTime;

        // Loop through all active monsters and update their position
        foreach (var (monsterComponent, localTransform, collisionComponent, activetag) in SystemAPI.Query<RefRO<MonsterComponent>, RefRW<LocalTransform>, RefRW<CollisionComponent>, RefRW<ActiveMonsterTag>>())
        {
            float3 direction = playerPosition - localTransform.ValueRW.Position;

            if (math.lengthsq(direction) > 0)
            {
                direction = math.normalize(direction);
            }

            float3 newPosition = localTransform.ValueRW.Position + direction * monsterComponent.ValueRO.MovementSpeed * deltaTime;
            localTransform.ValueRW.Position = newPosition;

            // Update CollisionComponent.Position to sync with movement
            collisionComponent.ValueRW.Position = newPosition;
        }
    }
}
