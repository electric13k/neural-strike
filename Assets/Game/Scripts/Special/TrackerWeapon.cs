using UnityEngine;

public class TrackerWeapon : HitscanWeapon
{
    [Header("Tracker")]
    public float trackerChance = 0.2f;
    
    protected override void PerformFire(GameObject owner)
    {
        if (muzzle == null) return;
        
        Vector3 direction = GetSpreadDirection(muzzle.forward);
        
        if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Apply damage
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo info = new DamageInfo(hit.point, hit.normal, direction, owner, gameObject);
                damageable.ApplyDamage(damage, info);
            }
            
            // Tracker chance
            if (Random.value <= trackerChance)
            {
                TrackerBullet.AttachTracker(hit.collider.gameObject, hit.point);
                Debug.Log($"Tracker attached to {hit.collider.gameObject.name}");
            }
            
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(direction * impactForce, ForceMode.Impulse);
            }
        }
    }
    
    private Vector3 GetSpreadDirection(Vector3 forward)
    {
        float spreadRad = spreadAngle * Mathf.Deg2Rad;
        float randYaw = Random.Range(-spreadRad, spreadRad);
        float randPitch = Random.Range(-spreadRad, spreadRad);
        Quaternion spreadRot = Quaternion.Euler(randPitch * Mathf.Rad2Deg, randYaw * Mathf.Rad2Deg, 0f);
        return spreadRot * forward;
    }
}