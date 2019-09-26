using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : SingletonPersistent<PlayerInputManager>, IMoveGetter, IRotateGetter
{
    private float _horMove;
    public float HorizontalMove { get { return _horMove; } set { _horMove = value; } }

    private float _verMove;
    public float VerticalMove { get { return _verMove; } set { _verMove = value; } }

    private float _horRotate;
    public float HorizontalRotate { get { return _horRotate; } set { _horRotate = value; } }

    private float _verRotate;
    public float VerticalRotate { get { return _verRotate; } set { _verRotate = value; } }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HorizontalMove = Input.GetAxisRaw(InputList.MOVE_HORIZONTAL);
    }

    void ProcessInput()
    {

    }
}
