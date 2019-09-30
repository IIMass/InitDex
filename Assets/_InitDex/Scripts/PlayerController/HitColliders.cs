using UnityEngine;

public class HitColliders : MonoBehaviour
{
    private ArmsFPS arms;
    private Collider thisHitBox;

    private IDamagable targetHit;

    private void Start()
    {
        arms = GetComponentInParent<ArmsFPS>();
        thisHitBox = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamagable>() != null)
        {
            targetHit = other.GetComponent<IDamagable>();
            arms.OnHit(thisHitBox, targetHit);
        }
    }

}
