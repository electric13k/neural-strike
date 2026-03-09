using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(BotPerception))]
public class BotController : MonoBehaviour
{
    [Header("References")]
    public BotPerception perception;
    public Weapon weapon;
    public Transform firePoint;
    
    [Header("Movement")]
    public float patrolRadius = 20f;
    public float chaseDistance = 30f;
    public float attackDistance = 15f;
    public float retreatHealthPercent = 0.3f;
    public float aimSpeed = 5f;
    public float fireInterval = 0.5f;
    public float followDistance = 5f;
    
    [Header("State")]
    public BotRole role = BotRole.Assault;
    
    protected NavMeshAgent agent;
    protected Health health;
    protected BotState currentState;
    protected GameObject currentTarget;
    protected Vector3 patrolPoint;
    protected float nextFireTime;
    protected GameObject owner;
    
    public BotState CurrentState => currentState;
    public GameObject Owner => owner;
    public float detectionRange => perception != null ? perception.visionRange : 30f;
    
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        perception = GetComponent<BotPerception>();
        
        if (role == BotRole.Assault)
        {
            GetComponent<AssaultBot>()?.Initialize(this);
        }
    }
    
    protected virtual void Start()
    {
        SetState(BotState.Patrol);
        PickNewPatrolPoint();
        
        health.onDeath.AddListener(OnDeath);
    }
    
    protected virtual void Update()
    {
        if (health.IsDead) return;
        
        UpdateState();
        UpdatePerception();
    }
    
    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case BotState.Patrol:
                UpdatePatrol();
                break;
            case BotState.Chase:
                UpdateChase();
                break;
            case BotState.Attack:
                UpdateAttack();
                break;
            case BotState.Retreat:
                UpdateRetreat();
                break;
        }
    }
    
    protected virtual void UpdatePatrol()
    {
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            PickNewPatrolPoint();
        }
    }
    
    protected virtual void UpdateChase()
    {
        if (currentTarget == null)
        {
            SetState(BotState.Patrol);
            return;
        }
        
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        if (distance <= attackDistance)
        {
            SetState(BotState.Attack);
        }
        else if (distance > chaseDistance)
        {
            currentTarget = null;
            SetState(BotState.Patrol);
        }
        else
        {
            agent.SetDestination(currentTarget.transform.position);
        }
    }
    
    protected virtual void UpdateAttack()
    {
        if (currentTarget == null)
        {
            SetState(BotState.Patrol);
            return;
        }
        
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        if (distance > attackDistance * 1.2f)
        {
            SetState(BotState.Chase);
            return;
        }
        
        if (health.HealthPercent < retreatHealthPercent)
        {
            SetState(BotState.Retreat);
            return;
        }
        
        agent.ResetPath();
        AimAtTarget(currentTarget.transform.position);
        
        if (Time.time >= nextFireTime && weapon != null)
        {
            weapon.Fire(gameObject);
            nextFireTime = Time.time + fireInterval;
        }
    }
    
    protected virtual void UpdateRetreat()
    {
        Vector3 retreatDirection = (transform.position - currentTarget.transform.position).normalized;
        Vector3 retreatPoint = transform.position + retreatDirection * 10f;
        
        if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        
        if (health.HealthPercent > retreatHealthPercent + 0.2f)
        {
            SetState(BotState.Attack);
        }
    }
    
    protected virtual void UpdatePerception()
    {
        if (perception == null) return;
        
        GameObject enemy = perception.GetNearestEnemy();
        
        if (enemy != null && currentState == BotState.Patrol)
        {
            currentTarget = enemy;
            SetState(BotState.Chase);
        }
    }
    
    protected virtual void AimAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, aimSpeed * Time.deltaTime);
    }
    
    protected virtual void PickNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            agent.SetDestination(patrolPoint);
        }
    }
    
    public virtual void SetState(BotState newState)
    {
        currentState = newState;
    }
    
    public virtual void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
    
    public virtual void SetTeam(string team)
    {
        health.SetTeam(team);
    }
    
    protected virtual void OnDeath(GameObject killer)
    {
        agent.ResetPath();
        gameObject.SetActive(false);
    }
}

public enum BotState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Retreat
}

public enum BotRole
{
    Assault,
    Support,
    Sniper
}