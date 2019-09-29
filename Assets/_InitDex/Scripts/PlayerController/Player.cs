using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingEntity
{
    private PlayerController playerController;

    protected override void Start()
    {
        base.Start();

        playerController = GetComponent<PlayerController>();
    }
}
