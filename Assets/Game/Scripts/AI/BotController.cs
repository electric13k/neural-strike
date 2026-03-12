using UnityEngine;
using UnityEngine.AI;

public enum BotState { Idle, Patrol, Chase, Attack, Retreat, Dead }
public enum BotRole { Assault, Medic, Sniper, Spy, Heavy }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class BotController : MonoBehaviour
{
    [Header("Role")] public BotRole role = BotRole.Assault;
    [Header("References")] public BotPerception perception; public Weapon weapon; public Transform firePoint;
    [Header("Movement")] public float patrolRadius = 20f; public float chaseDistance = 30f; public float attackDistance = 15f; public float retreatHealthPercent = 0.3f;
    [Header("Combat")] public float aimSpeed = 5f; public float fireInterval = 0.5f;
    [Header("Patrol")] public float patrolWaitTime = 3f;

    private NavMeshAgent agent;
    private Health health;
    private BotState currentState = BotState.Idle;
    private GameObject currentTarget;
    private float lastFireTime;
    private float patrolWaitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        if (perception == null) perception = GetComponent<BotPerception>();
    }

    private void Start()
    {
        health.onDeath.AddListener(OnDeath);
        SetState(BotState.Patrol);
        ApplyRoleModifiers();
    }

    private void Update()
    {
        if (currentState == BotState.Dead) return;
        UpdateState();
        ExecuteState();
    }

    private void ApplyRoleModifiers()
    {
        switch (role)
        {
            case BotRole.Assault: agent.speed = 5f; attackDistance = 15f; break;
            case BotRole.Medic:   agent.speed = 4.5f; attackDistance = 10f; break;
            case BotRole.Sniper:  agent.speed = 3.5f; attackDistance = 40f; break;
            case BotRole.Spy:     agent.speed = 6f; attackDistance = 8f; break;
            case BotRole.Heavy:   agent.speed = 3f; attackDistance = 12f; health.maxHealth = 150f; break;
        }
    }

    private void UpdateState()
    {
        currentTarget = perception != null ? perception.GetClosestEnemy() : null;
        switch (currentState)
        {
            case BotState.Idle:
            case BotState.Patrol:
                if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) <= chaseDistance)
                    SetState(BotState.Chase);
                break;
            case BotState.Chase:
                if (currentTarget == null) SetState(BotState.Patrol);
                else if (Vector3.Distance(transform.position, currentTarget.transform.position) <= attackDistance) SetState(BotState.Attack);
                break;
            case BotState.Attack:
                if (currentTarget == null) SetState(BotState.Patrol);
                else if (Vector3.Distance(transform.position, currentTarget.transform.position) > attackDistance * 1.2f) SetState(BotState.Chase);
                break;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case BotState.Idle:    agent.isStopped = true; break;
            case BotState.Patrol:  ExecutePatrol(); break;
            case BotState.Chase:   ExecuteChase(); break;
            case BotState.Attack:  ExecuteAttack(); break;
            case BotState.Retreat: ExecuteRetreat(); break;
        }
    }

    private void ExecutePatrol()
    {
        agent.isStopped = false;
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            if (patrolWaitTimer > 0) patrolWaitTimer -= Time.deltaTime;
            else { SetNewPatrolDestination(); patrolWaitTimer = patrolWaitTime; }
        }
    }

    private void ExecuteChase()
    {
        agent.isStopped = false;
        if (currentTarget != null) agent.SetDestination(currentTarget.transform.position);
    }

    private void ExecuteAttack()
    {
        agent.isStopped = true;
        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, aimSpeed * Time.deltaTime);
            if (weapon != null && Time.time >= lastFireTime + fireInterval)
            {
                weapon.TryFire(gameObject);
                lastFireTime = Time.time;
            }
        }
    }

    private void ExecuteRetreat()
    {
        agent.isStopped = false;
        if (currentTarget != null)
        {
            Vector3 dirAway = (transform.position - currentTarget.transform.position).normalized;
            Vector3 retreatPos = transform.position + dirAway * 10f;
            if (NavMesh.SamplePosition(retreatPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    private void SetState(BotState newState) { currentState = newState; }

    private void SetNewPatrolDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        randomDir.y = transform.position.y;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // Matches Health.DeathEvent = UnityEvent<GameObject>
    private void OnDeath(GameObject killer)
    {
        SetState(BotState.Dead);
        agent.isStopped = true;
        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;   Gizmos.DrawWireSphere(transform.position, patrolRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
