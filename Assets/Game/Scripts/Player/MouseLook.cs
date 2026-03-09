using UnityEngine;

namespace NeuralStrike.Player
{
    /// <summary>
    /// First-person mouse look camera controller.
    /// Handles camera rotation and mouse sensitivity.
    /// </summary>
    public class MouseLook : MonoBehaviour
    {
        [Header("Sensitivity")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;
        
        [Header("References")]
        [SerializeField] private Transform playerBody; // The parent transform to rotate horizontally
        [SerializeField] private Transform cameraTransform; // The camera transform to rotate vertically
        
        private float verticalRotation = 0f;
        
        void Start()
        {
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Auto-find references if not set
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;
            if (playerBody == null)
                playerBody = transform;
        }
        
        void Update()
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Horizontal rotation (player body)
            playerBody.Rotate(Vector3.up * mouseX);
            
            // Vertical rotation (camera)
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            
            // Toggle cursor lock with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursorLock();
            }
        }
        
        /// <summary>
        /// Toggle cursor lock (for menus, etc.).
        /// </summary>
        public void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        /// <summary>
        /// Set mouse sensitivity (for settings menu).
        /// </summary>
        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
    }
}