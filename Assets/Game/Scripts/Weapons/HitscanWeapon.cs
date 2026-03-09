using UnityEngine;

public class HitscanWeapon : Weapon
{
    [Header("Hitscan Settings")]
    public float range = 150f;
    public float impactForce = 5f;

    public override void TryFire(GameObject owner)
    {
        base.TryFire(owner);
    }

    protected override void PerformFire(GameObject owner)
    {
        if (muzzle == null)
        {
            Debug.LogWarning($"{weaponName}: muzzle not assigned.");
            return;
        }

        Vector3 direction = GetSpreadDirection(muzzle.forward);

        if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                DamageInfo info = new DamageInfo(
                    hit.point,
                    hit.normal,
                    direction,
                    owner,
                    gameObject
                );
                damageable.ApplyDamage(damage, info);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(direction * impactForce, ForceMode.Impulse);
            }

            // TODO: spawn impact VFX / decals (later batch)
        }
    }

    private Vector3 GetSpreadDirection(Vector3 forward)
    {
        float spreadRad = spreadAngle * Mathf.Deg2Rad;
        float randYaw = Random.Range(-spreadRad, spreadRad);
        float randPitch = Random.Range(-spreadRad, spreadRad);

        Quaternion spreadRot = Quaternion.Euler(
            randPitch * Mathf.Rad2Deg,
            randYaw * Mathf.Rad2Deg,
            0f
        );

        return spreadRot * forward;
    }
}