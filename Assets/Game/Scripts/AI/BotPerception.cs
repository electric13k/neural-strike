using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles vision and enemy detection for bots.
/// </summary>
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
    private List<GameObject> visibleEnemies = new List<GameObject>();
    private float lastUpdateTime;

    public List<GameObject> VisibleEnemies => new List<GameObject>(visibleEnemies);

    private void Awake()
    {
        myHealth = GetComponent<Health>();
    }

    private void Update()
    {
        if (Time.time >= lastUpdateTime + updateInterval)
        {
            UpdatePerception();
            lastUpdateTime = Time.time;
        }
    }

    private void UpdatePerception()
    {
        visibleEnemies.Clear();

        Collider[] nearby = Physics.OverlapSphere(transform.position, visionRange, targetMask);
        foreach (Collider col in nearby)
        {
            Health targetHealth = col.GetComponentInParent<Health>();
            if (targetHealth == null) continue;
            if (targetHealth == myHealth) continue;
            if (myHealth != null && targetHealth.team == myHealth.team) continue;
            if (targetHealth.IsDead) continue;

            if (IsInVisionCone(col.transform))
                visibleEnemies.Add(col.gameObject);
        }
    }

    private bool IsInVisionCone(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > visionAngle / 2f) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        if (Physics.Raycast(transform.position, direction, distance, obstacleMask))
            return false;

        return true;
    }

    public GameObject GetClosestEnemy()
    {
        if (visibleEnemies.Count == 0) return null;

        GameObject closest = null;
        float closestDist = float.MaxValue;

        foreach (GameObject enemy in visibleEnemies)
        {
            if (enemy == null) continue;
            float d = Vector3.Distance(transform.position, enemy.transform.position);
            if (d < closestDist) { closestDist = d; closest = enemy; }
        }
        return closest;
    }

    public bool IsDetected(GameObject target) => visibleEnemies.Contains(target);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Vector3 left  = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * visionRange;
        Vector3 right = Quaternion.Euler(0,  visionAngle / 2f, 0) * transform.forward * visionRange;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
    }
}
