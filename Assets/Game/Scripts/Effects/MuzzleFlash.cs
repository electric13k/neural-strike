using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [Header("Visual")]
    public ParticleSystem flashParticles;
    public Light flashLight;
    public float lightDuration = 0.05f;
    
    [Header("Settings")]
    public float flashIntensity = 5f;
    public Color flashColor = Color.yellow;
    
    private float lightTimer;
    
    private void Start()
    {
        if (flashLight != null)
        {
            flashLight.intensity = 0f;
            flashLight.color = flashColor;
        }
    }
    
    private void Update()
    {
        if (lightTimer > 0f)
        {
            lightTimer -= Time.deltaTime;
            if (flashLight != null)
            {
                flashLight.intensity = Mathf.Lerp(0f, flashIntensity, lightTimer / lightDuration);
            }
        }
        else if (flashLight != null)
        {
            flashLight.intensity = 0f;
        }
    }
    
    public void PlayFlash()
    {
        if (flashParticles != null)
            flashParticles.Play();
        
        lightTimer = lightDuration;
    }
}