using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("Player Input")]
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

    private void Start()
    {
        // Lock Mouse
        Cursor.lockState = CursorLockMode.Locked;

        // Clamp Camera
        _cameraClampX = _cameraOriginalClampX;
        _cameraClampY = _cameraOriginalClampY;
    }
}
