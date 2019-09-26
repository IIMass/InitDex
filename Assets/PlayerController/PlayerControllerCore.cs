using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerCore : MonoBehaviour
{
    [Header("Controller Components")]
    private CharacterController playerCC;

    private Camera playerCamera;

    [Space(10)]

    [Header("Player Input")]
    private Vector3 _moveInput;
    private Vector2 _rotateInput;

    [Space(10)]

    [Header("Camera Values")]
    // Default Axis Clamp. Vector2.x = Min | Vector2.y = Max
    [SerializeField] private Vector2 _cameraOriginalClampX;
    [SerializeField] private Vector2 _cameraOriginalClampY;

    // Actual Camera Clamp
    private Vector2 _cameraClampX;
    private Vector2 _cameraClampY;

    [Space(10)]

    // Pitch (Z Axis) Camera Rotation
    [SerializeField] private float _cameraPitchRotationAmount;
    private const float _cameraPitchLerpBack = .1f;

    // EULER ANGLES
    private float _cameraRotationX;
    private float _cameraRotationY;
    private float _cameraRotationZ;  

    [Space(10)]

    [Header("Constrains")]
    public bool move;
    public bool rotate;
    public bool jump_dodge;
    public bool crouch;
    public bool attack;
    public bool disarm_throw;
    public bool sheathe_unsheathe;

    [Header("Player States")]
    [SerializeField] private bool _jumping;
    [SerializeField] private bool _dodging;
    [SerializeField] private bool _grounded;

    protected bool Grounded
    {
        get
        {
            return _grounded;
        }
        set
        {
            if (value != _grounded)
            {
                _grounded = value;
                onGroundedValueChange();
            } 
        }
    }

    [Header("Controller Speed Values", order = 0)]
    [Header("Current Speed", order = 1)]
    [SerializeField] private float _currentControllerXZSpeed;
    [SerializeField] private float _currentControllerYSpeed;
    [SerializeField] private float _currentSelectedSpeed;

    [Space(5)]

    [Header("Ground Speed", order = 2)]
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _runAccelerationForwardSpeed;
    [SerializeField] private float _runAccelerationSidesSpeed;
    [SerializeField] private float _runDeccelerationSpeed;

    [Space(5)]

    [Header("Sliding and Slope Speed", order = 2)]
    [SerializeField] private float _slideDeccelerationSpeed;

    [SerializeField] private float _slopeSlideDownSpeed;
    [SerializeField] private float _slopeSlideDownAccelerationSpeed;
    [SerializeField] private float _slopeSlideUpDeccelerationSpeed;

    [Space(5)]

    [Header("Jump and Gravity Speed", order = 3)]
    [SerializeField] private float _jumpImpulse;
    [SerializeField] private float _maxFallingSpeed;
    [SerializeField] private float _gravity;
    [SerializeField] private float _gravityMultiplier;

    [Header("Actions and Events")]
    private Action onGroundedValueChange;

    // Start is called before the first frame update
    void Start() => StartValuesSet();

    void StartValuesSet()
    {
        // Get Components
        playerCC = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Lock Mouse
        Cursor.lockState = CursorLockMode.Locked;

        // Clamp Camera
        _cameraClampX = _cameraOriginalClampX;
        _cameraClampY = _cameraOriginalClampY;

        // Set Gravity
        _gravity = Physics.gravity.y;

        // Set Actions and Events
        onGroundedValueChange = GroundCheckEvent;
    }

    // Update is called once per frame
    void Update()
    {
        InputUpdate();

        MoveController();
        Gravity();

        Grounded = playerCC.isGrounded;
    }

    void InputUpdate()
    {
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        _rotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        if (Input.GetButtonDown("Jump")) Jump();
    }

    void MoveController()
    {
        Vector3 XZMovement = transform.TransformDirection(_runSpeed * _moveInput);
        Vector3 YMovement = new Vector3(0f, _currentControllerYSpeed, 0f);

        playerCC.Move ((XZMovement + YMovement) * Time.deltaTime);
    }

    void Jump()
    {
        if (!Grounded)
            return;

        _jumping = true;
        _currentControllerYSpeed = _jumpImpulse;
    }

    void Gravity()
    {
        // If Controller is in Ground AND not Jumping, apply constant gravity
        if (Grounded && !_jumping)
            _currentControllerYSpeed = _gravity * _gravityMultiplier;

        // Else, keep adding acceleration
        else
            _currentControllerYSpeed += _gravity * _gravityMultiplier * Time.deltaTime;

        // Clamping Controller Y Speed Values to prevent high falling speeds that might cause clipping
        _currentControllerYSpeed = Mathf.Clamp(_currentControllerYSpeed, -_maxFallingSpeed, float.MaxValue);
    }

    void GroundCheckEvent()
    {
        // If the Controller landed on the ground, do....
        if (Grounded)
        {
            _jumping = false;
        }

        // Else if the Controller has left the ground, do....
        else
        {
            // If statement done to prevent Jump Force not being applied
            if (_currentControllerYSpeed < 0f)
            {
                _currentControllerYSpeed = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        CameraLook();
    }

    void CameraLook()
    {
        // X Rotation
        _cameraRotationX = Mathf.Clamp(_cameraRotationX + _rotateInput.x, _cameraOriginalClampX.x, _cameraOriginalClampX.y);

        if (_cameraRotationX >= 360f || _cameraRotationX <= -360f)
            _cameraRotationX = 0f;

        // Y Rotation
        _cameraRotationY = Mathf.Clamp(_cameraRotationY + _rotateInput.y, _cameraClampY.x, _cameraClampY.y);

        // Z Rotation
        if (_moveInput.z > 0f)
            _cameraRotationZ = Mathf.LerpAngle(playerCamera.transform.localEulerAngles.z, _cameraPitchRotationAmount * -_rotateInput.x * _moveInput.z, _cameraPitchLerpBack);
        else
            _cameraRotationZ = Mathf.LerpAngle(playerCamera.transform.localEulerAngles.z, 0f, _cameraPitchLerpBack);

        // Set Camera and Character Controller Rotation
        playerCamera.transform.localEulerAngles = new Vector3 (-_cameraRotationY, 0f, _cameraRotationZ);
        transform.localEulerAngles              = new Vector3(0f, _cameraRotationX, 0f);
    }

}
