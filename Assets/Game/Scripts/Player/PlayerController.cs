using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed        = 5f;
    public float sprintSpeed      = 8f;
    public float jumpHeight       = 1.5f;
    public float gravity          = -9.81f;  // Earth gravity
    public float airControlMultiplier = 0.5f;

    [Header("Jump Skills (set by loadout)")]
    [Tooltip("0 = normal, 1 = double jump, 2 = triple jump")]
    [Range(0, 2)] public int jumpSkillLevel = 0;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;

    private CharacterController _cc;
    private Vector3 _velocity;
    private bool    _isGrounded;
    private int     _jumpsUsed;

    private int MaxJumps => 1 + Mathf.Clamp(jumpSkillLevel, 0, 2);

    private void Awake() => _cc = GetComponent<CharacterController>();

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        _cc.Move(_velocity * Time.deltaTime);
    }

    private void HandleGroundCheck()
    {
        _isGrounded = groundCheck != null
            ? Physics.CheckSphere(groundCheck.position, groundCheckRadius,
                  groundMask, QueryTriggerInteraction.Ignore)
            : _cc.isGrounded;

        if (_isGrounded && _velocity.y < 0f)
        {
            _velocity.y  = -2f;
            _jumpsUsed   = 0;   // reset multi-jump counter on land
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.magnitude >= 0.1f)
        {
            float spd     = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            float control = _isGrounded ? 1f : airControlMultiplier;
            Vector3 move  = transform.TransformDirection(dir) * spd * control;
            _velocity.x   = move.x;
            _velocity.z   = move.z;
        }
        else if (_isGrounded)
        {
            _velocity.x = _velocity.z = 0f;
        }
    }

    private void HandleJump()
    {
        if (!Input.GetButtonDown("Jump")) return;

        // Allow jump if: on ground, OR extra air jumps still available
        bool canJump = _isGrounded || _jumpsUsed < MaxJumps - 1;
        if (!canJump) return;

        _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        if (!_isGrounded) _jumpsUsed++;   // ground jump does not consume air count
        else              _jumpsUsed = 1; // first jump counts as 1 used
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
    }
}
