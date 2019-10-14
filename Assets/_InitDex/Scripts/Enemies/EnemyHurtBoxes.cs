using UnityEngine;

public class EnemyHurtBoxes : MonoBehaviour
{
    private TestEnemy enemy;
    private Collider thisHitBox;

    private IDamagable targetHit;

    private void Start()
    {
        enemy = GetComponentInParent<TestEnemy>();
        thisHitBox = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamagable>() != null)
        {
            targetHit = other.GetComponent<IDamagable>();
            enemy.DealDamage(5f, targetHit);
            enemy.AttackStateInactive();
        }
    }

}