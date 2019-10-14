using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterTrigger : MonoBehaviour
{
    bool alreadyEntered;

    public List<IATest> enemies;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() && !alreadyEntered)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].target = other.gameObject;
            }

            other.GetComponent<PlayerController>().playerCombatStatus = PlayerController.CombatStatus.Engaging;

            alreadyEntered = true;

            Destroy(gameObject);
        }
    }


}
