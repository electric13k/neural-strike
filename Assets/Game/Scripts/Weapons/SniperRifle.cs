using UnityEngine;

// ============================================================
//  SNIPER RIFLE  — Neural Strike
//  Bolt-action, high damage, tight spread when ADS.
// ============================================================

public class SniperRifle : WeaponBase
{
    protected override void Awake()
    {
        weaponName  = "Phantom MK-1";
        damage      = 95f;
        fireRate    = 0.8f;
        magSize     = 5;
        range       = 300f;
        reloadTime  = 3f;
        isAutomatic = false;
        spreadHip   = 4f;
        spreadADS   = 0f;
        base.Awake();
    }
}
