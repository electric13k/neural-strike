using UnityEngine;
using UnityEngine.Events;

namespace NeuralStrike.Core
{
    /// <summary>
    /// Health component for all damageable entities (players, bots, destructibles).
    /// Handles damage, healing, death events, and team ownership.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isInvulnerable = false;
        
        [Header("Team Settings")]
        [SerializeField] private Team team = Team.Neutral;
        
        [Header("Events")]
        public UnityEvent<float> OnDamaged;
        public UnityEvent<float> OnHealed;
        public UnityEvent<GameObject> OnDeath; // passes killer
        public UnityEvent<float, float> OnHealthChanged; // current, max
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsAlive => currentHealth > 0;
        public Team Team => team;
        public float HealthPercent => currentHealth / maxHealth;
        
        private GameObject lastDamager;
        
        void Awake()
        {
            currentHealth = maxHealth;
        }
        
        /// <summary>
        /// Apply damage to this entity.
        /// </summary>
        public void TakeDamage(float amount, GameObject damager = null)
        {
            if (isInvulnerable || IsDead) return;
            
            lastDamager = damager;
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);
            
            OnDamaged?.Invoke(amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (IsDead)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Heal this entity.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead) return;
            
            float oldHealth = currentHealth;
            currentHealth += amount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            
            float actualHealed = currentHealth - oldHealth;
            if (actualHealed > 0)
            {
                OnHealed?.Invoke(actualHealed);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }
        
        /// <summary>
        /// Set health to a specific value.
        /// </summary>
        public void SetHealth(float value)
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (IsDead)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Restore to full health.
        /// </summary>
        public void FullHeal()
        {
            SetHealth(maxHealth);
        }
        
        /// <summary>
        /// Change team (used for bot hacking).
        /// </summary>
        public void SetTeam(Team newTeam)
        {
            team = newTeam;
        }
        
        /// <summary>
        /// Enable/disable invulnerability (spawn protection, etc.).
        /// </summary>
        public void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }
        
        private void Die()
        {
            OnDeath?.Invoke(lastDamager);
            // Note: Respawn/destroy logic handled by GameManager or character-specific scripts
        }
    }
    
    /// <summary>
    /// Team enumeration for all entities.
    /// </summary>
    public enum Team
    {
        Neutral = 0,
        Alpha = 1,
        Bravo = 2,
        Tango = 3,
        Charlie = 4,
        // Duos mode color teams
        Red = 10,
        Blue = 11,
        Green = 12,
        Yellow = 13,
        Teal = 14,
        Cyan = 15,
        Purple = 16,
        Pink = 17,
        Orange = 18
    }
}