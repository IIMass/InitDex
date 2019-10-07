using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsFPS : MonoBehaviour, IPlayerAction
{
    [SerializeField] private Collider leftHand;
    [SerializeField] private Collider rightHand;
    private Collider handInUse;

    private Animator armsAnimator;

    [SerializeField] private bool _punching;
    [SerializeField] private bool _kicking;

    [SerializeField] private float damageToDeal = 5f;

    [SerializeField] private bool Fire1;
    [SerializeField] private bool Fire2;

    [Header("Combo System")]
    int[] registeredAttacks;
    int currentArrayIndex;

    public float comboEndTime;
    private float lastComboPressTime;

    public int[] leftPunch;

    // Left Combo
    public int[] leftBash;

    // Right Combo

    // Start is called before the first frame update
    void Start()
    {
        armsAnimator = GetComponent<Animator>();

        leftHand.enabled = false;
        rightHand.enabled = false;
    }

    void Update()
    {
        InputUpdate();
        Attack();
    }

    void InputUpdate()
    {
        Fire1 = Input.GetButtonDown("Fire1");
        Fire2 = Input.GetButtonDown("Fire2");
    }


    void Attack()
    {
        if (!_punching)
        {
            if (Fire1) LeftAttack();

            if (Fire2) RightAttack();
        }
    }

    public void AnimationsControllerUpdate(float controllerSpeedXZ, float maxSpeedXZ)
    {
        if (armsAnimator)
        {
            armsAnimator.SetFloat("PlayerSpeed", controllerSpeedXZ / maxSpeedXZ);
        }
    }

    public void LeftAttack()
    {
        _punching = true;
        handInUse = leftHand;

        armsAnimator.SetTrigger("LeftPunch");
    }

    public void RightAttack()
    {
        _punching = true;
        handInUse = rightHand;

        armsAnimator.SetTrigger("RightPunch");
    }

    public void AttackBegin()
    {
        handInUse.enabled = true;
    }

    public void AttackEnd()
    {
        handInUse.enabled = false;

        _punching = false;
    }

    public void OnHit(Collider handCollider, IDamagable targetHit)
    {
        handCollider.enabled = false;
        targetHit.TakeDamage(damageToDeal);

        _punching = false;
    }

    public void DisarmOrThrow()
    {
        //TODO
    }


}
