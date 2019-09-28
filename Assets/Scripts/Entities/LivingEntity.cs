using System.Collections;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    [SerializeField] private float _maxHealth;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    [SerializeField] private float _currentHealth;
    public float Health
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    Coroutine RegenCoroutine;


    public void Heal(float pointsToHeal)
    {
        if (Health + pointsToHeal <= MaxHealth)
            Health += pointsToHeal;

        else
            Health = MaxHealth;
    }

    public void Heal(float pointsToHeal, IDamagable targetToHeal)
    {
        if (targetToHeal.Health + pointsToHeal <= targetToHeal.MaxHealth)
            targetToHeal.Heal(pointsToHeal);
        else
            targetToHeal.Health = targetToHeal.MaxHealth;
    }

    public void RegenerateHealth(float pointToHealPerTick, float tickTime, float amountOfTicks, bool keepTryingToRegenAfterMaxHealth)
    {
        if (RegenCoroutine != null && Health < MaxHealth)
        {
            RegenCoroutine = StartCoroutine(Regenerating(pointToHealPerTick, tickTime, amountOfTicks, keepTryingToRegenAfterMaxHealth));
        }
    }

    public IEnumerator Regenerating(float pointToHealPerTick, float tickTime, float amountOfTicks, bool keepTryingToRegenAfterMaxHealth)
    {
        for (int i = 0; i < amountOfTicks; i++)
        {
            yield return new WaitForSeconds(tickTime);

            if (Health + pointToHealPerTick <= MaxHealth)
            {
                Heal(pointToHealPerTick);
            }
            else
            {
                Health = MaxHealth;

                if (keepTryingToRegenAfterMaxHealth)
                    continue;

                else
                    yield break;
            }
        }
    }


    public void RestoreAllHealth()
    {
        Health = MaxHealth;
    }


    public void DealDamage(float damageToDeal, IDamagable targetToHurt)
    {
        targetToHurt.TakeDamage(damageToDeal);
    }

    public void TakeDamage(float damageToTake)
    {
        Health -= damageToTake;

        if (Health <= 0f)
        {
            Health = 0f;
            Death();
        }
    }


    public void Death()
    {
        //TO OVERRIDE
    }
}
