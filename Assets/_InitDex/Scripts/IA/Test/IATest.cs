using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;

public class IATest : MonoBehaviour
{
    [Header("Agent Components")]
    public GameObject target;
    public NavMeshAgent thisAgent;

    public GameObject agentMesh;
    public GameObject agentHeadMesh;
    public Animator meshAnimator;


    [Header("Agent States")]
    public AgentStatus agentCurrentStatus;
    public enum AgentStatus {Idle, Patrol, Alerted, Chasing, StandOff, Attacking, Defending, Hit, Dead};
    public bool agentAttacking;
    public bool agentDefending;
    public bool agentHit;
    public bool agentDead;

    public bool agentRevive;


    [Header("Agent Speed Values")]
    public float acceleration;
    public float decceleration;

    public float chaseSpeed;
    public float standoffSpeed;

    public float lookSpeed;


    [Header("Agent Vision Values")]
    public float trackDistance;
    public float currentTrackDistance;

    public float visionSphereRadius;
    public RaycastHit visionSphereHitInfo;

    public RaycastHit visionHitInfo;
    public LayerMask visionLayerMask;


    [Header("Target to Agent Values")]
    public float targetDistance;
    public float chasingStoppingDistance;
    public float standOffStoppingDistance;

    public float standOffMoveVariation;
    public float standOffOrbitSpeed;
    public float standOffPosOffset;

    public Vector3 standoffPos;

    public bool targetLookAt;


    [Header("Target Check Values")]
    public float reactTimer;
    public float reactTime;

    public float attackTimer;
    public float attackTime;

    public bool isTargetHostile;


    [Header("Target Colliders")]
    public Collider arm;


    [Header("Debug")]
    public bool debug;


    // Update is called once per frame
    void Update()
    {
        AIThink();
        AIDo();

        if (target)
        AIVisualTargetTrack();

        AnimationUpdate();
    }

    void AIThink()
    {
        switch (agentCurrentStatus)
        {
            case AgentStatus.Idle:
                {
                    if (target != null)
                    {
                        meshAnimator.SetBool("Combat", true);

                        float randomNumber = Random.Range(0f, 1f);


                        thisAgent.speed = chaseSpeed;

                        if (randomNumber <= -1f)
                        {
                            agentCurrentStatus = AgentStatus.Chasing;
                        }
                        else
                        {
                            meshAnimator.SetTrigger("Alert");
                            agentCurrentStatus = AgentStatus.Alerted;
                        }                                              
                    }
                    else
                    {
                        meshAnimator.SetBool("Combat", false);
                    }

                    break;
                }

            case AgentStatus.Patrol:
                break;

            case AgentStatus.Alerted:

                if (!AnimationIsPlaying(0, "IdleCombat"))
                {
                    agentCurrentStatus = AgentStatus.Chasing;
                }

                break;

            case AgentStatus.Chasing:
                {
                    if (target)
                    {
                        if (targetDistance < chasingStoppingDistance)
                        {
                            thisAgent.acceleration = decceleration;
                            thisAgent.speed = standoffSpeed;
                            thisAgent.stoppingDistance = standOffStoppingDistance;

                            attackTime = Time.time;
                            agentCurrentStatus = AgentStatus.StandOff;
                        }
                    }
                    else
                    {
                        agentCurrentStatus = AgentStatus.Idle;
                    }

                    break;
                }

            case AgentStatus.StandOff:
                {
                    if (target)
                    {
                        AICheckTarget();

                        if (targetDistance >= chasingStoppingDistance)
                        {
                            thisAgent.acceleration = acceleration;
                            thisAgent.speed = chaseSpeed;
                            thisAgent.stoppingDistance = chasingStoppingDistance;
                            agentCurrentStatus = AgentStatus.Chasing;
                        }
                    }
                    else
                    {
                        agentCurrentStatus = AgentStatus.Idle;
                    }

                    break;
                }

            case AgentStatus.Attacking:

                if (!AnimationIsPlaying(0, "Cross Punch"))
                {
                    agentAttacking = false;
                    attackTime = Time.time;
                    agentCurrentStatus = AgentStatus.StandOff;
                }
                else
                {
                    if (targetLookAt)
                    {
                        AILookAt();
                    }
                }

                break;

            case AgentStatus.Defending:

                if (!AnimationIsPlaying(0, "Body Block"))
                {
                    attackTime = Time.time;
                    agentDefending = false;
                    agentCurrentStatus = AgentStatus.StandOff;
                }
                break;

            case AgentStatus.Hit:
                if (!AnimationIsPlaying(0, "Reaction"))
                {
                    attackTime = Time.time;
                    agentCurrentStatus = AgentStatus.StandOff;
                }
                break;

            case AgentStatus.Dead:

                if (agentRevive)
                {
                    agentRevive = false;
                    agentDead = false;
                    agentCurrentStatus = AgentStatus.Idle;
                }

                break;

        }

        // Constant Update
        if (target)
        {
            targetDistance = (target) ? (target.transform.position - transform.position).magnitude : 0f;
        }


        // Override States
        if (agentHit)
        {
            agentCurrentStatus = AgentStatus.Hit;
        }

        if (agentDead)
        {
            agentCurrentStatus = AgentStatus.Dead;
        }
    }

    void AIDo()
    {
        switch (agentCurrentStatus)
        {
            case AgentStatus.Idle:
                break;

            case AgentStatus.Patrol:
                break;

            case AgentStatus.Alerted:

                AIAlerted();

                break;

            case AgentStatus.Chasing:

                AIChase();

                break;

            case AgentStatus.StandOff:

                AIStandOff();

                break;

            case AgentStatus.Attacking:

                AIAttack();

                break;

            case AgentStatus.Defending:

                AIDefend();

                break;

            case AgentStatus.Hit:

                AIHit();

                break;

            case AgentStatus.Dead:

                AIDead();

                break;
        }
    }

    void AIChase()
    {
        thisAgent?.SetDestination(target.transform.position);
    }

    void AILookAt()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion targetLook = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z), Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetLook, lookSpeed * Time.deltaTime);
    }

    void AIStandOff()
    {
        standoffPos = (Quaternion.Euler(0f, standOffMoveVariation, 0f) * target.transform.InverseTransformDirection(target.transform.forward)) * standOffPosOffset;

        standOffMoveVariation += standOffOrbitSpeed * Time.deltaTime;
        if (standOffMoveVariation >= 360)
        {
            standOffMoveVariation = 0f;
        }

        thisAgent?.SetDestination(target.transform.position + standoffPos);

        AILookAt();
    }

    void AICheckTarget()
    {
        ArmsFPS playerArms = target.GetComponentInChildren<ArmsFPS>();
        isTargetHostile = playerArms._punching;

        //if (!isTargetHostile)
        //{
            if (Time.time >= attackTime + attackTimer)
            {
                agentCurrentStatus = AgentStatus.Attacking;
            }
        //}
        /*else
        {
            reactTime = Time.time;
            agentCurrentStatus = AgentStatus.Defending;
        }*/
    }

    void AIVisualTargetTrack()
    {
        if (Physics.SphereCast(agentHeadMesh.transform.position, visionSphereRadius, (target.transform.position - agentHeadMesh.transform.position).normalized, out visionSphereHitInfo, trackDistance, visionLayerMask, QueryTriggerInteraction.Ignore))
        {
            currentTrackDistance = visionSphereHitInfo.distance;
        }
        else
        {
            currentTrackDistance = trackDistance;
        }
    }


    void AIAlerted()
    {
        AILookAt();
    }

    void AIAttack()
    {
        if (!agentAttacking)
        {
            agentAttacking = true;
            meshAnimator.SetTrigger("Attack");
        }
    }

    void AIDefend()
    {
        if (Time.time >= reactTime + reactTimer && !agentDefending)
        {
            agentDefending = true;
            meshAnimator.SetTrigger("Defend");
        }
    }

    void AIHit()
    {
        if (agentHit)
        {
            agentHit = false;
            meshAnimator.SetTrigger("Hit");
        }
    }

    void AIDead()
    {
        if (agentDead)
        {
            agentDead = false;
            agentRevive = false;
            meshAnimator.SetTrigger("Dead");
        }
    }


    void AnimationUpdate()
    {
        meshAnimator?.SetFloat("SpeedBlend", (float)thisAgent?.velocity.magnitude / (float)thisAgent?.speed);
    }

    bool AnimationIsPlaying(int animLayer, string stateName)
    {
        if (meshAnimator.GetCurrentAnimatorStateInfo(animLayer).IsName(stateName) && meshAnimator.GetCurrentAnimatorStateInfo(animLayer).normalizedTime < 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (debug && target)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(agentHeadMesh.transform.position, agentHeadMesh.transform.position + ((target.transform.position - agentHeadMesh.transform.position).normalized * trackDistance));

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(agentHeadMesh.transform.position + ((target.transform.position - agentHeadMesh.transform.position).normalized * currentTrackDistance), visionSphereRadius);
            Gizmos.DrawLine(agentHeadMesh.transform.position, agentHeadMesh.transform.position + ((target.transform.position - agentHeadMesh.transform.position).normalized * currentTrackDistance));

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(target.transform.position + standoffPos, visionSphereRadius);
        }
    }
}
