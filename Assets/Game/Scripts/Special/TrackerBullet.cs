using UnityEngine;

public class TrackerBullet : MonoBehaviour
{
    [Header("Tracker Settings")]
    public float trackerDuration = 30f;
    public GameObject trackerEffectPrefab;
    
    private static TrackerBullet activeTracker;
    
    public static void AttachTracker(GameObject target, Vector3 hitPoint)
    {
        if (target == null) return;
        
        // Remove old tracker
        if (activeTracker != null)
        {
            Destroy(activeTracker.gameObject);
        }
        
        // Create new tracker
        GameObject trackerObj = new GameObject("ActiveTracker");
        trackerObj.transform.SetParent(target.transform);
        trackerObj.transform.position = hitPoint;
        
        TrackerBullet tracker = trackerObj.AddComponent<TrackerBullet>();
        tracker.Initialize(target);
        activeTracker = tracker;
    }
    
    public static GameObject GetTrackedTarget()
    {
        return activeTracker != null ? activeTracker.trackedTarget : null;
    }
    
    private GameObject trackedTarget;
    private float timeRemaining;
    
    private void Initialize(GameObject target)
    {
        trackedTarget = target;
        timeRemaining = trackerDuration;
        
        if (trackerEffectPrefab != null)
        {
            Instantiate(trackerEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }
    
    private void Update()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f || trackedTarget == null)
        {
            if (activeTracker == this) activeTracker = null;
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (activeTracker == this) activeTracker = null;
    }
}