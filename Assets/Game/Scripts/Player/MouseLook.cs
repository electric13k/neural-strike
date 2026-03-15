using UnityEngine;

// ============================================================
//  MOUSE LOOK  — Neural Strike
//
//  HOW TO WIRE IN UNITY
//  1. Attach to the Camera child of Player.
//  2. Assign playerBody = the Player root transform.
//  3. Camera sits at eye-height (0, 0.7, 0) relative to Player.
// ============================================================

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;          // Player root — yaw applied here

    [Header("Settings")]
    public float sensitivity = 120f;
    public float minPitch    = -85f;
    public float maxPitch    =  85f;
    public bool  lockCursor  = true;

    private float _pitch;                 // vertical angle

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void Update()
    {
        if (lockCursor && Input.GetKeyDown(KeyCode.Escape))
        {
            // toggle cursor for menus
            lockCursor = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            return;
        }

        float mX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        float mY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        _pitch -= mY;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mX);
    }

    /// <summary>Re-lock cursor when entering gameplay from a menu.</summary>
    public void RelockCursor()
    {
        lockCursor       = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}
