using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IATest : MonoBehaviour
{
    public GameObject target;
    public NavMeshAgent thisAgent;
    public Animator meshAnimator;

    public float acceleration;
    public float decceleration;
    public float lookSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        thisAgent?.SetDestination(target.transform.position);

        if (thisAgent?.remainingDistance >= thisAgent?.stoppingDistance)
        {
            thisAgent.acceleration = acceleration;
        }
        else
        {
            thisAgent.acceleration = decceleration;

            Vector3 direction = (target.transform.position - transform.position).normalized;
            Quaternion targetLook = Quaternion.LookRotation(new Vector3 (direction.x, 0f, direction.z), Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetLook, lookSpeed * Time.deltaTime);

        }

        meshAnimator?.SetFloat("SpeedBlend", (float)thisAgent?.velocity.magnitude / (float)thisAgent?.speed);
    }
}
