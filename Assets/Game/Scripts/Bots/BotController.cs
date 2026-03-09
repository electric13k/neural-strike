using UnityEngine;
using UnityEngine.AI;
using NeuralStrike.Core;

namespace NeuralStrike.Bots
{
    /// <summary>
    /// Base bot controller with AI state machine, perception, and combat logic.
    /// Handles movement via NavMesh and basic attack/follow behavior.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    public class BotController : MonoBehaviour
    {
        [Header("Bot Identity")]
        [SerializeField] private string botName = "Bot";
        [SerializeField] private BotRole role = BotRole.Assault;
        
        [Header("Combat Stats")]
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackRange = 20f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float detectionRange = 30f;
        
        [Header("Movement")]
        [SerializeField] private float followDistance = 3f;
        [SerializeField] private float wanderRadius = 10f;
        
        [Header("References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;
        
        protected NavMeshAgent agent;
        protected Health health;
        protected BotPerception perception;
        protected BotState currentState = BotState.Idle;
        
        protected GameObject owner; // Player who owns this bot
        protected GameObject currentTarget;
        protected float nextAttackTime;
        
        public BotRole Role => role;
        public BotState CurrentState => currentState;
        public GameObject Owner => owner;
        public string BotName => botName;
        
        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            perception = GetComponent<BotPerception>();
            
            if (perception == null)
                perception = gameObject.AddComponent<BotPerception>();
        }
        
        protected virtual void Start()
        {
            health.OnDeath.AddListener(OnDeath);
        }
        
        protected virtual void Update()
        {
            if (health.IsDead) return;
            
            // Update AI state machine
            switch (currentState)
            {
                case BotState.Idle:
                    UpdateIdle();
                    break;
                case BotState.Follow:
                    UpdateFollow();
                    break;
                case BotState.Attack:
                    UpdateAttack();
                    break;
                case BotState.Wander:
                    UpdateWander();
                    break;
            }
            
            // Check for enemies
            UpdatePerception();
        }
        
        #region State Updates
        
        protected virtual void UpdateIdle()
        {
            // Idle - wait for orders or enemy detection
            if (owner != null && Vector3.Distance(transform.position, owner.transform.position) > followDistance)
            {
                SetState(BotState.Follow);
            }
        }
        
        protected virtual void UpdateFollow()
        {
            // Follow owner
            if (owner != null)
            {
                agent.SetDestination(owner.transform.position);
                
                // Stop following if close enough
                if (Vector3.Distance(transform.position, owner.transform.position) <= followDistance)
                {
                    agent.ResetPath();
                    SetState(BotState.Idle);
                }
            }
            else
            {
                SetState(BotState.Wander);
            }
        }
        
        protected virtual void UpdateAttack()
        {
            // Attack current target
            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                currentTarget = null;
                SetState(BotState.Follow);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            // Move to attack range
            if (distanceToTarget > attackRange * 0.8f)
            {
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                agent.ResetPath();
            }
            
            // Face target
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            
            // Attack if in range and cooldown ready
            if (distanceToTarget <= attackRange && Time.time >= nextAttackTime)
            {
                Attack(currentTarget);
            }
        }
        
        protected virtual void UpdateWander()
        {
            // Wander around randomly if no owner
            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
        
        #endregion
        
        #region Perception & Targeting
        
        protected virtual void UpdatePerception()
        {
            if (perception == null) return;
            
            GameObject enemy = perception.GetNearestEnemy();
            if (enemy != null && currentState != BotState.Attack)
            {
                currentTarget = enemy;
                SetState(BotState.Attack);
            }
        }
        
        protected virtual bool IsValidTarget(GameObject target)
        {
            if (target == null) return false;
            
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth == null) return false;
            if (targetHealth.IsDead) return false;
            if (targetHealth.Team == health.Team) return false;
            
            return true;
        }
        
        #endregion
        
        #region Combat
        
        protected virtual void Attack(GameObject target)
        {
            nextAttackTime = Time.time + attackCooldown;
            
            // Simple hitscan attack
            if (firePoint != null)
            {
                Vector3 direction = (target.transform.position - firePoint.position).normalized;
                
                if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, attackRange))
                {
                    Health targetHealth = hit.collider.GetComponent<Health>();
                    if (targetHealth != null)
                    {
                        targetHealth.TakeDamage(attackDamage, gameObject);
                    }
                }
            }
        }
        
        #endregion
        
        #region Ownership & Hacking
        
        /// <summary>
        /// Set bot owner (player who spawned or hacked this bot).
        /// </summary>
        public virtual void SetOwner(GameObject newOwner)
        {
            owner = newOwner;
            Debug.Log($"{botName} owner set to {newOwner.name}");
        }
        
        /// <summary>
        /// Transfer bot ownership (used by Data Knife hack).
        /// </summary>
        public virtual void TransferOwnership(Team newTeam, GameObject newOwner)
        {
            health.SetTeam(newTeam);
            owner = newOwner;
            currentTarget = null;
            SetState(BotState.Follow);
            
            Debug.Log($"{botName} hacked! New owner: {newOwner.name}, New team: {newTeam}");
        }
        
        #endregion
        
        #region State Management
        
        protected virtual void SetState(BotState newState)
        {
            currentState = newState;
        }
        
        protected virtual void OnDeath(GameObject killer)
        {
            // Notify game manager of bot death for scoring
            // Respawn logic handled by GameManager
            agent.ResetPath();
            Debug.Log($"{botName} killed by {(killer != null ? killer.name : "unknown")}");
        }
        
        #endregion
    }
    
    public enum BotState
    {
        Idle,
        Follow,
        Attack,
        Wander,
        Retreat,
        Defend
    }
    
    public enum BotRole
    {
        Assault,
        Medic,
        Sniper,
        Spy,
        Courier
    }
}