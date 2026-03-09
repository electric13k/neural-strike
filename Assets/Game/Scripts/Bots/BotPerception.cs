using UnityEngine;
using System.Collections.Generic;
using NeuralStrike.Core;

namespace NeuralStrike.Bots
{
    /// <summary>
    /// Bot perception system for detecting enemies within view cone and range.
    /// Uses vision cone, hearing radius, and line-of-sight checks.
    /// </summary>
    public class BotPerception : MonoBehaviour
    {
        [Header("Vision")]
        [SerializeField] private float visionRange = 30f;
        [SerializeField] private float visionAngle = 90f; // Field of view in degrees
        [SerializeField] private LayerMask visionBlockingLayers;
        
        [Header("Hearing")]
        [SerializeField] private float hearingRange = 15f;
        
        [Header("Update Rate")]
        [SerializeField] private float perceptionUpdateRate = 0.2f; // Check for enemies every 0.2s
        
        private Health myHealth;
        private List<GameObject> detectedEnemies = new List<GameObject>();
        private float nextPerceptionUpdate;
        
        public List<GameObject> DetectedEnemies => detectedEnemies;
        
        void Awake()
        {
            myHealth = GetComponent<Health>();
        }
        
        void Update()
        {
            if (Time.time >= nextPerceptionUpdate)
            {
                UpdatePerception();
                nextPerceptionUpdate = Time.time + perceptionUpdateRate;
            }
        }
        
        /// <summary>
        /// Update list of detected enemies.
        /// </summary>
        private void UpdatePerception()
        {
            detectedEnemies.Clear();
            
            // Find all potential targets in range
            Collider[] nearby = Physics.OverlapSphere(transform.position, visionRange);
            
            foreach (Collider col in nearby)
            {
                Health targetHealth = col.GetComponent<Health>();
                if (targetHealth == null) continue;
                if (targetHealth == myHealth) continue; // Don't detect self
                if (targetHealth.Team == myHealth.Team) continue; // Same team
                if (targetHealth.IsDead) continue;
                
                // Check if in vision cone
                if (IsInVisionCone(col.transform))
                {
                    detectedEnemies.Add(col.gameObject);
                }
            }
        }
        
        /// <summary>
        /// Check if target is within vision cone and line of sight.
        /// </summary>
        private bool IsInVisionCone(Transform target)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            // Check angle
            if (angleToTarget > visionAngle / 2f)
                return false;
            
            // Check line of sight
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (Physics.Raycast(transform.position, directionToTarget, distanceToTarget, visionBlockingLayers))
            {
                return false; // Vision blocked
            }
            
            return true;
        }
        
        /// <summary>
        /// Get nearest detected enemy.
        /// </summary>
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
        
        /// <summary>
        /// Check if specific target is detected.
        /// </summary>
        public bool IsDetected(GameObject target)
        {
            return detectedEnemies.Contains(target);
        }
        
        /// <summary>
        /// Debug draw vision cone in editor.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
            
            // Draw vision cone
            Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward * visionRange;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}