using UnityEngine;

// ============================================================
//  ASSAULT RIFLE  — Neural Strike
//  Standard automatic rifle — full-auto, moderate recoil.
//  Inherits everything from WeaponBase.
// ============================================================

public class AssaultRifle : WeaponBase
{
    protected override void Awake()
    {
        weaponName  = "Neural AR-7";
        damage      = 22f;
        fireRate    = 10f;
        magSize     = 30;
        range       = 70f;
        reloadTime  = 2f;
        isAutomatic = true;
        spreadHip   = 2.5f;
        spreadADS   = 0.6f;
        base.Awake();
    }
}
