using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Define a partial struct for the PlayerMovementSystem, implementing ISystem
partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // This method is called when the system is created
        // It can be used to initialize data or state needed by the system
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        // This method is called when the system is destroyed
        // Use this to clean up any resources or state to avoid leaks
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get input from the keyboard using Unity's Input system
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float deltaTime = SystemAPI.Time.DeltaTime;  // Get the time passed since the last frame

        // Iterate over all entities that have these components
        foreach (var (playerComponent, localTransform, collisionComponent, spawnedEvent) in SystemAPI.Query<RefRO<PlayerComponent>, RefRW<LocalTransform>, RefRW<CollisionComponent>, RefRW<PlayerSpawnedEvent>>())
        {
            // Calculate the movement direction based on input
            float3 direction = new float3(horizontal, vertical, 0f);

            // Normalize the direction vector to prevent faster movement when moving diagonally
            if (math.lengthsq(direction) > 0)
            {
                direction = math.normalize(direction);
            }

            // Apply movement to the player's transform based on the calculated direction and the player's movement speed
            float3 newPosition = localTransform.ValueRW.Position + direction * playerComponent.ValueRO.MovementSpeed * deltaTime;

            // Update the player's position
            localTransform.ValueRW.Position = newPosition;

            // Update CollisionComponent.Position
            collisionComponent.ValueRW.Position = newPosition;
        }
    }
}
