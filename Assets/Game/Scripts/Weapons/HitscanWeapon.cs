using UnityEngine;
using NeuralStrike.Core;

namespace NeuralStrike.Weapons
{
    /// <summary>
    /// Hitscan weapon using raycasts for instant bullet travel.
    /// Used for rifles, pistols, SMGs, etc.
    /// </summary>
    public class HitscanWeapon : Weapon
    {
        [Header("Hitscan Settings")]
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private float spread = 0f; // Bullet spread in degrees
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private GameObject bloodEffectPrefab;
        [SerializeField] private LineRenderer bulletTrailPrefab;
        [SerializeField] private float trailDuration = 0.05f;
        
        [Header("Input")]
        [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
        [SerializeField] private bool isAutomatic = true;
        
        protected override void Update()
        {
            base.Update();
            
            // Handle firing input
            if (isAutomatic && Input.GetKey(fireKey))
            {
                TryFire();
            }
            else if (!isAutomatic && Input.GetKeyDown(fireKey))
            {
                TryFire();
            }
            
            // Handle reload input
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }
        
        protected override void Fire()
        {
            PlaySound(fireSound);
            PlayMuzzleFlash();
            ConsumeAmmo();
            
            // Calculate shot direction with spread
            Vector3 shootDirection = CalculateShootDirection();
            
            // Perform raycast
            if (Physics.Raycast(muzzlePoint.position, shootDirection, out RaycastHit hit, range, hitLayers))
            {
                // Hit something
                HandleHit(hit);
                
                // Draw bullet trail to hit point
                DrawBulletTrail(muzzlePoint.position, hit.point);
            }
            else
            {
                // Miss - draw trail to max range
                DrawBulletTrail(muzzlePoint.position, muzzlePoint.position + shootDirection * range);
            }
        }
        
        /// <summary>
        /// Calculate shoot direction with random spread.
        /// </summary>
        private Vector3 CalculateShootDirection()
        {
            Vector3 direction = muzzlePoint.forward;
            
            if (spread > 0)
            {
                // Add random spread
                direction += new Vector3(
                    Random.Range(-spread, spread),
                    Random.Range(-spread, spread),
                    0
                );
                direction.Normalize();
            }
            
            return direction;
        }
        
        /// <summary>
        /// Handle what was hit by the raycast.
        /// </summary>
        protected virtual void HandleHit(RaycastHit hit)
        {
            // Try to damage the hit object
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, gameObject);
                
                // Spawn blood effect if hit a living target
                if (bloodEffectPrefab != null)
                {
                    GameObject blood = Instantiate(bloodEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(blood, 2f);
                }
            }
            else
            {
                // Spawn impact effect on surfaces
                if (impactEffectPrefab != null)
                {
                    GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 2f);
                }
            }
        }
        
        /// <summary>
        /// Draw bullet trail line renderer.
        /// </summary>
        protected virtual void DrawBulletTrail(Vector3 startPos, Vector3 endPos)
        {
            if (bulletTrailPrefab != null)
            {
                LineRenderer trail = Instantiate(bulletTrailPrefab);
                trail.SetPosition(0, startPos);
                trail.SetPosition(1, endPos);
                Destroy(trail.gameObject, trailDuration);
            }
        }
    }
}