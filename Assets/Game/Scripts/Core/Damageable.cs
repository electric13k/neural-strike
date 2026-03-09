using UnityEngine;

namespace NeuralStrike.Core
{
    /// <summary>
    /// Interface for entities that can receive damage.
    /// Implemented by anything that needs to react to weapon hits.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject damager = null);
        bool IsDead { get; }
        Team Team { get; }
    }
    
    /// <summary>
    /// Helper component that forwards damage to a Health component.
    /// Attach to colliders/rigidbodies that should receive damage.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        [SerializeField] private Health healthComponent;
        [SerializeField] private float damageMultiplier = 1f; // For headshot zones, etc.
        
        public bool IsDead => healthComponent != null && healthComponent.IsDead;
        public Team Team => healthComponent != null ? healthComponent.Team : Team.Neutral;
        
        void Awake()
        {
            if (healthComponent == null)
            {
                healthComponent = GetComponentInParent<Health>();
            }
        }
        
        public void TakeDamage(float amount, GameObject damager = null)
        {
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(amount * damageMultiplier, damager);
            }
        }
        
        /// <summary>
        /// Set damage multiplier (2.0 for headshot zones, 0.5 for armored areas, etc.).
        /// </summary>
        public void SetMultiplier(float multiplier)
        {
            damageMultiplier = multiplier;
        }
    }
}