using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DeathEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class DamageEvent : UnityEvent<float, DamageInfo> { }

public class Health : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public bool destroyOnDeath = false;
    public string team = "Team1"; // Add team system
    
    [Header("Events")]
    public DeathEvent onDeath;
    public DamageEvent onDamage;
    
    private float currentHealth;
    
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
    public string Team => team;
    
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public void ApplyDamage(float amount, DamageInfo info)
    {
        if (IsDead) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        onDamage?.Invoke(amount, info);
        
        if (currentHealth <= 0f)
        {
            Die(info);
        }
    }
    
    public void Heal(float amount)
    {
        if (IsDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
    }
    
    public void SetTeam(string newTeam)
    {
        team = newTeam;
    }
    
    private void Die(DamageInfo info)
    {
        onDeath?.Invoke(info.attacker);
        
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void Revive()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
    }
}