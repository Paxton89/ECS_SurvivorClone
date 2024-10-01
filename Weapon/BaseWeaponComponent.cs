using Unity.Entities;

public struct BaseWeaponComponent : IComponentData
{
    public float Cooldown;
    public float CooldownTimer;

    public bool IsReadyToFire()
    {
        return CooldownTimer <= 0;
    }

    public void ResetCooldown()
    {
        CooldownTimer = Cooldown;
    }
}
