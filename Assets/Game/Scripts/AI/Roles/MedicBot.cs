using UnityEngine;

public class MedicBot : MonoBehaviour
{
    private BotController controller;
    [Header("Medic Behavior")] public float healRange = 10f; public float healAmount = 20f; public float healCooldown = 5f; public LayerMask allyMask;
    private float lastHealTime;
    private void Awake() { controller = GetComponent<BotController>(); if (controller != null) { controller.role = BotRole.Medic; controller.retreatHealthPercent = 0.5f; } }
    private void Update() { if (controller == null) return; if (Time.time >= lastHealTime + healCooldown) TryHealAlly(); }
    private void TryHealAlly() { Collider[] allies = Physics.OverlapSphere(transform.position, healRange, allyMask); Health lowestHealthAlly = null; float lowestHealthPercent = 1f; foreach (Collider ally in allies) { if (ally.gameObject == gameObject) continue; Health allyHealth = ally.GetComponent<Health>(); if (allyHealth != null) { float healthPercent = allyHealth.maxHealth > 0 ? (allyHealth.maxHealth / allyHealth.maxHealth) : 1f; if (healthPercent < lowestHealthPercent && healthPercent < 0.8f) { lowestHealthPercent = healthPercent; lowestHealthAlly = allyHealth; } } } if (lowestHealthAlly != null) { lowestHealthAlly.Heal(healAmount); lastHealTime = Time.time; } }
    private void OnDrawGizmosSelected() { Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, healRange); }
}