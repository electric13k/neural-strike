using UnityEngine;

public class SniperBot : MonoBehaviour
{
    private BotController controller;
    [Header("Sniper Behavior")] public float vantagePointCheckInterval = 10f; public float vantagePointRadius = 30f; public float minVantageHeight = 2f;
    private float lastVantageCheck; private Vector3 currentVantagePoint; private bool hasVantagePoint;
    private void Awake() { controller = GetComponent<BotController>(); if (controller != null) { controller.role = BotRole.Sniper; controller.attackDistance = 50f; controller.fireInterval = 1.5f; } if (controller.weapon != null) { controller.weapon.damage = 50f; controller.weapon.spreadAngle = 0.5f; } }
    private void Update() { if (controller == null) return; if (Time.time >= lastVantageCheck + vantagePointCheckInterval) { FindVantagePoint(); lastVantageCheck = Time.time; } if (hasVantagePoint && controller.perception?.GetClosestEnemy() == null) { var agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); if (agent != null && Vector3.Distance(transform.position, currentVantagePoint) > 2f) agent.SetDestination(currentVantagePoint); } }
    private void FindVantagePoint() { float bestHeight = transform.position.y; Vector3 bestPosition = transform.position; bool foundBetter = false; for (int i = 0; i < 10; i++) { Vector3 randomDirection = Random.insideUnitSphere * vantagePointRadius; randomDirection += transform.position; if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit hit, vantagePointRadius, UnityEngine.AI.NavMesh.AllAreas)) { if (hit.position.y > bestHeight + minVantageHeight) { bestHeight = hit.position.y; bestPosition = hit.position; foundBetter = true; } } } if (foundBetter) { currentVantagePoint = bestPosition; hasVantagePoint = true; } }
    private void OnDrawGizmosSelected() { if (hasVantagePoint) { Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(currentVantagePoint, 1f); Gizmos.DrawLine(transform.position, currentVantagePoint); } }
}