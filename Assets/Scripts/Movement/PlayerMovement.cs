using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform Camera;
    [Header("Locomotion")]
    [SerializeField] private float WalkSpeed = 5f;
    [SerializeField] private float RunSpeed = 8f;
    [SerializeField] private float CrouchSpeed = 3f;
    [SerializeField] private float AirSpeed = 1.5f;
    [Header("Crouch")]
    [SerializeField] private float CrouchHeight = 0.5f;
    [SerializeField] private float CrouchCameraHeight = 1.0f;
    [SerializeField] private float CrouchTime = 3f;
    [Header("Ground Check")]
    [SerializeField] private Transform GroundCheckObject;
    [SerializeField] private float GroundCheckRadius = 0.1f;
    [SerializeField] private LayerMask GroundLayer;
    [Header("Jumping")]
    [SerializeField] private float JumpHeight = 2f;

    [Header("Input")]
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private InputActionReference MoveAction;

    [Header("Camera Adjustment Vectors")]
    [SerializeField] private Vector3 CrouchCameraAddition;
    [SerializeField] private Vector3 JumpAddition;
    [Header("Audio")]
    [SerializeField] private AudioSource AudioPlayer;
    [SerializeField] private AudioSource AudioPlayer2;
    [SerializeField] private AudioClip WalkAudio;
    [SerializeField] private AudioClip RunAudio;
    [SerializeField] private AudioClip CrouchAudio;
    [SerializeField] private AnimationCurve pitchCurve;
    [SerializeField] private AudioClip DamagedAudio;
    [SerializeField] private Vector2 PitchRange;
    [SerializeField] private float DamageAudioDelay = 0.3f;
    [SerializeField] private float WalkDelay = 0.1f;
    [SerializeField] private float RunDelay = 0.1f;
    [SerializeField] private float CrouchDelay = 0.1f;
    [SerializeField] private float DamageAudioThreshold = 20f; 
    [Header("Networking")]
    [SerializeField] private GameObject RealBody;
    [SerializeField] private GameObject[] Thingstoturnoff;
    //Movement speed 
    [HideInInspector]
    public float _walkSpeed;
    [HideInInspector]
    public float _runSpeed;
    //Movement
    [HideInInspector]
    public Vector2 InputVector;
    //Crouching 
    [HideInInspector]
    public float _crouchSpeed;
    private float operatingSpeed;
    private float playerHeight;
    private float cameraHeight;
    private float airSpeed;
    private CapsuleCollider PlayerCollider;


    //States 
    public bool isSprinting = false;
    public bool isCrouching = false;
    public bool isJumping = false;
    public bool _isGrounded = false;
    Vector3 groundCheckPosition;
    //Maybe another. ,BTW if you are here you are a G.
    public bool AllowMovement = true;
    private Vector3 NetworkPosition;
    private Vector3 NetworkVelocity;
    private double LastUpdateTime;
    private Vector3 estimatedPosition;
    //Audio
    private float LastTimeSinceStep;
    private float LastDamageAudio;
    void Awake()
    {
        LastDamageAudio += DamageAudioDelay;
        //Getting Components
        rb = GetComponent<Rigidbody>();
        PlayerCollider = GetComponent<CapsuleCollider>();

        //Initialization
        _walkSpeed = WalkSpeed;
        _runSpeed = RunSpeed;
        _crouchSpeed = CrouchSpeed;
        operatingSpeed = _walkSpeed;
        airSpeed = AirSpeed;

        //Saving data for crouching
        playerHeight = PlayerCollider.height;
        cameraHeight = Camera.localPosition.y;
        groundCheckPosition = GroundCheckObject.localPosition;
    }


    void FixedUpdate()
    {

        _isGrounded = isGrounded();
        if (isJumping && _isGrounded)
        {
            isJumping = false;
        }
        InputVector = Input();
        Move(InputVector);


    }
    private void Update()
    {
        HandleFootSteps();
        LastTimeSinceStep += Time.deltaTime;
        LastDamageAudio += Time.deltaTime;
    }



    public void Move(Vector2 input)
    {
        if (!isGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * airSpeed, rb.linearVelocity.y, rb.linearVelocity.z * airSpeed);
        }
        Vector3 moveDirection;
        Vector3 cameraForward = Vector3.Scale(Camera.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(Camera.right, new Vector3(1, 0, 1)).normalized;

        // Calculate moveDirection based on input
        moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;

        // Calculate movement vector
        Vector3 movement = moveDirection * operatingSpeed;

        // Assign the velocity to the Rigidbody
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);



    }

    public void HandleFootSteps()
    {
        if (_isGrounded && InputVector.magnitude > 0f)
        {
            float normalizedSpeed = Mathf.InverseLerp(0f, RunSpeed, operatingSpeed);  // Assuming MaxSpeed is defined as the player's max movement speed
            float pitch = Mathf.Lerp(PitchRange.x, PitchRange.y, pitchCurve.Evaluate(normalizedSpeed));

            AudioPlayer.pitch = pitch;

            // Adjust the timing of footstep sounds based on the action and movement state
            if (isSprinting && LastTimeSinceStep > RunDelay)
            {
                AudioPlayer.PlayOneShot(RunAudio);
                LastTimeSinceStep = 0;
            }
            else if (isCrouching && LastTimeSinceStep > CrouchDelay)
            {
                AudioPlayer.PlayOneShot(CrouchAudio);
                LastTimeSinceStep = 0;
            }
            else if (!isSprinting && !isCrouching && LastTimeSinceStep > WalkDelay)
            {
                AudioPlayer.PlayOneShot(WalkAudio);
                LastTimeSinceStep = 0;
            }
        }

    }
    public void DamageAudio(float Damage)
    {
        if (LastDamageAudio < DamageAudioDelay) return;
        if (Damage < DamageAudioThreshold)
        {
            return;
        }

        float pitch = Random.Range(PitchRange.x, PitchRange.y);
        AudioPlayer2.pitch = pitch;
        AudioPlayer2.PlayOneShot(DamagedAudio);
        LastDamageAudio = 0;
    }



    public void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded && !isCrouching && AllowMovement)
        {
            isJumping = true;
            float JumpVelocity = Mathf.Sqrt(-2f * Physics.gravity.y * JumpHeight);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, JumpVelocity, rb.linearVelocity.z);

        }

        if (context.performed)
        {
            Camera.localPosition += JumpAddition;
        }
        else
        {
            Camera.localPosition = new Vector3(0f, cameraHeight, 0f);
        }
    }

    public bool isGrounded()
    {
        return Physics.CheckSphere(GroundCheckObject.position, GroundCheckRadius, GroundLayer);
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (isCrouching) return;
        if (context.performed)
        {
            operatingSpeed = _runSpeed;
            isSprinting = true;
        }
        else
        {
            operatingSpeed = _walkSpeed;
            isSprinting = false;
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            isCrouching = true;
            operatingSpeed = _crouchSpeed;

            GroundCheckObject.localPosition += new Vector3(0f, CrouchHeight + GroundCheckRadius, 0f);
            StartCoroutine(CrouchTo(CrouchTime, CrouchHeight, CrouchCameraHeight));
            Camera.localPosition += CrouchCameraAddition;

        }
        else if (context.canceled)
        {
            isCrouching = false;
            operatingSpeed = _walkSpeed;
            GroundCheckObject.localPosition = groundCheckPosition;
            StartCoroutine(CrouchTo(CrouchTime, playerHeight, cameraHeight));

        }
    }

    public IEnumerator CrouchTo(float totalTime, float playerheight, float cameraheight)
    {
        float timer = 0f;
        while (timer < totalTime)
        {
            PlayerCollider.height = Mathf.Lerp(PlayerCollider.height, playerheight, timer / totalTime);
            Camera.localPosition = new Vector3(0f, Mathf.Lerp(Camera.localPosition.y + CrouchCameraAddition.y, cameraheight, timer / totalTime), 0f);
            timer += Time.deltaTime;
        }
        PlayerCollider.height = playerheight;
        Camera.localPosition = new Vector3(0f, cameraheight, 0f);

        yield return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GroundCheckObject.position, GroundCheckRadius);
    }

    Vector2 Input()
    {
        Vector2 input = MoveAction.action.ReadValue<Vector2>();
        return input;
    }
}
