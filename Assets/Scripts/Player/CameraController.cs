using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class uses processed input from the input manager to control the vertical rotation of the camera
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The camera to have control over")]
    public Camera controlledCamera;
    [Tooltip("The speed at which the camera rotates to look up and down (calculated in degrees)")]
    public float rotationSpeed = 60f;
    [Tooltip("Whether or not to invert the look direction")]
    public bool invert = true;

    [Header("Input")]
    [Tooltip("The input for looking around")]
    public InputAction lookInput;

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    private void OnEnable()
    {
        lookInput.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    private void OnDisable()
    {
        lookInput.Disable();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        SetUpCamera();
    }

    // Wait this many frames before starting to process the camera rotation
    int waitForFrames = 3;
    int framesWaited = 0;

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {
        // Wait so many frames to avoid startup camera movement bug
        if (framesWaited <= waitForFrames)
        {
            framesWaited += 1;
            return;
        }
        ProcessVerticalRotation();
        // NOTE: Horizontal rotation is handled within the PlayerController script.
    }

    /// <summary>
    /// Description:
    /// Sets up the camera component if not already donw
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void SetUpCamera()
    {
        if (controlledCamera == null)
        {
            controlledCamera = GetComponent<Camera>();
        }
    }

    /// <summary>
    /// Description:
    /// Process the vertical look input to rotate the player accordingly
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void ProcessVerticalRotation()
    {
        float verticalSensitivityMultiplier = 1;
        if (PlayerPrefs.HasKey("VerticalMouseSensitivity"))
        {
            verticalSensitivityMultiplier = PlayerPrefs.GetFloat("VerticalMouseSensitivity");
        }

        float verticalLookInput = lookInput.ReadValue<Vector2>().y * verticalSensitivityMultiplier;

        Vector3 cameraRotation = controlledCamera.transform.rotation.eulerAngles;
        float newXRotation = 0;
        if (invert)
        {
            newXRotation  = cameraRotation.x - verticalLookInput * rotationSpeed * Time.deltaTime;
        }
        else
        {
            newXRotation = cameraRotation.x + verticalLookInput * rotationSpeed * Time.deltaTime;
        }

        // clamp the rotation 360 - 270 is up 0 - 90 is down
        // Because of the way eular angles work with Unity's rotations we have to act differently when clamping the rotation
        if (newXRotation < 270 && newXRotation >= 180)
        {
            newXRotation = 270;
        }
        else if (newXRotation > 90 && newXRotation < 180)
        {
            newXRotation = 90;
        }
        controlledCamera.transform.rotation = Quaternion.Euler(new Vector3(newXRotation, cameraRotation.y, cameraRotation.z));
    }
}
