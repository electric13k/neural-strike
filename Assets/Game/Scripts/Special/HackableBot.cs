using UnityEngine;

public class HackableBot : MonoBehaviour, IHackable
{
    [Header("Hack Settings")]
    public string hackPrompt = "[E] Hack Bot";
    public float virusDuration = 10f;
    public float virusDamagePerSecond = 5f;
    
    private bool isHacked;
    private float virusTimeRemaining;
    private BotController botController;
    private Health health;
    
    private void Awake()
    {
        botController = GetComponent<BotController>();
        health = GetComponent<Health>();
    }
    
    private void Update()
    {
        if (isHacked)
        {
            virusTimeRemaining -= Time.deltaTime;
            
            // Apply virus damage
            if (health != null)
            {
                float damage = virusDamagePerSecond * Time.deltaTime;
                DamageInfo info = new DamageInfo(
                    transform.position,
                    Vector3.up,
                    Vector3.zero,
                    null,
                    null
                );
                health.ApplyDamage(damage, info);
            }
            
            if (virusTimeRemaining <= 0f)
            {
                isHacked = false;
            }
        }
    }
    
    public void OnHackComplete(GameObject hacker)
    {
        isHacked = true;
        virusTimeRemaining = virusDuration;
        
        // Disable bot temporarily
        if (botController != null)
        {
            botController.enabled = false;
            StartCoroutine(ReenableBotAfterDelay());
        }
        
        Debug.Log($"{gameObject.name} has been hacked! Virus active for {virusDuration}s");
    }
    
    private System.Collections.IEnumerator ReenableBotAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (botController != null)
        {
            botController.enabled = true;
        }
    }
    
    public string GetHackPrompt()
    {
        return isHacked ? "Already Hacked" : hackPrompt;
    }
    
    public bool IsHacked() { return isHacked; }
}