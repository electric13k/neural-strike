using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HackStartEvent : UnityEvent<GameObject> { }
[System.Serializable]
public class HackCompleteEvent : UnityEvent<GameObject> { }
[System.Serializable]
public class HackCancelEvent : UnityEvent { }

public interface IHackable
{
    void OnHackComplete(GameObject hacker);
    string GetHackPrompt();
}

public class DataKnife : MonoBehaviour
{
    [Header("Hacking")]
    public float hackRange = 3f;
    public float hackDuration = 3f;
    public LayerMask hackableMask;
    public KeyCode hackKey = KeyCode.E;
    
    [Header("Events")]
    public HackStartEvent onHackStart;
    public HackCompleteEvent onHackComplete;
    public HackCancelEvent onHackCancel;
    
    private GameObject currentTarget;
    private IHackable currentHackable;
    private float hackProgress;
    private bool isHacking;
    
    private void Update()
    {
        if (!isHacking)
        {
            DetectHackableTarget();
            
            if (currentTarget != null && Input.GetKeyDown(hackKey))
            {
                StartHack();
            }
        }
        else
        {
            UpdateHack();
        }
    }
    
    private void DetectHackableTarget()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, hackRange, hackableMask))
        {
            IHackable hackable = hit.collider.GetComponentInParent<IHackable>();
            if (hackable != null)
            {
                currentTarget = hit.collider.gameObject;
                currentHackable = hackable;
                return;
            }
        }
        
        currentTarget = null;
        currentHackable = null;
    }
    
    private void StartHack()
    {
        if (currentTarget == null || currentHackable == null) return;
        
        isHacking = true;
        hackProgress = 0f;
        onHackStart.Invoke(currentTarget);
    }
    
    private void UpdateHack()
    {
        if (currentTarget == null || currentHackable == null)
        {
            CancelHack();
            return;
        }
        
        // Check if still in range and looking at target
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > hackRange)
        {
            CancelHack();
            return;
        }
        
        // Release key = cancel
        if (Input.GetKeyUp(hackKey))
        {
            CancelHack();
            return;
        }
        
        // Update progress
        hackProgress += Time.deltaTime;
        
        if (hackProgress >= hackDuration)
        {
            CompleteHack();
        }
    }
    
    private void CompleteHack()
    {
        if (currentHackable != null)
        {
            currentHackable.OnHackComplete(gameObject);
            onHackComplete.Invoke(currentTarget);
        }
        
        isHacking = false;
        hackProgress = 0f;
        currentTarget = null;
        currentHackable = null;
    }
    
    private void CancelHack()
    {
        isHacking = false;
        hackProgress = 0f;
        onHackCancel.Invoke();
    }
    
    public bool IsHacking() { return isHacking; }
    public float GetHackProgress() { return Mathf.Clamp01(hackProgress / hackDuration); }
    public GameObject GetCurrentTarget() { return currentTarget; }
    public string GetCurrentPrompt() { return currentHackable?.GetHackPrompt() ?? ""; }
}