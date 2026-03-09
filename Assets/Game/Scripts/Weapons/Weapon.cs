using UnityEngine;
using NeuralStrike.Core;

namespace NeuralStrike.Weapons
{
    /// <summary>
    /// Base weapon class for all firearms and melee weapons.
    /// Handles ammo, fire rate, reloading, and basic weapon logic.
    /// </summary>
    public abstract class Weapon : MonoBehaviour
    {
        [Header("Weapon Stats")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected float damage = 25f;
        [SerializeField] protected float fireRate = 0.1f; // Time between shots
        [SerializeField] protected float range = 100f;
        
        [Header("Ammo")]
        [SerializeField] protected int maxAmmo = 30;
        [SerializeField] protected int currentAmmo;
        [SerializeField] protected int reserveAmmo = 90;
        [SerializeField] protected float reloadTime = 2f;
        [SerializeField] protected bool infiniteAmmo = false;
        
        [Header("Effects")]
        [SerializeField] protected GameObject muzzleFlashPrefab;
        [SerializeField] protected Transform muzzlePoint;
        [SerializeField] protected AudioClip fireSound;
        [SerializeField] protected AudioClip reloadSound;
        [SerializeField] protected AudioClip emptySound;
        
        protected float nextFireTime = 0f;
        protected bool isReloading = false;
        protected AudioSource audioSource;
        
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => maxAmmo;
        public int ReserveAmmo => reserveAmmo;
        public bool IsReloading => isReloading;
        public string WeaponName => weaponName;
        
        protected virtual void Awake()
        {
            currentAmmo = maxAmmo;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        protected virtual void Update()
        {
            // Subclasses handle input and firing
        }
        
        /// <summary>
        /// Attempt to fire the weapon.
        /// </summary>
        public virtual bool TryFire()
        {
            if (Time.time < nextFireTime) return false;
            if (isReloading) return false;
            
            if (currentAmmo <= 0)
            {
                PlaySound(emptySound);
                return false;
            }
            
            Fire();
            return true;
        }
        
        /// <summary>
        /// Fire the weapon (implemented by subclasses).
        /// </summary>
        protected abstract void Fire();
        
        /// <summary>
        /// Consume ammo and update fire rate timer.
        /// </summary>
        protected virtual void ConsumeAmmo()
        {
            if (!infiniteAmmo)
            {
                currentAmmo--;
            }
            nextFireTime = Time.time + fireRate;
        }
        
        /// <summary>
        /// Start reload coroutine.
        /// </summary>
        public virtual void Reload()
        {
            if (isReloading) return;
            if (currentAmmo >= maxAmmo) return;
            if (reserveAmmo <= 0 && !infiniteAmmo) return;
            
            StartCoroutine(ReloadCoroutine());
        }
        
        protected virtual System.Collections.IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            PlaySound(reloadSound);
            
            yield return new WaitForSeconds(reloadTime);
            
            if (!infiniteAmmo)
            {
                int ammoNeeded = maxAmmo - currentAmmo;
                int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
                currentAmmo += ammoToReload;
                reserveAmmo -= ammoToReload;
            }
            else
            {
                currentAmmo = maxAmmo;
            }
            
            isReloading = false;
        }
        
        /// <summary>
        /// Add ammo to reserve (from pickups).
        /// </summary>
        public virtual void AddAmmo(int amount)
        {
            reserveAmmo += amount;
        }
        
        /// <summary>
        /// Spawn muzzle flash effect.
        /// </summary>
        protected virtual void PlayMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }
        
        /// <summary>
        /// Play weapon sound.
        /// </summary>
        protected virtual void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}