using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// This class handles the movement of the player with given input from the input manager
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the player moves")]
    public float moveSpeed = 20f;
    [Tooltip("The speed at which the player rotates to look left and right (calculated in degrees)")]
    public float lookSpeed = 60f;
    [Tooltip("The power with which the player jumps")]
    public float jumpPower = 8f;
    [Tooltip("The strength of gravity")]
    public float gravity = 9.81f;

    [Tooltip("The falling gravity multiplier after apex of jump. This is used to reduce floatiness.")]
    public float fallingMultiplier = 1.75f;

    [Header("Jump Timing")]
    public float jumpTimeLeniency = 0.25f;

    [Header("Required References")]
    [Tooltip("The player shooter script that fires projectiles")]
    public Shooter playerShooter;   

    [Header("Input Actions")]
    [Tooltip("The bindings for moving the player")]
    public InputAction moveInput;
    [Tooltip("The binding to make the player jump")]
    public InputAction jumpInput;
    [Tooltip("The binding for making the player look left and right")]
    public InputAction lookInput;

    // The character controller component on the player
    private CharacterController controller;

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// We use this to enable the input actions
    /// </summary>
    private void OnEnable()
    {
        moveInput.Enable();
        jumpInput.Enable();
        lookInput.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// We use this to disable the input actions
    /// </summary>
    private void OnDisable()
    {
        moveInput.Disable();
        jumpInput.Disable();
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
        SetUpCharacterController();
        SetUpRigidbody();
    }

    /// <summary>
    /// Description:
    /// Set's up the character controller component for use in this script
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void SetUpCharacterController()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("The player controller script does not have a character controller on the same game object!");
        }
    }

    /// <summary>
    /// Description:
    /// Set's up the Rigidbody component for use in this script
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void SetUpRigidbody()
    {
        Rigidbody playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.useGravity = false;
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Process the movement and horizontal rotation on the player
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {
        ProcessMovement();
        ProcessHorizontalRotation();
    }

    /// <summary>
    /// Description:
    /// This processes the movement on the player based on the player inputs as well as gravity.
    /// Input:
    /// none
    /// Return:
    /// none
    /// </summary>
    /// <returns></returns>
    Vector3 moveDirection;
    float timeToStopBeingLenient = 0f; // track when to stop being lenient based on the jumpTimeLienciency
    bool doubleJumpAvailable = false; // keep track if the player can double jump - they cannot until they are grounded and jump once
    void ProcessMovement()
    {
        // Get the input from the player
        float leftRightInput = moveInput.ReadValue<Vector2>().x;
        float forwardBackwardInput = moveInput.ReadValue<Vector2>().y;
        bool jumpPressed = jumpInput.triggered;

        // Handle the control of the player while it is on the ground
        if (controller.isGrounded && moveDirection.y <= 0) // could also use RayCastGrounded instea of isGrounded
        {
            doubleJumpAvailable = true;
            timeToStopBeingLenient = Time.time + jumpTimeLeniency;

            // Set the movement direction to be the received input, set y to 0 since we are on the ground
            moveDirection = new Vector3(leftRightInput, 0, forwardBackwardInput);
            // Set the move direction in relation to the transform
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection = moveDirection * moveSpeed;

            if (jumpPressed)
            {
                moveDirection.y = jumpPower;
            }

        }
        else
        {
            moveDirection = new Vector3(leftRightInput * moveSpeed, moveDirection.y, forwardBackwardInput * moveSpeed);
            moveDirection = transform.TransformDirection(moveDirection);

            if (jumpPressed && Time.time < timeToStopBeingLenient)
            {
                moveDirection.y = jumpPower;
            }
            else if (jumpPressed && doubleJumpAvailable)
            {
                moveDirection.y = jumpPower;
                doubleJumpAvailable = false;
            }
        }

        if (controller.isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -0.3f;
        }

        // add effect of gravity
        if (isFalling())
        {
            // Mario style falling where gravity is more on fall than on jump, to make the player seem less floaty
            moveDirection.y -= gravity * fallingMultiplier * Time.deltaTime;
        }
        else
        {
            // Apply regular gravity
            moveDirection.y -= gravity * Time.deltaTime;
        }


        controller.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Description:
    /// Bounces the player upwards with some multiplier by the jump strength
    /// Input:
    /// float bounceForceMultiplier | float bounceJumpButtonHeldMultiplyer
    /// Output:
    /// void
    /// </summary>
    /// <param name="bounceForceMultiplier">The force to multiply jump strength by when bounce is called</param>
    /// <param name="bounceJumpButtonHeldMultiplyer">The force to multiply jump strength by when bounce is called and the jump button is held down</param>
    public void Bounce(float bounceForceMultiplier, float bounceJumpButtonHeldMultiplyer)
    {
        if (jumpInput.ReadValue<float>() != 0)
        {
            moveDirection.y = jumpPower * bounceJumpButtonHeldMultiplyer;
        }
        else
        {
            moveDirection.y = jumpPower * bounceForceMultiplier;
        }
    }

    /// <summary>
    /// Description:
    /// Resets the double jump of the player
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
    public void ResetJumps()
    {
        doubleJumpAvailable = true;
    }

    /// <summary>
    /// Description:
    /// Rotate the player based on the look inputs
    /// Input:
    /// none
    /// Return:
    /// none
    /// </summary>
    /// <returns></returns>
    void ProcessHorizontalRotation()
    {
        float horizontalSensitivityMultiplier = 1;
        if (PlayerPrefs.HasKey("HorizontalMouseSensitivity"))
        {
            horizontalSensitivityMultiplier = PlayerPrefs.GetFloat("HorizontalMouseSensitivity");
        }

        float horizontalLookInput = lookInput.ReadValue<Vector2>().x * horizontalSensitivityMultiplier;
        Vector3 playerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(0, playerRotation.y + horizontalLookInput * lookSpeed * Time.deltaTime, 0));
    }

    /// <summary>
    /// Description:
    /// Checks to see if player is falling (vs. going up from jump)
    /// Input:
    /// none
    /// Return:
    /// bool, whether or not the player is falling
    /// </summary>
    /// <returns></returns>
    float previousHeight; // track the previous height so you can determine when the player is falling
    bool isFalling()
    {
        bool isFalling = false;

        if (previousHeight > transform.localPosition.y)
        {
            isFalling = true;
        }

        previousHeight = transform.localPosition.y;

        return isFalling;
    }

    /// <summary>
    /// Description:
    /// Checks if the player is on the ground using raycasting.
    /// Input:
    /// none
    /// Return:
    /// bool, whether or not the player is on the ground
    /// </summary>
    /// <returns></returns>
    bool RayCastGrounded()
    {
        bool isGrounded = false;

        Debug.DrawRay(transform.position, -transform.up * 1.1f, Color.red);
        if (Physics.Raycast(transform.position, -transform.up, 1.1f))
        {
            isGrounded = true;
        }

        return isGrounded;
    }
}
