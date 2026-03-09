using UnityEngine;

namespace NeuralStrike.Bots
{
    /// <summary>
    /// Courier bot that fetches items, delivers supplies, and provides utility.
    /// Can carry extra ammo/health packs for owner.
    /// </summary>
    public class CourierBot : BotController
    {
        [Header("Courier Settings")]
        [SerializeField] private int maxCarryCapacity = 3;
        [SerializeField] private float deliveryRange = 2f;
        
        private int itemsCarried = 0;
        
        public int ItemsCarried => itemsCarried;
        public bool CanCarryMore => itemsCarried < maxCarryCapacity;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Couriers are faster and less combat-focused
            agent.speed *= 1.2f;
            attackDamage *= 0.7f; // Weaker in combat
        }
        
        /// <summary>
        /// Pick up item (called by item pickup system).
        /// </summary>
        public bool PickupItem(GameObject item)
        {
            if (!CanCarryMore) return false;
            
            itemsCarried++;
            Destroy(item); // Remove item from world
            Debug.Log($"Courier picked up item. Carrying: {itemsCarried}/{maxCarryCapacity}");
            return true;
        }
        
        /// <summary>
        /// Deliver items to owner when in range.
        /// </summary>
        private void TryDeliverItems()
        {
            if (itemsCarried == 0) return;
            if (owner == null) return;
            
            float distanceToOwner = Vector3.Distance(transform.position, owner.transform.position);
            if (distanceToOwner <= deliveryRange)
            {
                // TODO: Actually give items to owner (health, ammo, etc.)
                Debug.Log($"Courier delivered {itemsCarried} items to {owner.name}");
                itemsCarried = 0;
            }
        }
        
        protected override void UpdateFollow()
        {
            base.UpdateFollow();
            TryDeliverItems();
        }
        
        /// <summary>
        /// Courier avoids combat and focuses on support.
        /// </summary>
        protected override void UpdateAttack()
        {
            // Couriers are less aggressive - retreat if threatened
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distance < attackRange * 0.5f)
                {
                    // Retreat toward owner
                    if (owner != null)
                    {
                        agent.SetDestination(owner.transform.position);
                    }
                    else
                    {
                        // Flee
                        Vector3 fleeDirection = (transform.position - currentTarget.transform.position).normalized;
                        agent.SetDestination(transform.position + fleeDirection * 10f);
                    }
                }
            }
        }
    }
}