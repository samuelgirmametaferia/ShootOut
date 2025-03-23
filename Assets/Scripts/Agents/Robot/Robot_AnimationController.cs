using System.Collections;
using UnityEngine;

public class Robot_AnimationController : MonoBehaviour
{
    [Header("Components :)")]
    public Animator animator; // Reference to the Animator component
    public PlayerMovement playerMovement; // Reference to the PlayerMovement script
    public CameraMovement cameraMovement; // Reference to the CameraMovement script

    private int VelocityX;
    private int VelocityZ;
    private int Turn;
    private int isCrouching;
    private int isJumping;
    private int isGrounded;
    private int isFalling;
    [Header("Animation Interpolation Speed")]
    // Interpolation speed
    public float interpolationSpeed = 5f;
    public float interpolationTurnSpeed = 5f;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        VelocityX = Animator.StringToHash("VelocityX");
        VelocityZ = Animator.StringToHash("VelocityZ");
        Turn = Animator.StringToHash("Turn");
        isCrouching = Animator.StringToHash("isCrouching");
        isJumping = Animator.StringToHash("isJumping");
        isGrounded = Animator.StringToHash("isGrounded");
        isFalling = Animator.StringToHash("isFalling");
    }

    void Update()
    {
        ApplyMovementAnimations();
        ApplyGroundingState();
        ApplyTurning();
        ApplyJumping();
        ApplyCrouching();
    }
    void UpdateShootingState(bool SingleShot, bool RegularFire, bool RapidFire, bool Reload)
    {

        if(SingleShot)
        {

        }else if(RegularFire)
        {

        }else if(RapidFire)
        {

        }
        else
        {

        }
    }

    void ApplyCrouching()
    {
        animator.SetBool(isCrouching, playerMovement.isCrouching);
    }
    void ApplyJumping()
    {
        animator.SetBool(isJumping, playerMovement.isJumping);
    }
    void ApplyMovementAnimations()
    {
        // Ensure playerMovement reference is assigned
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement reference is missing!");
            return;
        }

        // Get normalized input and sprinting status
        Vector2 input = playerMovement.InputVector.normalized;
        bool isSprinting = playerMovement.isSprinting;


        // Target animation vector
        Vector2 targetAnimationVector = isSprinting ? input : input * 0.5f;

        // Current animation values
        float currentVelocityX = animator.GetFloat(VelocityX);
        float currentVelocityZ = animator.GetFloat(VelocityZ);

        // Interpolate values
        float interpolatedX = Mathf.Lerp(currentVelocityX, targetAnimationVector.x, Time.deltaTime * interpolationSpeed);
        float interpolatedZ = Mathf.Lerp(currentVelocityZ, targetAnimationVector.y, Time.deltaTime * interpolationSpeed);

        // Apply interpolated values to the animator
        animator.SetFloat(VelocityX, interpolatedX);
        animator.SetFloat(VelocityZ, interpolatedZ);
    }
    void ApplyGroundingState()
    {
        bool _isGrounded = playerMovement._isGrounded;

        if (_isGrounded)
        {
            animator.SetBool(isFalling, false);
        }
        else
        {
            animator.SetBool(isFalling, true);
        }
        animator.SetBool(isGrounded, _isGrounded);
    }
    void ApplyTurning()
    {
        float mouseX = cameraMovement.mouseDelta.x;
      
        if (mouseX < 0)
        {
            // Current animation values
            float currentTurn = animator.GetFloat(Turn);

            // Interpolate values
            float interpolatedTurn = Mathf.Lerp(currentTurn, 0f, Time.deltaTime * interpolationTurnSpeed);

            // Apply interpolated values to the animator
            animator.SetFloat(Turn, interpolatedTurn);
        }
        else if (mouseX > 0)
        {
            // Current animation values
            float currentTurn = animator.GetFloat(Turn);

            // Interpolate values
            float interpolatedTurn = Mathf.Lerp(currentTurn, 1f, Time.deltaTime * interpolationTurnSpeed);

            // Apply interpolated values to the animator
            animator.SetFloat(Turn, interpolatedTurn);
        }
        else
        {

            // Current animation values
            float currentTurn = animator.GetFloat(Turn);

            // Interpolate values
            float interpolatedTurn = Mathf.Lerp(currentTurn, 0.5f, Time.deltaTime * interpolationTurnSpeed);

            // Apply interpolated values to the animator
            animator.SetFloat(Turn, interpolatedTurn);

        }
    }
}
