using System;
using System.Collections;
using UnityEngine;

public interface IDamagable
{
    float MaxHealth { get; set; }
    float Health { get; set; }

    void DealDamage(float damageToDeal, IDamagable targetToHurt);

    void TakeDamage(float damageToTake);

    void Heal(float pointsToHeal);
    void Heal(float pointsToHeal, IDamagable targetToHeal);

    void RestoreAllHealth();
}

public interface IRegenerable
{
    void RegenerateHealth(float pointToHealPerTick, float tickTime, float amountOfTicks, bool keepTryingToRegenAfterMaxHealth);

    IEnumerator Regenerating(float pointToHealPerTick, float tickTime, float amountOfTicks, bool keepTryingToRegenAfterMaxHealth);

    Coroutine RegenCoroutine { get; set; }
}

public interface IPlayerAction
{
    void LeftAttack();
    void RightAttack();

    void DisarmOrThrow();
}