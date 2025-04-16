using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// This class handles the movement of the player with support for jumping, double-jumping,
/// crouching (with headroom checks), and a slide mechanic. While sliding, the player is forced
/// into a crouched state and moves in the input direction (or forward by default) at high speed,
/// and can receive a jump boost if timed as the slide ends. Additionally, a slide cooldown prevents spamming.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the player moves")]
    public float moveSpeed = 20f;
    [Tooltip("The speed at which the player rotates to look left/right (degrees)")]
    public float lookSpeed = 60f;
    
    [Header("Crouch Settings")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    [Tooltip("Movement speed when crouching (only when grounded)")]
    public float crouchSpeed = 10f;
    
    [Header("Slide Settings")]
    [Tooltip("Slide speed (applied during a slide)")]
    public float slideSpeed = 30f;
    [Tooltip("Duration of a slide in seconds")]
    public float slideDuration = 0.75f;
    [Tooltip("Cooldown time between slides (in seconds)")]
    public float slideCooldown = 1.5f;
    private bool isSliding = false;
    private float slideEndTime = 0f;
    private float lastSlideTime = 0f;
    
    [Header("Camera Settings")]
    [Tooltip("Reference to the player camera (child of the player)")]
    public Transform playerCamera;
    [Tooltip("Camera's local Y position when standing")]
    public float standingCameraHeight = 0.6f;
    [Tooltip("Camera's local Y position when crouching")]
    public float crouchingCameraHeight = 0.3f;
    [Tooltip("Speed at which the camera transitions between heights")]
    public float cameraCrouchSpeed = 5f;
    
    [Header("Jump & Gravity Settings")]
    [Tooltip("The power with which the player jumps")]
    public float jumpPower = 8f;
    [Tooltip("The strength of gravity")]
    public float gravity = 9.81f;
    [Tooltip("Falling gravity multiplier to reduce floatiness")]
    public float fallingMultiplier = 1.75f;
    
    [Header("Jump Timing")]
    public float jumpTimeLeniency = 0.25f;
    
    [Header("Required References")]
    [Tooltip("The player shooter script")]
    public Shooter playerShooter;
    
    [Header("Input Actions")]
    [Tooltip("Input for moving the player")]
    public InputAction moveInput;
    [Tooltip("Input for jump")]
    public InputAction jumpInput;
    [Tooltip("Input for look (horizontal)")]
    public InputAction lookInput;
    [Tooltip("Input for crouch (toggle)")]
    public InputAction crouchInput;
    [Tooltip("Input for slide")]
    public InputAction slideInput;
    
    private CharacterController controller;
    
    // Movement tracking.
    Vector3 moveDirection;
    float timeToStopBeingLenient = 0f;
    bool doubleJumpAvailable = false;
    
    // Crouch state (toggle-based)
    bool isCrouching = false;
    
    // For collision adjustments: store the original offset from transform.position to the bottom of the collider.
    private Vector3 standingBottomOffset;
    
    // For tracking falling.
    float previousY;
    
    private void OnEnable()
    {
        moveInput.Enable();
        jumpInput.Enable();
        lookInput.Enable();
        crouchInput.Enable();
        slideInput.Enable();
    }
    
    private void OnDisable()
    {
        moveInput.Disable();
        jumpInput.Disable();
        lookInput.Disable();
        crouchInput.Disable();
        slideInput.Disable();
    }
    
    void Start()
    {
        SetUpCharacterController();
        SetUpRigidbody();
        
        // Cache the original collider values.
        standingHeight = controller.height;
        crouchHeight = standingHeight * 0.5f; // Example: crouch is half as tall.
        standingBottomOffset = controller.center - Vector3.up * (controller.height / 2f);
        
        // Cache the original camera height.
        if (playerCamera != null)
            standingCameraHeight = playerCamera.localPosition.y;
    }
    
    private void SetUpCharacterController()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            Debug.LogError("PlayerController requires a CharacterController on the same GameObject!");
    }
    
    private void SetUpRigidbody()
    {
        Rigidbody playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody != null)
            playerRigidbody.useGravity = false;
    }
    
    void Update()
    {
        // Slide: only start a slide if slide input is triggered, player is grounded, not already sliding,
        // and slide cooldown has elapsed.
        if (slideInput.triggered && controller.isGrounded && !isSliding && Time.time >= lastSlideTime + slideCooldown)
        {
            isSliding = true;
            slideEndTime = Time.time + slideDuration;
            isCrouching = true; // Force crouched state during slide.
        }
        
        // When slide duration ends, end slide and record lastSlideTime.
        if (isSliding && Time.time > slideEndTime)
        {
            isSliding = false;
            lastSlideTime = Time.time;
            // Attempt to uncrouch if headroom is clear.
            if (CanStand())
                isCrouching = false;
        }
        
        // Toggle crouch only when not sliding.
        if (!isSliding && crouchInput.triggered)
        {
            if (isCrouching && CanStand())
                isCrouching = false;
            else if (!isCrouching)
                isCrouching = true;
        }
        
        ProcessMovement();
        ProcessHorizontalRotation();
        ProcessCameraHeight();
        ProcessColliderCrouch();
    }
    
    void ProcessMovement()
    {
        float leftRightInput = moveInput.ReadValue<Vector2>().x;
        float forwardBackwardInput = moveInput.ReadValue<Vector2>().y;
        bool jumpPressed = jumpInput.triggered;
        
        float currentSpeed;
        // When airborne, always use moveSpeed.
        if (!controller.isGrounded)
        {
            currentSpeed = moveSpeed;
        }
        else // Grounded.
        {
            if (isSliding)
                currentSpeed = slideSpeed;
            else
                currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        }
        
        // Grounded movement.
        if (controller.isGrounded && moveDirection.y <= 0)
        {
            doubleJumpAvailable = false;
            timeToStopBeingLenient = Time.time + jumpTimeLeniency;
            
            Vector3 inputDir = new Vector3(leftRightInput, 0, forwardBackwardInput);
            inputDir = transform.TransformDirection(inputDir);
            
            // If sliding, use input direction if provided, otherwise default to forward.
            if (isSliding)
            {
                if (inputDir.magnitude < 0.1f)
                    inputDir = transform.forward;
                else
                    inputDir = inputDir.normalized;
                moveDirection = inputDir * currentSpeed;
            }
            else
            {
                moveDirection = inputDir * currentSpeed;
            }
            
            if (jumpPressed)
                moveDirection.y = jumpPower;
        }
        else // Airborne movement.
        {
            Vector3 inputDir = new Vector3(leftRightInput * currentSpeed, moveDirection.y, forwardBackwardInput * currentSpeed);
            moveDirection = transform.TransformDirection(inputDir);
            
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
            moveDirection.y = -0.3f;
        
        if (isFalling())
            moveDirection.y -= gravity * fallingMultiplier * Time.deltaTime;
        else
            moveDirection.y -= gravity * Time.deltaTime;
        
        controller.Move(moveDirection * Time.deltaTime);
    }
    
    void ProcessHorizontalRotation()
    {
        float horizontalSensitivity = 1;
        if (PlayerPrefs.HasKey("HorizontalMouseSensitivity"))
            horizontalSensitivity = PlayerPrefs.GetFloat("HorizontalMouseSensitivity");
        
        float horizontalLookInput = lookInput.ReadValue<Vector2>().x * horizontalSensitivity;
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(0, currentEuler.y + horizontalLookInput * lookSpeed * Time.deltaTime, 0));
    }
    
    void ProcessCameraHeight()
    {
        if (playerCamera == null)
            return;
        
        float targetCameraY = isCrouching ? crouchingCameraHeight : standingCameraHeight;
        Vector3 camLocal = playerCamera.localPosition;
        camLocal.y = Mathf.Lerp(camLocal.y, targetCameraY, Time.deltaTime * cameraCrouchSpeed);
        playerCamera.localPosition = camLocal;
    }
    
    void ProcessColliderCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float newHeight = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * cameraCrouchSpeed);
        controller.height = newHeight;
        controller.center = standingBottomOffset + Vector3.up * (newHeight / 2f);
    }
    
    // Checks if there is sufficient headroom to stand up using a raycast.
    private bool CanStand()
    {
        float checkStartHeight = 0.1f;
        float checkDistance = standingHeight - checkStartHeight;
        Vector3 origin = transform.position + Vector3.up * checkStartHeight;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.up, out hit, checkDistance))
        {
            if (hit.collider.gameObject != gameObject)
                return false;
        }
        return true;
    }
    
    bool isFalling()
    {
        bool falling = previousY > transform.localPosition.y;
        previousY = transform.localPosition.y;
        return falling;
    }
    
    public void Bounce(float bounceForceMultiplier, float bounceJumpButtonHeldMultiplyer)
    {
        if (jumpInput.ReadValue<float>() != 0)
            moveDirection.y = jumpPower * bounceJumpButtonHeldMultiplyer;
        else
            moveDirection.y = jumpPower * bounceForceMultiplier;
    }
    
    public void ResetJumps()
    {
        doubleJumpAvailable = false;
    }
    
    bool RayCastGrounded()
    {
        bool grounded = false;
        Debug.DrawRay(transform.position, -transform.up * 1.1f, Color.red);
        if (Physics.Raycast(transform.position, -transform.up, 1.1f))
            grounded = true;
        return grounded;
    }
}
