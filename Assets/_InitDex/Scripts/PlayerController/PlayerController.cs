using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
#pragma warning disable 0649
    [Header("Controller Components")]
    private CharacterController playerCC;
    private Camera playerCamera;

    //private Animator modelAnimator;

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
    public bool attack;

    [Header("Player States")]
    [SerializeField] private bool _running;
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
    [Header("Ground Speed", order = 1)]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runForwardSpeed;
    [SerializeField] private float _runPositiveDiagonalSpeed;
    [SerializeField] private float _runOtherDirectionsSpeed;

    [SerializeField] private float _runAccelerationSpeed;
    [SerializeField] private float _runDeccelerationSpeed;

    [Space(5)]

    [Header("Jump and Gravity Speed", order = 2)]
    [SerializeField] private float _jumpImpulse;
    [SerializeField] private float _maxFallingSpeed;
    [SerializeField] private float _gravity;
    [SerializeField] private float _gravityMultiplier;

    [Header("Current Speed", order = 3)]
    private float _currentControllerSpeedXZ;
    private float _currentControllerSpeedY;

    private float _currentSelectedSpeed;

    private Vector3 _lastRecordedDirection;


    [Space(5)]

    [Header("Dodge Values")]
    [SerializeField] private float _dodgeSpeed;
    [SerializeField] private float _dodgeTime;
    [SerializeField] private float _dodgeCooldown;
    [SerializeField] private bool _canDodgeAgain = true;

    [Space(5)]

    /* TO IMPLEMENT
    [Header("Sliding and Slope Speed", order = 4)]
    [SerializeField] private float _slideDeccelerationSpeed;

    [SerializeField] private float _slopeSlideDownSpeed;
    [SerializeField] private float _slopeSlideDownAccelerationSpeed;
    [SerializeField] private float _slopeSlideUpDeccelerationSpeed;
    */

    [Header("Actions and Events")]
    private Action onGroundedValueChange;
#pragma warning restore 0649

    // Start is called before the first frame update
    void Start() => StartValuesSet();

    void StartValuesSet()
    {
        // Get Components
        playerCC = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        //modelAnimator = GetComponentInChildren<SkinnedMeshRenderer>().GetComponentInParent<Animator>();

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
        HandleMovement();
    }

    void InputUpdate()
    {
        _moveInput = new Vector3 (Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        _rotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        _jumping = Input.GetButtonDown("Jump");
    }

    void HandleMovement()
    {
        MoveController();
        Gravity();

        Grounded = playerCC.isGrounded;

        if (_jumping)
        {
            if (!_dodging && Grounded)
            {
                Dodge();
            }
        }
    }

    // Selección de Velocidad (En Suelo y Aire)
    void SpeedSelection()
    {
        // Floats privados para asignar las aceleraciones con independencia de FPS.
        float runAcceleration   = _runAccelerationSpeed * Time.deltaTime;
        float runDecceleration  = _runDeccelerationSpeed * Time.deltaTime;

        // Si el Input es mayor que 0.7, el jugador se moverá en modo Run.
        _running = (Mathf.Abs(_moveInput.x) > 0.7f || Mathf.Abs(_moveInput.z) > 0.7f) ? true : false;

        // Si está corriendo...
        if (_running)
        {
            // Si está corriendo hacia Adelante...
            if (_moveInput.z > 0f)

                // Si hay Input en X, aplica la velocidad en Diagonal. Si no hay, aplica la velocidad hacia Delante.
                _currentControllerSpeedXZ = (_moveInput.x == 0f)
                                            ? Mathf.MoveTowards(_currentControllerSpeedXZ, _runForwardSpeed, runAcceleration)
                                            : Mathf.MoveTowards(_currentControllerSpeedXZ, _runPositiveDiagonalSpeed, runAcceleration);

            // Sino, si está yendo hacia Atrás o en otra dirección en X, aplica la velocidad hacia Otras Direcciones.
            else if (_moveInput.z < 0f || _moveInput.x != 0f)
                _currentControllerSpeedXZ = Mathf.MoveTowards(_currentControllerSpeedXZ, _runOtherDirectionsSpeed, runAcceleration);

            // Sino, decelera el movimiento.
            else
                _currentControllerSpeedXZ = Mathf.MoveTowards(_currentControllerSpeedXZ, 0f, runDecceleration);
        }

        // Sino está corriendo...
        else
        {
            // Si se ha detectado cualquier Input de movimiento, aplica la velocidad de andar.
            if (_moveInput != Vector3.zero)
                _currentControllerSpeedXZ = Mathf.MoveTowards(_currentControllerSpeedXZ, _walkSpeed, runAcceleration);

            // Sino, decelera la velocidad del Controlador.
            else
                _currentControllerSpeedXZ = Mathf.MoveTowards(_currentControllerSpeedXZ, 0f, runDecceleration);
        }
    }

    void MoveController()
    {
        SpeedSelection();

        if (!_dodging && _moveInput != Vector3.zero)
            _lastRecordedDirection = transform.TransformDirection(_moveInput);


        Vector3 XZMovement = _currentControllerSpeedXZ * _lastRecordedDirection;
        Vector3 YMovement = new Vector3(0f, _currentControllerSpeedY, 0f);

        if (!_dodging)
            playerCC.Move((XZMovement + YMovement) * Time.deltaTime);
        else
            playerCC.Move(((_lastRecordedDirection * _dodgeSpeed) + YMovement) * Time.deltaTime);
    }

    void Jump()
    {
        if (!Grounded)
            return;

        _jumping = true;
        _currentControllerSpeedY = _jumpImpulse;
    }

    void Dodge()
    {
        if (_canDodgeAgain)
            StartCoroutine(Dodging());
    }

    IEnumerator Dodging()
    {
        _lastRecordedDirection = (_moveInput != Vector3.zero) ? transform.TransformDirection(_moveInput) : transform.TransformDirection(Vector3.back); ;

        _dodging = true;
        _canDodgeAgain = false;

        yield return new WaitForSeconds(_dodgeTime);

        _dodging = false;
        _currentControllerSpeedXZ = 0f;

        yield return new WaitForSeconds(_dodgeCooldown);

        _canDodgeAgain = true;
    }

    void Gravity()
    {
        // If Controller is in Ground AND not Jumping, apply constant gravity
        if (Grounded && !_jumping)
            _currentControllerSpeedY = _gravity * _gravityMultiplier;

        // Else, keep adding acceleration
        else
            _currentControllerSpeedY += _gravity * _gravityMultiplier * Time.deltaTime;

        // Clamping Controller Y Speed Values to prevent high falling speeds that might cause clipping
        _currentControllerSpeedY = Mathf.Clamp(_currentControllerSpeedY, -_maxFallingSpeed, float.MaxValue);
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
            if (_currentControllerSpeedY < 0f)
            {
                _currentControllerSpeedY = 0f;
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
        if (_moveInput.z > 0f || (_moveInput.x != 0f && _moveInput.z >= 0f))
            _cameraRotationZ = Mathf.LerpAngle(playerCamera.transform.localEulerAngles.z, _cameraPitchRotationAmount * -_rotateInput.x * _currentControllerSpeedXZ / _runForwardSpeed, _cameraPitchLerpBack);
        else
            _cameraRotationZ = Mathf.LerpAngle(playerCamera.transform.localEulerAngles.z, 0f, _cameraPitchLerpBack);

        // Set Camera and Character Controller Rotation
        playerCamera.transform.localEulerAngles = new Vector3 (-_cameraRotationY, 0f, _cameraRotationZ);
        transform.localEulerAngles              = new Vector3(0f, _cameraRotationX, 0f);
    }
}