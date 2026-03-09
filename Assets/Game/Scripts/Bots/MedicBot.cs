using UnityEngine;

public class MedicBot : MonoBehaviour
{
    [Header("Medic Settings")]
    public float healAmount = 20f;
    public float healRange = 5f;
    public float healCooldown = 2f;
    public GameObject healEffectPrefab;
    
    private BotController controller;
    private Health health;
    private float nextHealTime;
    
    private void Awake()
    {
        controller = GetComponent<BotController>();
        health = GetComponent<Health>();
    }
    
    private void Update()
    {
        if (Time.time >= nextHealTime)
        {
            CheckForHealTargets();
        }
    }
    
    private void CheckForHealTargets()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, healRange);
        
        GameObject lowestHealthAlly = null;
        float lowestHealthPercent = 1f;
        
        foreach (Collider col in nearby)
        {
            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth == null) continue;
            if (targetHealth == health) continue;
            if (targetHealth.Team != health.Team) continue;
            if (targetHealth.IsDead) continue;
            
            float healthPercent = targetHealth.HealthPercent;
            if (healthPercent < 1f && healthPercent < lowestHealthPercent)
            {
                lowestHealthPercent = healthPercent;
                lowestHealthAlly = col.gameObject;
            }
        }
        
        if (lowestHealthAlly != null)
        {
            HealTarget(lowestHealthAlly);
        }
    }
    
    private void HealTarget(GameObject target)
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.Heal(healAmount);
            nextHealTime = Time.time + healCooldown;
            
            if (healEffectPrefab != null)
            {
                GameObject effect = Instantiate(healEffectPrefab, target.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
    }
}