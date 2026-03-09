using UnityEngine;
using System.Collections.Generic;

public class BotPerception : MonoBehaviour
{
    [Header("Vision")]
    public float visionRange = 30f;
    public float visionAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    
    [Header("Hearing")]
    public float hearingRange = 15f;
    
    [Header("Update Rate")]
    public float updateInterval = 0.2f;
    
    private Health myHealth;
    private List<GameObject> detectedEnemies = new List<GameObject>();
    private float nextUpdate;
    
    public List<GameObject> DetectedEnemies => detectedEnemies;
    
    private void Awake()
    {
        myHealth = GetComponent<Health>();
    }
    
    private void Update()
    {
        if (Time.time >= nextUpdate)
        {
            UpdatePerception();
            nextUpdate = Time.time + updateInterval;
        }
    }
    
    private void UpdatePerception()
    {
        detectedEnemies.Clear();
        
        Collider[] nearby = Physics.OverlapSphere(transform.position, visionRange, targetMask);
        
        foreach (Collider col in nearby)
        {
            Health targetHealth = col.GetComponentInParent<Health>();
            if (targetHealth == null) continue;
            if (targetHealth == myHealth) continue;
            if (targetHealth.Team == myHealth.Team) continue;
            if (targetHealth.IsDead) continue;
            
            if (IsInVisionCone(col.transform))
            {
                detectedEnemies.Add(col.gameObject);
            }
        }
    }
    
    private bool IsInVisionCone(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        
        if (angle > visionAngle / 2f) return false;
        
        float distance = Vector3.Distance(transform.position, target.position);
        if (Physics.Raycast(transform.position, direction, distance, obstacleMask))
        {
            return false;
        }
        
        return true;
    }
    
    public GameObject GetNearestEnemy()
    {
        if (detectedEnemies.Count == 0) return null;
        
        GameObject nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject enemy in detectedEnemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    public bool IsDetected(GameObject target)
    {
        return detectedEnemies.Contains(target);
    }
}