using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsFPS : MonoBehaviour, IPlayerAction
{
    [SerializeField] private Collider leftHand;
    [SerializeField] private Collider rightHand;
    private Collider handInUse;

    private Animator armsAnimator;

    [SerializeField] public bool _punching;
    [SerializeField] private bool _kicking;
    [SerializeField] private bool _nextAttackAvailable = true;

    [SerializeField] private float damageToDeal = 5f;


    [Header("Input")]
    [SerializeField] private bool Fire1;
    [SerializeField] private bool Fire2;

    [SerializeField] private float bufferTime;
    private float bufferActivatedTime;
    private Action InputAction;

    [Header("Combo System")]
    [SerializeField] int currentArrayIndex = 0;
    public int maxComboAttacks;

    public float comboEndTime;
    private float lastComboPressTime;

    [SerializeField] private bool comboActive;

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
        ComboManagement();
    }

    void InputUpdate()
    {
        Fire1 = Input.GetButtonDown("Fire1");
        if (Fire1)
        {
            InputAction = LeftAttack;
            bufferActivatedTime = Time.time;
        }

        Fire2 = Input.GetButtonDown("Fire2");
        if (Fire2)
        {
            InputAction = RightAttack;
            bufferActivatedTime = Time.time;
        }

        if (Time.time > bufferActivatedTime + bufferTime && InputAction != null)
            InputAction = null;
    }


    void Attack()
    {
        if (!_punching && _nextAttackAvailable)
        {
            InputAction?.Invoke();
            InputAction = null;
        }
    }

    public void AnimationsControllerUpdate(float controllerSpeedXZ, float maxSpeedXZ)
    {
        armsAnimator?.SetFloat("PlayerSpeed", controllerSpeedXZ / maxSpeedXZ);
    }

    public void LeftAttack()
    {
        _punching = true;
        handInUse = leftHand;

        if (currentArrayIndex == 0)
            armsAnimator?.SetTrigger("LeftPunch");

        else if (currentArrayIndex == 1)
            armsAnimator?.SetTrigger("LeftBash");

        else if (currentArrayIndex == 2)
            armsAnimator?.SetTrigger("LeftSlap");

        _nextAttackAvailable = false;

        UpdateComboAttacks(currentArrayIndex + 1);
    }

    public void RightAttack()
    {
        _punching = true;
        handInUse = rightHand;

        if (currentArrayIndex == 0)
            armsAnimator.SetTrigger("RightPunch");

        else if (currentArrayIndex == 1)
            armsAnimator?.SetTrigger("RightSideHook");

        else if (currentArrayIndex == 2)
            armsAnimator?.SetTrigger("RightUppercut");

        _nextAttackAvailable = false;

        UpdateComboAttacks(currentArrayIndex + 1);
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

    public void NextAttack()
    {
        _nextAttackAvailable = true;
    }

    public void OnHit(Collider handCollider, IDamagable targetHit)
    {
        handCollider.enabled = false;
        targetHit.TakeDamage(damageToDeal);

        _punching = false;
    }

    public void ComboStart()
    {
        comboActive = true;
        lastComboPressTime = Time.time;
    }

    public void ComboEnd()
    {
        UpdateComboAttacks(0);
        comboActive = false;
    }

    void ComboManagement()
    {
        if (Time.time > lastComboPressTime + comboEndTime && comboActive)
            ComboEnd();
    }

    void UpdateComboAttacks(int setArrayIndex)
    {
        if (setArrayIndex >= maxComboAttacks)
        {
            currentArrayIndex = 0;
            comboActive = false;
        }
        else
        {
            currentArrayIndex = setArrayIndex;
        }
    }

    public void DisarmOrThrow()
    {
        //TODO
    }


}
