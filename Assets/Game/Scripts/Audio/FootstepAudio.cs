using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] footstepSounds;
    
    [Header("Settings")]
    public float stepInterval = 0.5f;
    public float walkVolume = 0.3f;
    public float sprintVolume = 0.5f;
    
    private AudioSource audioSource;
    private CharacterController controller;
    private float stepTimer;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        
        controller = GetComponent<CharacterController>();
    }
    
    private void Update()
    {
        if (controller == null || footstepSounds.Length == 0) return;
        
        bool isMoving = controller.velocity.magnitude > 0.1f;
        bool isGrounded = controller.isGrounded;
        
        if (isMoving && isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = stepInterval * 0.5f;
        }
    }
    
    private void PlayFootstep()
    {
        if (footstepSounds.Length == 0) return;
        
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float volume = isSprinting ? sprintVolume : walkVolume;
        
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();
    }
}