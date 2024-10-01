
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int initialMonsterAmount = 5;
    public float spawnInterval = 10f;
    public int spawnIncrement = 6;
    public int maxMonsters = 500;
    public int monsterClumpSizeMin = 10;
    public int monsterClumpSizeMax = 20;
    public float monsterClumpSpawnRadius = 5f;
    public float spawnDistanceFromPlayer = 20f;
    public int spawnBatchSize = 1000; // Number of monsters to spawn per frame

    private EntityManager entityManager;
    private Entity playerPrefabEntity;
    private Entity weaponPrefabEntity;
    private Entity projectilePrefabEntity;
    private Entity playerEntity;

    private float spawnTimer = 0f;
    private Queue<(Entity, float3)> spawnQueue = new Queue<(Entity, float3)>(); // Queue for deferred spawning
    
    void Start()
    {
        // Get the EntityManager from the default world. EntityManager is central to ECS and manages the lifecycle of entities.
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Initial Spawning
        playerEntity = initializePlayer();
        initializeMonsters(initialMonsterAmount);
        initializeStarterWeapon(playerEntity);
        initializeProjectiles();
    }

    private Entity initializePlayer()
    {
        var playerQuery = entityManager.CreateEntityQuery(typeof(EntityPrefab), typeof(PlayerComponent));
        var playerPrefabs = playerQuery.ToEntityArray(Allocator.Temp);
        if (playerPrefabs.Length > 0)
        {
            playerPrefabEntity = playerPrefabs[0];
            if (playerPrefabEntity == Entity.Null)
            {
                Debug.LogError("Cannot spawn player -- prefab entity is null");
                playerPrefabs.Dispose();
                return Entity.Null;
            }

            // Instantiate the player entity from the prefab
            Entity playerEntity = entityManager.Instantiate(playerPrefabEntity);
            //Set Player Transform
            entityManager.SetComponentData(playerEntity, new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            // Add a PlayerSpawnedEvent component to signal that the player has been spawned
            entityManager.AddComponent<PlayerSpawnedEvent>(playerEntity);
            Debug.Log("Player entity spawned successfully.");

            playerPrefabs.Dispose();
            return playerEntity;
        }
        else
        {
            Debug.LogError("Player prefab entity not found in subscene.");
            return Entity.Null;
        }
    }

    private void initializeMonsters(int monsterAmount)
    {
        var monsterQuery = entityManager.CreateEntityQuery(typeof(EntityPrefab), typeof(MonsterComponent));
        var monsterPrefabs = monsterQuery.ToEntityArray(Allocator.Temp);

        if (monsterPrefabs.Length > 0)
        {
            var monsterType = UnityEngine.Random.Range(0, monsterPrefabs.Length); // Randomly select one type of monster
            for (int i = 0; i < monsterAmount; i++)
            {
                float3 spawnPos = new float3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), 0);
                enqueueSpawn(monsterPrefabs[monsterType], spawnPos);
            }
        }
        else
        {
            Debug.LogError("Monster prefab entity not found in subscene.");
        }

        monsterPrefabs.Dispose();
    }

    private void enqueueSpawn(Entity entity, float3 position)
    {
        spawnQueue.Enqueue((entity, position));
    }

    private void processSpawnQueue(int batchSize)
    {
        int spawnCount = math.min(batchSize, spawnQueue.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            var (entity, position) = spawnQueue.Dequeue();
            spawnMonster(entity, position);
        }
    }

    private void spawnMonster(Entity entity, float3 spawnPos)
    {
        if (entity == Entity.Null)
        {
            Debug.LogError("Cannot spawn monster -- prefab entity is null");
            return;
        }
        // Instantiate the monster entity from the prefab and set its initial position
        Entity monsterEntity = entityManager.Instantiate(entity);
        entityManager.SetComponentData(monsterEntity, new LocalTransform
        {
            Position = spawnPos,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        // Add an ActiveMonsterTag to mark the monster as active in the game
        entityManager.AddComponent<ActiveMonsterTag>(monsterEntity);
    }

    private float3 getRandomSpawnPositionOutsidePlayerView(float3 playerPosition)
    {
        float angle = UnityEngine.Random.Range(0, 2 * math.PI); // Random angle around the player
        float distance = spawnDistanceFromPlayer + UnityEngine.Random.Range(5, 10); // Distance from player

        return playerPosition + new float3(math.cos(angle), math.sin(angle), 0) * distance;
    }

    private float3 getRandomPointInRadius(float radius)
    {
        // Random point within a circle of given radius
        float angle = UnityEngine.Random.Range(0, 2 * math.PI);
        float distance = UnityEngine.Random.Range(0, radius);
        return new float3(math.cos(angle), math.sin(angle), 0) * distance;
    }

    private void spawnEnemyClumps(int clumpCount)
    {
        var playerPosition = entityManager.GetComponentData<LocalTransform>(playerEntity).Position;

            var monsterQuery = entityManager.CreateEntityQuery(
        ComponentType.ReadOnly<MonsterComponent>(), 
        ComponentType.ReadWrite<LocalTransform>(), 
        ComponentType.Exclude<ActiveMonsterTag>()
    );
        var monsterPrefabs = monsterQuery.ToEntityArray(Allocator.Temp);

        if(monsterPrefabs.Length == 0)
        {
            Debug.LogError("Monster prefab entity not found in subscene");
            return;
        }

        for (int i = 0; i < clumpCount; i++)
        {
            int monsterType = UnityEngine.Random.Range(0, monsterPrefabs.Length);
            float3 clumpCenter = getRandomSpawnPositionOutsidePlayerView(playerPosition);
            int clumpSize = UnityEngine.Random.Range(monsterClumpSizeMin, monsterClumpSizeMax);


            for (int j = 0; j < clumpSize; j++)
            {
                float3 offset = getRandomPointInRadius(monsterClumpSpawnRadius);
                enqueueSpawn(monsterPrefabs[monsterType], clumpCenter + offset);
            }
        }
        monsterPrefabs.Dispose();
    }


    private void initializeStarterWeapon(Entity playerEntity)
    {
        var weaponQuery = entityManager.CreateEntityQuery(typeof(EntityPrefab), typeof(BaseWeaponComponent));
        var weaponPrefabs = weaponQuery.ToEntityArray(Allocator.Temp);

        if (weaponPrefabs.Length > 0)
        {
            weaponPrefabEntity = weaponPrefabs[0];
            var weaponEntity = entityManager.Instantiate(weaponPrefabEntity);
            equipWeaponToPlayer(playerEntity, weaponEntity);
        }
        else
        {
            Debug.LogError("Weapon prefab entity not found in subscene.");
        }

        weaponPrefabs.Dispose();
    }

    private void equipWeaponToPlayer(Entity playerEntity, Entity weaponEntity)
    {
        if (playerEntity == Entity.Null || weaponEntity == Entity.Null)
        {
            Debug.LogError("Cannot equip weapon -- player or weapon entity is null");
            return;
        }

        //Get the players equipped weapons
        var equippedWeapons = entityManager.GetComponentData<EquippedWeaponsComponent>(playerEntity);

        //Equip to first availible slot
        equippedWeapons.weapon1 = weaponEntity;

        //Update the player's EquippedWeaponsComponent
        entityManager.SetComponentData(playerEntity, equippedWeapons);

    }

    private void initializeProjectiles()
    {
        Entity prefabSingleton = entityManager.CreateEntity();

        var projectileQuery = entityManager.CreateEntityQuery(typeof(EntityPrefab), typeof(ProjectileComponent));
        var projectilePrefabs = projectileQuery.ToEntityArray(Allocator.Temp);

        if (projectilePrefabs.Length > 0)
        {
            projectilePrefabEntity = projectilePrefabs[0];

            entityManager.AddComponentData(prefabSingleton, new ProjectilePrefab
            {
                PrefabEntity = projectilePrefabEntity
            });
        }
        else
        {
            Debug.LogError("Projectile prefab not found in subscene.");
        }

        projectilePrefabs.Dispose();
    }

    private int getActiveMonsterCount()
    {
        var activeMonsterQuery = entityManager.CreateEntityQuery(typeof(ActiveMonsterTag));
        return activeMonsterQuery.CalculateEntityCount();
    }

    void Update()
    {
        if (playerEntity == Entity.Null) return;

        // Runtime Spawning: Update monster count and spawn more if needed
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            // Check the current number of active monsters
            int activeMonsterCount = getActiveMonsterCount();

            // Spawn more monsters if below threshold
            if (activeMonsterCount < maxMonsters / 10)
            {
                int clumpCount = UnityEngine.Random.Range(1, 4);
                spawnEnemyClumps(clumpCount);
            }

            spawnTimer = 0f; // Reset the spawn timer
        }
        processSpawnQueue(spawnBatchSize);
    }
}
// Singleton component to store the projectile prefab
public struct ProjectilePrefab : IComponentData
{
    public Entity PrefabEntity;
}
