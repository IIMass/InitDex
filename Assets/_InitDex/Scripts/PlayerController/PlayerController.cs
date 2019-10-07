using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables
#pragma warning disable 0649
    #region Controller Components
    [Header("Controller Components")]
    [SerializeField] private ArmsFPS arms;
    [SerializeField] private GameObject legs;

    private CharacterController playerCC;
    private CameraLook playerCamera;
    #endregion

    [Space(10)]

    #region Player Input
    [Header("Player Input")]
    private Vector3 _moveInput;
    #endregion

    [Space(10)]

    #region Constrains
    [Header("Constrains")]
    public bool move;
    public bool jump_dodge;
    public bool attack;
    #endregion

    #region Player States
    [Header("Player States")]
    public CombatStatus playerCombatStatus;
    public enum CombatStatus { Engaging, Peaceful}

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
    #endregion

    #region Controller Speed Values
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

    private Vector3 _lastRecordedDirection;
    #endregion

    [Space(5)]

    #region Dodge Values
    [Header("Dodge Values")]
    [SerializeField] private float _dodgeSpeed;
    [SerializeField] private float _dodgeTime;
    [SerializeField] private float _dodgeCooldown;
    [SerializeField] private bool _canDodgeAgain = true;
    #endregion

    [Space(5)]

    #region Slide and Slope Speed
    /* TO IMPLEMENT
    [Header("Sliding and Slope Speed", order = 4)]
    [SerializeField] private float _slideDeccelerationSpeed;

    [SerializeField] private float _slopeSlideDownSpeed;
    [SerializeField] private float _slopeSlideDownAccelerationSpeed;
    [SerializeField] private float _slopeSlideUpDeccelerationSpeed;
    */
    #endregion

    [Header("Actions and Events")]
    private Action onGroundedValueChange;
#pragma warning restore 0649
    #endregion

    // Start is called before the first frame update
    void Start() => StartValuesSet();

    void StartValuesSet()
    {
        // Get Components
        playerCC = GetComponent<CharacterController>();

        playerCamera = GetComponentInChildren<CameraLook>();
        // Null Exception Detection
        if (playerCamera != null && playerCamera.isActiveAndEnabled)
            PassCameraStartValues();

        else
            Debug.LogError($"{this} didn't find a Player Camera inside the Controller or the Camera script is disabled!");

        arms = GetComponentInChildren<ArmsFPS>();

        // Lock Mouse
        Cursor.lockState = CursorLockMode.Locked;

        // Set Gravity
        _gravity = Physics.gravity.y;

        // Set Actions and Events
        onGroundedValueChange = GroundCheckEvent;
    }

    void PassCameraStartValues()
    {
        playerCamera.localPlayer = this;
        playerCamera.walkSpeed = _walkSpeed;
        playerCamera.controllerMaxGroundSpeed = _runForwardSpeed;
        playerCamera.controllerMaxFallSpeed = _maxFallingSpeed;
    }


    // Update is called once per frame
    void Update()
    {
        InputUpdate();
        HandleMovement();

        CameraValuesUpdate();
        AnimationsUpdate();
    }

    #region Update Methods
    #region Controller
    void InputUpdate()
    {
        _moveInput = new Vector3 (Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

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

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (playerCombatStatus == CombatStatus.Peaceful)
                playerCombatStatus = CombatStatus.Engaging;

            else if (playerCombatStatus == CombatStatus.Engaging)
                playerCombatStatus = CombatStatus.Peaceful;

        }
    }

    // Selección de Velocidad (En Suelo y Aire)
    void SpeedSelection()
    {
        // Floats privados para asignar las aceleraciones con independencia de FPS.
        float runAcceleration   = _runAccelerationSpeed * Time.deltaTime;
        float runDecceleration  = _runDeccelerationSpeed * Time.deltaTime;

        if (playerCombatStatus == CombatStatus.Engaging)
        {
            // Si se ha detectado cualquier Input de movimiento, aplica la velocidad de andar.
            if (_moveInput != Vector3.zero)
                _currentControllerSpeedXZ = Mathf.MoveTowards(_currentControllerSpeedXZ, _runPositiveDiagonalSpeed, runAcceleration * 2);

            // Sino, decelera la velocidad del Controlador.
            else
                _currentControllerSpeedXZ = Mathf.MoveTowards(_currentControllerSpeedXZ, 0f, runDecceleration);

        }
        else if (playerCombatStatus == CombatStatus.Peaceful)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _running = false;
            }
            else
            {
                // Si el Input es mayor que 0.7, el jugador se moverá en modo Run.
                _running = (Mathf.Abs(_moveInput.x) > 0.7f || Mathf.Abs(_moveInput.z) > 0.7f) ? true : false;
            }

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
        _currentControllerSpeedXZ /= 1.5f;

        yield return new WaitForSeconds(_dodgeCooldown);

        _canDodgeAgain = true;
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
    #endregion

    void CameraValuesUpdate()
    {
        playerCamera.ControllerValuesUpdate(_moveInput, _currentControllerSpeedXZ, _currentControllerSpeedY, Grounded);
    }

    void AnimationsUpdate()
    {
        arms.AnimationsControllerUpdate(_currentControllerSpeedXZ, _runForwardSpeed);
    }
    #endregion
}