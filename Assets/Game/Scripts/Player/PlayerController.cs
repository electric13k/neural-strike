using UnityEngine;

// ============================================================
//  PLAYER CONTROLLER  — Neural Strike
//  Covers: WASD movement, sprint, variable jump (skill-based
//  multi-jump), air-control, ground-check sphere, full gravity.
//
//  HOW TO WIRE IN UNITY
//  1. Create empty GameObject "Player" at (0,1,0).
//  2. Add CharacterController  (center 0,1,0 | height 2 | radius 0.4).
//  3. Add THIS script.
//  4. Add child empty "GroundCheck" at (0, -0.05, 0) → assign groundCheck.
//  5. Add child Camera → assign to MouseLook.playerBody is THIS transform,
//     MouseLook lives on the Camera.
//  6. Create layer "Ground", apply it to all floor/terrain objects.
//     Set groundMask to that layer in Inspector.
// ============================================================

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Movement ──────────────────────────────────────────────
    [Header("Movement")]
    public float walkSpeed        = 5f;
    public float sprintSpeed      = 8f;
    public float gravity          = -9.81f;
    public float jumpHeight       = 1.5f;
    public float airControlMult   = 0.45f;

    // ── Skill-based multi-jump ─────────────────────────────────
    [Header("Jump Skill  (0=single  1=double  2=triple)")]
    [Range(0,2)] public int jumpSkillLevel = 0;

    // ── Ground detection ──────────────────────────────────────
    [Header("Ground Check")]
    public Transform  groundCheck;
    public float      groundCheckRadius = 0.28f;
    public LayerMask  groundMask;

    // ── Internal state ────────────────────────────────────────
    private CharacterController _cc;
    private Vector3 _vel;          // accumulated velocity
    private bool    _grounded;
    private int     _jumpsUsed;

    private int MaxJumps => 1 + Mathf.Clamp(jumpSkillLevel, 0, 2);

    // ─────────────────────────────────────────────────────────
    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        GroundCheck();
        MoveHorizontal();
        TryJump();
        ApplyGravity();
        _cc.Move(_vel * Time.deltaTime);
    }

    // ── Private helpers ───────────────────────────────────────

    void GroundCheck()
    {
        if (groundCheck != null)
            _grounded = Physics.CheckSphere(
                groundCheck.position, groundCheckRadius,
                groundMask, QueryTriggerInteraction.Ignore);
        else
            _grounded = _cc.isGrounded;

        if (_grounded && _vel.y < 0f)
        {
            _vel.y    = -2f;   // keeps player pressed to slope
            _jumpsUsed = 0;
        }
    }

    void MoveHorizontal()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.sqrMagnitude > 0.01f)
        {
            float spd     = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            float control = _grounded ? 1f : airControlMult;
            Vector3 move  = transform.TransformDirection(dir) * spd * control;
            _vel.x = move.x;
            _vel.z = move.z;
        }
        else if (_grounded)
        {
            _vel.x = 0f;
            _vel.z = 0f;
        }
    }

    void TryJump()
    {
        if (!Input.GetButtonDown("Jump")) return;

        bool canJump = _grounded || (_jumpsUsed < MaxJumps - 1);
        if (!canJump) return;

        _vel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        _jumpsUsed++;
    }

    void ApplyGravity()
    {
        _vel.y += gravity * Time.deltaTime;
    }

    // ── Public API (called by other systems) ──────────────────

    /// <summary>Called by TeleportAbility — momentarily disables CC to warp.</summary>
    public void Warp(Vector3 destination)
    {
        _cc.enabled = false;
        transform.position = destination;
        _cc.enabled = true;
    }

    /// <summary>Force-sets vertical velocity (e.g. grenade knockback).</summary>
    public void ApplyImpulse(Vector3 impulse)
    {
        _vel += impulse;
    }
}
