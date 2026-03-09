using UnityEngine;

public class AssaultBot : MonoBehaviour
{
    private BotController controller;
    [Header("Assault Behavior")] public float flankCheckInterval = 5f; public float flankDistance = 10f;
    private float lastFlankCheck;
    private void Awake() { controller = GetComponent<BotController>(); if (controller != null) controller.role = BotRole.Assault; }
    private void Update() { if (controller == null) return; if (Time.time >= lastFlankCheck + flankCheckInterval) { TryFlank(); lastFlankCheck = Time.time; } }
    private void TryFlank() { var target = controller.perception?.GetClosestEnemy(); if (target == null) return; Vector3 directionToTarget = (target.transform.position - transform.position).normalized; Vector3 flankDirection = Vector3.Cross(directionToTarget, Vector3.up).normalized; if (Random.value > 0.5f) flankDirection = -flankDirection; Vector3 flankPosition = transform.position + flankDirection * flankDistance; if (UnityEngine.AI.NavMesh.SamplePosition(flankPosition, out UnityEngine.AI.NavMeshHit hit, flankDistance, UnityEngine.AI.NavMesh.AllAreas)) GetComponent<UnityEngine.AI.NavMeshAgent>()?.SetDestination(hit.position); }
}