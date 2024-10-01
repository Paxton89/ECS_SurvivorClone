using Unity.Entities;

public struct HealthComponent : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;

    // Method to apply damage directly to the health
    public void ApplyDamage(float damage)
    {
        CurrentHealth -= damage;
    }

    // Method to check if the entity is dead
    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }
}
