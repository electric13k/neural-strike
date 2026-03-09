using UnityEngine;

namespace NeuralStrike.Bots
{
    /// <summary>
    /// Sniper bot with long-range attacks and positioning preference.
    /// Seeks vantage points and engages from distance.
    /// </summary>
    public class SniperBot : BotController
    {
        [Header("Sniper Settings")]
        [SerializeField] private float preferredRange = 40f; // Prefers to fight at this distance
        [SerializeField] private float maxEngagementRange = 100f;
        [SerializeField] private float aimDelay = 0.5f; // Time to aim before firing
        
        private bool isAiming = false;
        private float aimStartTime;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Snipers have longer range
            attackRange = maxEngagementRange;
            attackCooldown = 1.5f; // Slower fire rate
            attackDamage = 40f; // Higher damage
        }
        
        protected override void UpdateAttack()
        {
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                currentTarget = null;
                isAiming = false;
                SetState(BotState.Follow);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            // Maintain preferred range
            if (distanceToTarget < preferredRange * 0.7f)
            {
                // Too close - back away
                Vector3 retreatDirection = (transform.position - currentTarget.transform.position).normalized;
                agent.SetDestination(transform.position + retreatDirection * 10f);
            }
            else if (distanceToTarget > preferredRange * 1.3f && distanceToTarget < maxEngagementRange)
            {
                // Too far but still in range - move closer
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                // Good range - stop moving
                agent.ResetPath();
            }
            
            // Face target
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 3f);
            
            // Aim before shooting
            if (!isAiming)
            {
                isAiming = true;
                aimStartTime = Time.time;
            }
            
            // Attack if aimed and cooldown ready
            if (isAiming && Time.time >= aimStartTime + aimDelay && Time.time >= nextAttackTime)
            {
                Attack(currentTarget);
                isAiming = false;
            }
        }
        
        /// <summary>
        /// TODO: Find vantage points (high ground, cover) for better positioning.
        /// </summary>
        private void SeekVantagePoint()
        {
            // Future enhancement: NavMesh query for elevated positions
        }
    }
}