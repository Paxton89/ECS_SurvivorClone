using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

[BurstCompile]
public partial class CollisionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Query all entities with a CollisionComponent
        EntityQuery collisionQuery = GetEntityQuery(ComponentType.ReadWrite<CollisionComponent>(), ComponentType.ReadWrite<LocalTransform>());

        // Get all entities and their CollisionComponents
        NativeArray<Entity> entities = collisionQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<CollisionComponent> collisionComponents = collisionQuery.ToComponentDataArray<CollisionComponent>(Allocator.TempJob);
        NativeArray<LocalTransform> localTransforms = collisionQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        // Loop through entities to check for collisions and resolve them
        for (int i = 0; i < entities.Length; i++)
        {
            var collisionA = collisionComponents[i];
            var transformA = localTransforms[i];

            for (int j = i + 1; j < entities.Length; j++)
            {
                var collisionB = collisionComponents[j];
                var transformB = localTransforms[j];

                if (CheckCollision(collisionA, collisionB))
                {
                    // Resolve the collision by applying a separation force
                    ResolveCollision(ref transformA, ref transformB, collisionA, collisionB);

                    // Update the entity positions

                    // Modify collisionComponentA
                    collisionA.Position = transformA.Position;
                    collisionComponents[i] = collisionA; // Write back to the array

                    // Modify collisionComponentB
                    collisionB.Position = transformB.Position;
                    collisionComponents[j] = collisionB; // Write back to the array

                    localTransforms[i] = transformA;
                    localTransforms[j] = transformB;
                }
            }
        }

        // Update the modified components in the ECS world
        for (int i = 0; i < entities.Length; i++)
        {
            EntityManager.SetComponentData(entities[i], collisionComponents[i]);
            EntityManager.SetComponentData(entities[i], localTransforms[i]);
        }

        // Dispose of the allocated arrays
        entities.Dispose();
        collisionComponents.Dispose();
        localTransforms.Dispose();
    }

    private bool CheckCollision(CollisionComponent a, CollisionComponent b)
    {
        float3 delta = a.Position - b.Position;
        float distanceSquared = math.lengthsq(delta);
        float totalRadius = a.Radius + b.Radius;
        float totalRadiusSquared = totalRadius * totalRadius;

        return distanceSquared < totalRadiusSquared;
    }

    private void ResolveCollision(ref LocalTransform transformA, ref LocalTransform transformB, CollisionComponent a, CollisionComponent b)
    {
        float3 delta = transformA.Position - transformB.Position;
        float distance = math.length(delta);

        if (distance > 0)
        {
            // Calculate the overlap distance
            float overlap = (a.Radius + b.Radius) - distance;

            // Normalize the direction of the collision
            float3 direction = math.normalize(delta);

            // Apply a separation force proportional to the overlap distance
            float3 separation = direction * (overlap / 2);

            // Move entities apart
            transformA.Position += separation;
            transformB.Position -= separation;
        }
    }
}
