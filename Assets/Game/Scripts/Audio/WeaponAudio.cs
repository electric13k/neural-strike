using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound;
    
    [Header("Settings")]
    public float fireVolume = 1f;
    public float reloadVolume = 0.8f;
    public float dryFireVolume = 0.5f;
    
    private Weapon weapon;
    private AudioSource audioSource;
    
    private void Awake()
    {
        weapon = GetComponent<Weapon>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }
    
    private void Start()
    {
        if (weapon != null)
        {
            weapon.onFire.AddListener(PlayFireSound);
            weapon.onReload.AddListener(PlayReloadSound);
            weapon.onDryFire.AddListener(PlayDryFireSound);
        }
    }
    
    private void PlayFireSound()
    {
        if (fireSound != null)
        {
            audioSource.clip = fireSound;
            audioSource.volume = fireVolume;
            audioSource.Play();
        }
    }
    
    private void PlayReloadSound()
    {
        if (reloadSound != null)
        {
            audioSource.clip = reloadSound;
            audioSource.volume = reloadVolume;
            audioSource.Play();
        }
    }
    
    private void PlayDryFireSound()
    {
        if (dryFireSound != null)
        {
            audioSource.clip = dryFireSound;
            audioSource.volume = dryFireVolume;
            audioSource.Play();
        }
    }
}