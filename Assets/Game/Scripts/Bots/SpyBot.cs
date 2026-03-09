using UnityEngine;

namespace NeuralStrike.Bots
{
    /// <summary>
    /// Spy bot that scouts ahead, flanks enemies, and prefers stealth/backstab attacks.
    /// Moves faster and has wider detection range.
    /// </summary>
    public class SpyBot : BotController
    {
        [Header("Spy Settings")]
        [SerializeField] private float scoutRange = 50f;
        [SerializeField] private float backstabMultiplier = 2f; // Bonus damage from behind
        [SerializeField] private float backstabAngle = 60f; // Degrees behind target for backstab
        
        protected override void Awake()
        {
            base.Awake();
            
            // Spies are faster
            agent.speed *= 1.3f;
            
            // Wider detection
            if (perception != null)
            {
                // Perception range set in inspector, but spy has advantage
            }
        }
        
        protected override void UpdateAttack()
        {
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                currentTarget = null;
                SetState(BotState.Follow);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            // Try to flank/get behind target
            if (distanceToTarget > attackRange * 0.5f)
            {
                Vector3 behindTarget = GetFlankPosition(currentTarget);
                agent.SetDestination(behindTarget);
            }
            else
            {
                agent.ResetPath();
            }
            
            // Face target
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
            
            // Attack
            if (distanceToTarget <= attackRange && Time.time >= nextAttackTime)
            {
                Attack(currentTarget);
            }
        }
        
        /// <summary>
        /// Calculate flank position behind target.
        /// </summary>
        private Vector3 GetFlankPosition(GameObject target)
        {
            // Position behind target
            Vector3 behindTarget = target.transform.position - target.transform.forward * 3f;
            return behindTarget;
        }
        
        /// <summary>
        /// Check if attacking from behind for backstab bonus.
        /// </summary>
        private bool IsBackstab(GameObject target)
        {
            Vector3 toBot = (transform.position - target.transform.position).normalized;
            float angle = Vector3.Angle(target.transform.forward, toBot);
            return angle < backstabAngle;
        }
        
        protected override void Attack(GameObject target)
        {
            // Check for backstab
            float damage = attackDamage;
            if (IsBackstab(target))
            {
                damage *= backstabMultiplier;
                Debug.Log("Spy backstab!");
            }
            
            nextAttackTime = Time.time + attackCooldown;
            
            if (firePoint != null)
            {
                Vector3 direction = (target.transform.position - firePoint.position).normalized;
                
                if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, attackRange))
                {
                    var targetHealth = hit.collider.GetComponent<Health>();
                    if (targetHealth != null)
                    {
                        targetHealth.TakeDamage(damage, gameObject);
                    }
                }
            }
        }
    }
}