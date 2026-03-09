using UnityEngine;
using System.Collections.Generic;

public class CameraFeedManager : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCamera;
    public KeyCode switchToTrackerKey = KeyCode.T;
    public KeyCode returnToPlayerKey = KeyCode.Escape;
    
    private enum ViewMode { Player, TrackedTarget }
    private ViewMode currentMode = ViewMode.Player;
    
    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;
    
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            originalCameraParent = mainCamera.transform.parent;
            originalCameraLocalPosition = mainCamera.transform.localPosition;
            originalCameraLocalRotation = mainCamera.transform.localRotation;
        }
    }
    
    private void Update()
    {
        HandleCameraSwitching();
    }
    
    private void HandleCameraSwitching()
    {
        if (Input.GetKeyDown(switchToTrackerKey))
        {
            GameObject tracked = TrackerBullet.GetTrackedTarget();
            if (tracked != null)
            {
                SwitchToTrackedView(tracked);
            }
            else
            {
                Debug.Log("No tracked target available");
            }
        }
        
        if (Input.GetKeyDown(returnToPlayerKey) && currentMode != ViewMode.Player)
        {
            ReturnToPlayerView();
        }
    }
    
    private void SwitchToTrackedView(GameObject target)
    {
        if (mainCamera == null) return;
        
        currentMode = ViewMode.TrackedTarget;
        
        // Attach camera to tracked target
        mainCamera.transform.SetParent(target.transform);
        mainCamera.transform.localPosition = new Vector3(0, 2, -5);
        mainCamera.transform.localRotation = Quaternion.Euler(15, 0, 0);
        
        Debug.Log($"Switched to tracked target: {target.name}");
    }
    
    private void ReturnToPlayerView()
    {
        if (mainCamera == null) return;
        
        currentMode = ViewMode.Player;
        
        mainCamera.transform.SetParent(originalCameraParent);
        mainCamera.transform.localPosition = originalCameraLocalPosition;
        mainCamera.transform.localRotation = originalCameraLocalRotation;
        
        Debug.Log("Returned to player view");
    }
    
    public bool IsViewingTrackedTarget() { return currentMode == ViewMode.TrackedTarget; }
}