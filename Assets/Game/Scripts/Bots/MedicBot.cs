using UnityEngine;
using NeuralStrike.Core;

namespace NeuralStrike.Bots
{
    /// <summary>
    /// Medic bot that heals nearby allies and owner.
    /// Prioritizes low-health targets and stays near owner.
    /// </summary>
    public class MedicBot : BotController
    {
        [Header("Medic Settings")]
        [SerializeField] private float healAmount = 20f;
        [SerializeField] private float healRange = 5f;
        [SerializeField] private float healCooldown = 2f;
        [SerializeField] private GameObject healEffectPrefab;
        
        private float nextHealTime;
        private GameObject healTarget;
        
        protected override void Update()
        {
            base.Update();
            
            // Check for allies needing healing
            if (Time.time >= nextHealTime)
            {
                CheckForHealTargets();
            }
        }
        
        /// <summary>
        /// Find and heal injured allies.
        /// </summary>
        private void CheckForHealTargets()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, healRange);
            
            GameObject lowestHealthAlly = null;
            float lowestHealthPercent = 1f;
            
            foreach (Collider col in nearby)
            {
                Health targetHealth = col.GetComponent<Health>();
                if (targetHealth == null) continue;
                if (targetHealth == health) continue; // Don't heal self
                if (targetHealth.Team != health.Team) continue; // Same team only
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
        
        /// <summary>
        /// Heal specific target.
        /// </summary>
        private void HealTarget(GameObject target)
        {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.Heal(healAmount);
                nextHealTime = Time.time + healCooldown;
                
                // Spawn heal effect
                if (healEffectPrefab != null)
                {
                    GameObject effect = Instantiate(healEffectPrefab, target.transform.position, Quaternion.identity);
                    Destroy(effect, 2f);
                }
                
                Debug.Log($"Medic bot healed {target.name} for {healAmount} HP");
            }
        }
        
        protected override void UpdateFollow()
        {
            // Medic stays closer to owner than other bots
            if (owner != null)
            {
                float closeFollowDistance = followDistance * 0.7f;
                agent.SetDestination(owner.transform.position);
                
                if (Vector3.Distance(transform.position, owner.transform.position) <= closeFollowDistance)
                {
                    agent.ResetPath();
                    SetState(BotState.Idle);
                }
            }
        }
    }
}