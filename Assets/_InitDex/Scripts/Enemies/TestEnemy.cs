using UnityEngine;

public class TestEnemy : LivingEntity
{
    IATest thisAgent;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        thisAgent = GetComponentInParent<IATest>();
    }

    public void AttackTrack()
    {
        thisAgent.targetLookAt = true;
    }

    public void AttackUnTrack()
    {
        thisAgent.targetLookAt = false;
    }

    public void AttackStateActive()
    {
        thisAgent.arm.enabled = true;
    }

    public void AttackStateInactive()
    {
        thisAgent.arm.enabled = false;
    }


    protected override void EntityHit()
    {
        thisAgent.agentHit = true;
    }

    protected override void Death()
    {
        GetComponent<Collider>().enabled = false;
        thisAgent.agentDead = true;
    }
}