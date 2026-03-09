using UnityEngine;

public class HeavyBot : MonoBehaviour
{
    private BotController controller;
    [Header("Heavy Behavior")] public float suppressionRange = 20f; public float holdPositionDuration = 10f; public float positionRadius = 2f;
    private Vector3 holdPosition; private float holdPositionTimer; private bool isHoldingPosition;
    private void Awake() { controller = GetComponent<BotController>(); if (controller != null) { controller.role = BotRole.Heavy; controller.retreatHealthPercent = 0.2f; } var agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); if (agent != null) { agent.speed = 3f; agent.acceleration = 4f; } if (controller.weapon != null) { controller.weapon.fireRate = 15f; controller.weapon.spreadAngle = 3f; controller.weapon.magazineSize = 100; controller.weapon.damage = 15f; } }
    private void Update() { if (controller == null) return; var target = controller.perception?.GetClosestEnemy(); if (target != null) { float distanceToTarget = Vector3.Distance(transform.position, target.transform.position); if (distanceToTarget <= suppressionRange) { if (!isHoldingPosition) StartHoldingPosition(); } } if (isHoldingPosition) { holdPositionTimer -= Time.deltaTime; if (holdPositionTimer <= 0f) isHoldingPosition = false; else { if (Vector3.Distance(transform.position, holdPosition) > positionRadius) { var agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); agent?.SetDestination(holdPosition); } } } }
    private void StartHoldingPosition() { isHoldingPosition = true; holdPosition = transform.position; holdPositionTimer = holdPositionDuration; }
    private void OnDrawGizmosSelected() { if (isHoldingPosition) { Gizmos.color = Color.blue; Gizmos.DrawWireSphere(holdPosition, positionRadius); } Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, suppressionRange); }
}