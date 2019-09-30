using UnityEngine;

public class Dummy : LivingEntity
{
    private Animator dummyAnimator;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        dummyAnimator = GetComponent<Animator>();
        OnRegenEnd = DummyRegenEnded;

        StartDummyRegen();
    }

    void DummyRegenEnded()
    {
        if (RegenCoroutine == null)
            StartDummyRegen();
    }

    void StartDummyRegen()
    {
        RegenerateHealth(5f, 1f, 10, true);
    }

    protected override void EntityHit()
    {
        dummyAnimator.SetTrigger("Hit");
    }

    protected override void Death()
    {
        EntityHit();
        RestoreAllHealth();
    }
}
