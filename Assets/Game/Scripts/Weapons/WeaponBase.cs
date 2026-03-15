using System.Collections;
using UnityEngine;

// ============================================================
//  WEAPON BASE  — Neural Strike
//  All guns inherit from this.  Handles:
//    fire-rate, ammo, reload, hitscan raycast, recoil.
//  Derive and override OnFire() for special bullet types.
// ============================================================

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Identity")]
    public string  weaponName  = "Weapon";
    public Sprite  icon;

    [Header("Stats")]
    public float damage      = 25f;
    public float fireRate    = 10f;     // rounds per second
    public float range       = 80f;
    public int   magSize     = 30;
    public float reloadTime  = 1.8f;
    public bool  isAutomatic = true;

    [Header("Spread (degrees, 0 = perfect)")]
    public float spreadHip   = 2f;
    public float spreadADS   = 0.4f;

    [Header("References")]
    public Transform   muzzle;           // muzzle point for ray origin & FX
    public Camera      fpCam;            // first-person camera (for ADS)
    public AudioClip   fireSound;
    public AudioClip   reloadSound;
    public GameObject  muzzleFlashPrefab;
    public LayerMask   hitMask = ~0;

    // ── State ──────────────────────────────────────────────────
    protected int   _ammo;
    protected int   _reserveAmmo;
    protected bool  _reloading;
    protected bool  _ads;
    private   float _nextFireTime;
    private   AudioSource _audio;

    public int  Ammo        => _ammo;
    public int  ReserveAmmo => _reserveAmmo;
    public bool IsReloading => _reloading;

    protected virtual void Awake()
    {
        _ammo        = magSize;
        _reserveAmmo = magSize * 4;
        _audio       = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        _ads = Input.GetMouseButton(1);   // right-click = ADS

        bool triggerHeld = isAutomatic
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (triggerHeld && CanFire())
            FireShot();

        if (Input.GetKeyDown(KeyCode.R) && !_reloading && _ammo < magSize)
            StartCoroutine(Reload());
    }

    // ── Core logic ────────────────────────────────────────────

    bool CanFire() =>
        !_reloading &&
        _ammo > 0 &&
        Time.time >= _nextFireTime;

    void FireShot()
    {
        _ammo--;
        _nextFireTime = Time.time + 1f / fireRate;

        // Muzzle flash
        if (muzzleFlashPrefab && muzzle)
            Destroy(Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation), 0.05f);

        // Sound
        if (fireSound) _audio.PlayOneShot(fireSound);

        // Raycast
        float spread  = _ads ? spreadADS : spreadHip;
        Vector3 dir   = GetSpreadDirection(spread);
        Vector3 origin = fpCam ? fpCam.transform.position : (muzzle ? muzzle.position : transform.position);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask))
        {
            OnHit(hit);
        }

        OnFire();

        if (_ammo == 0 && _reserveAmmo > 0)
            StartCoroutine(Reload());
    }

    Vector3 GetSpreadDirection(float spreadDeg)
    {
        Transform lookRef = fpCam ? fpCam.transform : transform;
        Vector3 forward   = lookRef.forward;
        float angle       = Random.Range(0f, spreadDeg);
        float rot         = Random.Range(0f, 360f);
        return Quaternion.AngleAxis(rot, forward) *
               Quaternion.AngleAxis(angle, lookRef.right) * forward;
    }

    IEnumerator Reload()
    {
        _reloading = true;
        if (reloadSound) _audio.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadTime);

        int needed = magSize - _ammo;
        int refill = Mathf.Min(needed, _reserveAmmo);
        _ammo        += refill;
        _reserveAmmo -= refill;
        _reloading    = false;
    }

    // ── Overrides ─────────────────────────────────────────────

    /// <summary>Called each shot — override for special fx/logic.</summary>
    protected virtual void OnFire() { }

    /// <summary>Called when the raycast hits something.</summary>
    protected virtual void OnHit(RaycastHit hit)
    {
        HealthSystem hs = hit.collider.GetComponentInParent<HealthSystem>();
        if (hs != null) hs.TakeDamage(damage, gameObject);
    }

    /// <summary>Externally add ammo (loot pickup).</summary>
    public void AddAmmo(int amount) => _reserveAmmo += amount;
}
