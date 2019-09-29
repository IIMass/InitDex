using System;
using System.Collections;

public interface IDamagable
{
    float MaxHealth { get; set; }
    float Health { get; set; }

    void DealDamage(float damageToDeal, IDamagable targetToHurt);

    void TakeDamage(float damageToTake);

    void Heal(float pointsToHeal);
    void Heal(float pointsToHeal, IDamagable targetToHeal);

    void RestoreAllHealth();

    void RegenerateHealth(float pointToHealPerTick, float tickTime, float amountOfTicks, bool keepTryingToRegenAfterMaxHealth);

    IEnumerator Regenerating(float pointToHealPerTick, float tickTime, float amountOfTicks, bool keepTryingToRegenAfterMaxHealth);

    void Death();
}
