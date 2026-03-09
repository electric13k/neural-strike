using UnityEngine;
using NeuralStrike.Core;

namespace NeuralStrike.Player
{
    /// <summary>
    /// First-person player controller with movement, jumping, sprinting, and crouching.
    /// Requires CharacterController component.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = 20f;
        
        [Header("Crouch Settings")]
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        
        [Header("Input Settings")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        
        private CharacterController controller;
        private Vector3 velocity;
        private bool isCrouching = false;
        private float currentSpeed;
        
        void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.height = standingHeight;
            currentSpeed = walkSpeed;
        }
        
        void Update()
        {
            HandleMovement();
            HandleCrouch();
        }
        
        void HandleMovement()
        {
            // Ground check
            bool isGrounded = controller.isGrounded;
            
            // Gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to stay grounded
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }
            
            // Jump
            if (isGrounded && Input.GetKeyDown(jumpKey) && !isCrouching)
            {
                velocity.y = jumpForce;
            }
            
            // Movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // Determine speed
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (Input.GetKey(sprintKey) && vertical > 0) // Sprint only when moving forward
            {
                currentSpeed = sprintSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }
            
            // Calculate movement direction
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            move = move.normalized * currentSpeed;
            
            // Apply movement
            velocity.x = move.x;
            velocity.z = move.z;
            
            controller.Move(velocity * Time.deltaTime);
        }
        
        void HandleCrouch()
        {
            // Toggle crouch
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = !isCrouching;
            }
            
            // Smooth height transition
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            
            // Adjust camera/center position if needed
            controller.center = new Vector3(0, controller.height / 2f, 0);
        }
        
        /// <summary>
        /// Get current movement velocity (for footstep sounds, animations, etc.).
        /// </summary>
        public Vector3 GetVelocity()
        {
            return velocity;
        }
        
        /// <summary>
        /// Check if player is currently sprinting.
        /// </summary>
        public bool IsSprinting()
        {
            return Input.GetKey(sprintKey) && currentSpeed == sprintSpeed;
        }
        
        /// <summary>
        /// Check if player is crouching.
        /// </summary>
        public bool IsCrouching()
        {
            return isCrouching;
        }
    }
}