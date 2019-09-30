using System;
using System.Collections;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable, IRegenerable
{
    [SerializeField] protected float _maxHealth;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    [SerializeField] protected float _currentHealth;
    public float Health
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    [SerializeField]
    protected Coroutine _regenCoroutine;
    public Coroutine RegenCoroutine
    {
        get { return _regenCoroutine; }
        set
        {
            if (_regenCoroutine != value)
            {
                _regenCoroutine = value;
                if (OnRegenEnd != null) OnRegenEnd();
            }
        }
    }
    protected Action OnRegenEnd;


    protected virtual void Start()
    {
        Health = MaxHealth;
    }


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
        if (RegenCoroutine == null)
        {
            if (keepTryingToRegenAfterMaxHealth)
                RegenCoroutine = StartCoroutine(Regenerating(pointToHealPerTick, tickTime, amountOfTicks, keepTryingToRegenAfterMaxHealth));

            else
            {
                if (Health < MaxHealth)
                    RegenCoroutine = StartCoroutine(Regenerating(pointToHealPerTick, tickTime, amountOfTicks, keepTryingToRegenAfterMaxHealth));
            }
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
                {
                    RegenCoroutine = null;
                    yield break;
                }
            }
        }
        RegenCoroutine = null;
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
        else
        {
            EntityHit();
        }
    }

    protected virtual void EntityHit()
    {
        //TO OVERRIDE
    }

    protected virtual void Death()
    {
        //TO OVERRIDE
    }

}
