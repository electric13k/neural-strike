using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class WeaponFireEvent : UnityEvent { }
[System.Serializable]
public class WeaponDryFireEvent : UnityEvent { }
[System.Serializable]
public class WeaponReloadEvent : UnityEvent { }

public abstract class Weapon : MonoBehaviour
{
    [Header("General")]
    public string weaponName = "Rifle";
    public float damage = 25f;
    public float fireRate = 10f;      // rounds per second
    public float spreadAngle = 1.5f;  // degrees
    public int magazineSize = 30;
    public int ammoInMagazine = 30;
    public int ammoReserve = 90;
    public float reloadTime = 1.8f;

    [Header("References")]
    public Transform muzzle;
    public LayerMask hitMask;

    [Header("Events")]
    public WeaponFireEvent onFire;
    public WeaponDryFireEvent onDryFire;
    public WeaponReloadEvent onReload;

    protected bool isReloading;
    protected float nextFireTime;

    protected virtual void Awake()
    {
        if (ammoInMagazine > magazineSize)
            ammoInMagazine = magazineSize;
    }

    public virtual void TryFire(GameObject owner)
    {
        if (Time.time < nextFireTime) return;
        if (isReloading) return;

        if (ammoInMagazine <= 0)
        {
            onDryFire.Invoke();
            return;
        }

        ammoInMagazine--;
        nextFireTime = Time.time + (1f / fireRate);
        onFire.Invoke();

        PerformFire(owner);
    }

    protected abstract void PerformFire(GameObject owner);

    public virtual void TryReload()
    {
        if (isReloading) return;
        if (ammoInMagazine >= magazineSize) return;
        if (ammoReserve <= 0) return;

        onReload.Invoke();
        CoroutineRunner.Instance.StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - ammoInMagazine;
        int toLoad = Mathf.Min(needed, ammoReserve);
        ammoInMagazine += toLoad;
        ammoReserve -= toLoad;

        isReloading = false;
    }
}