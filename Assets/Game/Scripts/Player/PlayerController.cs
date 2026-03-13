using UnityEngine;

/// <summary>
/// First-person player movement with earth gravity (-9.81) and skill-based multi-jump.
/// jumpSkillLevel: 0 = 1 jump, 1 = double, 2 = triple.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;

    // Earth gravity: -9.81 m/s^2
    public float gravity = -9.81f;
    public float airControlMultiplier = 0.5f;

    [Header("Jump Skills")]
    [Tooltip("0=single, 1=double, 2=triple jump")]
    [Range(0, 2)] public int jumpSkillLevel = 0;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private int jumpsUsed;

    private int MaxJumps => 1 + Mathf.Clamp(jumpSkillLevel, 0, 2);

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleGroundCheck()
    {
        isGrounded = groundCheck != null
            ? Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore)
            : controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            jumpsUsed = 0;
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            bool sprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = sprinting ? sprintSpeed : walkSpeed;
            float control = isGrounded ? 1f : airControlMultiplier;
            Vector3 move = transform.TransformDirection(inputDir) * currentSpeed * control;
            velocity.x = move.x;
            velocity.z = move.z;
        }
        else if (isGrounded)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }
    }

    private void HandleJump()
    {
        if (!Input.GetButtonDown("Jump")) return;
        if (isGrounded || jumpsUsed < MaxJumps - 1)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }
}
