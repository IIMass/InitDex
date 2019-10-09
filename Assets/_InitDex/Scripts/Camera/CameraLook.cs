using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
#pragma warning disable 0649
    [Header("Camera Components")]
    [HideInInspector] public PlayerController localPlayer;
    [HideInInspector] public Camera localCamera;

    [Header("Player Input")]
    private Vector2 _rotateInput;

    [Header("Constrains")]
    public bool rotate;
    public bool headbob;

    #region Camera Values
    [Header("Camera Values")]
    /*[SerializeField] private bool targetLocking;
    [SerializeField] private GameObject targetToLock;*/

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

    // CAMERA TRANSFORM
    private Vector3 _originalCameraPos;
    #endregion

    #region Player Controller Values
    [Header("Player Controller Values")]
    private Vector3 _controllerMoveInput;

    public float _controllerCurrentSpeedY;

    [SerializeField] private float _controllerCurrentSpeedXZ;
    public float ControllerCurrentSpeedXZ
    {
        get { return _controllerCurrentSpeedXZ; }
        set
        {
            if (_controllerCurrentSpeedXZ != value)
            {
                _controllerCurrentSpeedXZ = value;
                OnControllerSpeedChange?.Invoke();
            }
        }
    }

    [SerializeField] private bool _controllerIsGrounded;
    public bool ControllerIsGrounded
    {
        get { return _controllerIsGrounded; }
        set
        {
            if (_controllerIsGrounded != value)
            {
                _controllerIsGrounded = value;
                OnControllerIsGroundedChange?.Invoke();
            }
        }

    }

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float controllerMaxGroundSpeed;
    public float controllerMaxFallSpeed;

    private Action OnControllerSpeedChange;
    private Action OnControllerIsGroundedChange;

    #endregion

    #region Head Bobbing
    [Header("Head Bobbing")]
    [SerializeField] private HeadBobData currentHeadBob;

    [SerializeField] private HeadBobData idleBob;
    [SerializeField] private HeadBobData walkBob;
    [SerializeField] private HeadBobData runBob;
    [SerializeField] private HeadBobData landingBob;

    [SerializeField] private float cycle;
    [SerializeField] private float smoothBobLerp;

    [SerializeField] private bool transitionToNormalHeadbob;
    #endregion

    #region Camera Shake
    [Header("Camera Shake")]
    private Vector3 shakeToApply;
    #endregion
#pragma warning restore 0649


    // Start is called before the first frame update
    void Start()
    {
        _originalCameraPos = transform.localPosition;

        // Clamp Camera
        _cameraClampX = _cameraOriginalClampX;
        _cameraClampY = _cameraOriginalClampY;

        ControllerSpeedChangeAction();
        ControllerAirGroundAction();

        OnControllerSpeedChange = ControllerSpeedChangeAction;
        OnControllerIsGroundedChange = ControllerAirGroundAction;
    }


    // Update is called once per frame
    void Update()
    {
        InputUpdate();

        HeadBob();
    }

    void InputUpdate()
    {
        _rotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
    }

    public void ControllerValuesUpdate(Vector3 controllerInput, float currentControllerSpeedXZ, float currentControllerSpeedY, bool controllerIsGrounded)
    {
        _controllerMoveInput = controllerInput;
        ControllerCurrentSpeedXZ = currentControllerSpeedXZ;
        _controllerCurrentSpeedY = currentControllerSpeedY;
        ControllerIsGrounded = controllerIsGrounded;
    }


    private void LateUpdate() => CameraRotate();

    void CameraRotate()
    {
        // X Rotation
        _cameraRotationX = Mathf.Clamp(_cameraRotationX + _rotateInput.x, _cameraOriginalClampX.x, _cameraOriginalClampX.y);

        if (_cameraRotationX >= 360f || _cameraRotationX <= -360f)
            _cameraRotationX = 0f;

        // Y Rotation
        _cameraRotationY = Mathf.Clamp(_cameraRotationY + _rotateInput.y, _cameraClampY.x, _cameraClampY.y);

        // Z Rotation
        if (_controllerMoveInput.z > 0f || (_controllerMoveInput.x != 0f && _controllerMoveInput.z >= 0f))
            _cameraRotationZ = Mathf.LerpAngle(transform.localEulerAngles.z, _cameraPitchRotationAmount * -_rotateInput.x * _controllerCurrentSpeedXZ / controllerMaxGroundSpeed, _cameraPitchLerpBack);
        else
            _cameraRotationZ = Mathf.LerpAngle(transform.localEulerAngles.z, 0f, _cameraPitchLerpBack);

        // Set Camera and Character Controller Rotation
        transform.localEulerAngles = new Vector3(-_cameraRotationY, 0f, _cameraRotationZ);
        localPlayer.transform.localEulerAngles = new Vector3(0f, _cameraRotationX, 0f);
    }

    void HeadBob()
    {
        if (transitionToNormalHeadbob)
        {
            float headBobSpeedPercentage;

            if (currentHeadBob == idleBob)
            {
                headBobSpeedPercentage = 1f;
            }
            else if (currentHeadBob == walkBob)
            {
                headBobSpeedPercentage = ControllerCurrentSpeedXZ / walkSpeed;
            }
            else if (currentHeadBob == runBob)
            {
                headBobSpeedPercentage = ControllerCurrentSpeedXZ / controllerMaxGroundSpeed;
            }
            else
            {
                Debug.LogWarning("Unknown HeadBob!");
                headBobSpeedPercentage = 1f;
            }


            Vector3 camBob = _originalCameraPos + currentHeadBob.DoHeadBob(cycle) * headBobSpeedPercentage;

            cycle += (currentHeadBob.speedCurve * Time.deltaTime) / currentHeadBob.bobInterval;
            if (cycle > currentHeadBob.headbobHorizontalCurve[currentHeadBob.headbobHorizontalCurve.length - 1].time)
                cycle = 0f;

            transform.localPosition = Vector3.Lerp(transform.localPosition, camBob, smoothBobLerp * Time.deltaTime);
        }
        else
        {
            if (ControllerIsGrounded)
            {
                Vector3 camBob = _originalCameraPos + currentHeadBob.DoHeadBob(cycle) * (Mathf.Abs(_controllerCurrentSpeedY) / Mathf.Abs(controllerMaxFallSpeed));

                cycle += (currentHeadBob.speedCurve * Time.deltaTime) / currentHeadBob.bobInterval;
                if (cycle > currentHeadBob.headbobHorizontalCurve[currentHeadBob.headbobHorizontalCurve.length - 1].time)
                {
                    transitionToNormalHeadbob = true;
                    ControllerSpeedChangeAction();
                    cycle = 0f;
                }

                transform.localPosition = Vector3.Lerp(transform.localPosition, camBob, smoothBobLerp * Time.deltaTime);
            }
        }
    }


    // ACTIONS

    void ControllerSpeedChangeAction()
    {
        if (transitionToNormalHeadbob)
        {
            if (ControllerCurrentSpeedXZ > walkSpeed)
            {
                currentHeadBob = runBob;
            }
            else if (ControllerCurrentSpeedXZ > 0f)
            {
                currentHeadBob = walkBob;
            }
            else if (ControllerCurrentSpeedXZ == 0f)
            {
                currentHeadBob = idleBob;
            }
        }
    }

    void ControllerAirGroundAction()
    {
        if (ControllerIsGrounded)
        {
            currentHeadBob = landingBob;
            cycle = 0f;
        }

        transitionToNormalHeadbob = false;
    }
}