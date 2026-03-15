using UnityEngine;

// ============================================================
//  SHOTGUN  — Neural Strike
//  Fires multiple pellets per shot via repeated raycasts.
// ============================================================

public class Shotgun : WeaponBase
{
    [Header("Shotgun Pellets")]
    public int pellets = 8;

    protected override void Awake()
    {
        weaponName  = "Wraith-12";
        damage      = 12f;      // per pellet
        fireRate    = 1.5f;
        magSize     = 6;
        range       = 20f;
        reloadTime  = 0.5f;    // pump each shell
        isAutomatic = false;
        spreadHip   = 8f;
        spreadADS   = 4f;
        base.Awake();
    }

    protected override void OnFire()
    {
        // Extra pellets (WeaponBase already fired one ray; fire pellets-1 more)
        for (int i = 0; i < pellets - 1; i++)
        {
            float spread   = _ads ? spreadADS : spreadHip;
            Vector3 dir    = transform.forward;
            dir            = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.up) * dir;
            dir            = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.right) * dir;
            Vector3 origin = fpCam ? fpCam.transform.position : transform.position;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask))
            {
                HealthSystem hs = hit.collider.GetComponentInParent<HealthSystem>();
                if (hs != null) hs.TakeDamage(damage, gameObject);
            }
        }
    }
}
