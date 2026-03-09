using UnityEngine;
using System.Collections.Generic;

public class BotPerception : MonoBehaviour
{
    [Header("Vision")] public float visionRange = 30f; public float visionAngle = 90f; public LayerMask targetMask; public LayerMask obstacleMask;
    [Header("Hearing")] public float hearingRange = 15f;
    [Header("Update")] public float updateInterval = 0.2f;
    private List<GameObject> visibleEnemies = new List<GameObject>(); private float lastUpdateTime;
    private void Update() { if (Time.time >= lastUpdateTime + updateInterval) { UpdatePerception(); lastUpdateTime = Time.time; } }
    private void UpdatePerception() { visibleEnemies.Clear(); Collider[] targetsInRange = Physics.OverlapSphere(transform.position, visionRange, targetMask); foreach (Collider target in targetsInRange) { Transform targetTransform = target.transform; Vector3 directionToTarget = (targetTransform.position - transform.position).normalized; float angleToTarget = Vector3.Angle(transform.forward, directionToTarget); if (angleToTarget <= visionAngle / 2f) { float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position); if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)) visibleEnemies.Add(target.gameObject); } } }
    public GameObject GetClosestEnemy() { if (visibleEnemies.Count == 0) return null; GameObject closest = null; float closestDistance = Mathf.Infinity; foreach (GameObject enemy in visibleEnemies) { if (enemy == null) continue; float distance = Vector3.Distance(transform.position, enemy.transform.position); if (distance < closestDistance) { closest = enemy; closestDistance = distance; } } return closest; }
    public List<GameObject> GetAllVisibleEnemies() { return new List<GameObject>(visibleEnemies); }
    private void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, visionRange); Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * visionRange; Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward * visionRange; Gizmos.color = Color.cyan; Gizmos.DrawLine(transform.position, transform.position + leftBoundary); Gizmos.DrawLine(transform.position, transform.position + rightBoundary); Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, hearingRange); }
}