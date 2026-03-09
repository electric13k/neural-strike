using UnityEngine;

public class SpyBot : MonoBehaviour
{
    private BotController controller;
    [Header("Spy Behavior")] public float backstabAngle = 60f; public float backstabDamageMultiplier = 3f;
    private void Awake() { controller = GetComponent<BotController>(); if (controller != null) { controller.role = BotRole.Spy; controller.attackDistance = 5f; } var agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); if (agent != null) agent.speed = 6f; }
    private void Update() { if (controller == null) return; CheckBackstab(); }
    private void CheckBackstab() { var target = controller.perception?.GetClosestEnemy(); if (target == null) return; Vector3 directionToBot = (transform.position - target.transform.position).normalized; Vector3 targetForward = target.transform.forward; float angle = Vector3.Angle(targetForward, directionToBot); if (angle < backstabAngle) { if (controller.weapon != null) { float originalDamage = controller.weapon.damage; controller.weapon.damage = originalDamage * backstabDamageMultiplier; StartCoroutine(ResetDamageAfterFrame(originalDamage)); } } }
    private System.Collections.IEnumerator ResetDamageAfterFrame(float originalDamage) { yield return new WaitForEndOfFrame(); if (controller.weapon != null) controller.weapon.damage = originalDamage; }
    private void OnDrawGizmosSelected() { Gizmos.color = Color.red; Vector3 leftBound = Quaternion.Euler(0, -backstabAngle / 2f, 0) * -transform.forward * 3f; Vector3 rightBound = Quaternion.Euler(0, backstabAngle / 2f, 0) * -transform.forward * 3f; Gizmos.DrawLine(transform.position, transform.position + leftBound); Gizmos.DrawLine(transform.position, transform.position + rightBound); }
}