using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthChangedEvent : UnityEvent<float, float> { } // current, max
[System.Serializable]
public class DeathEvent : UnityEvent<DamageInfo> { }

public class Health : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public bool destroyOnDeath = false;

    [Header("Events")]
    public HealthChangedEvent onHealthChanged;
    public DeathEvent onDeath;

    private float currentHealth;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    public void ApplyDamage(float damage, DamageInfo info)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        onHealthChanged.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die(info);
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    private void Die(DamageInfo info)
    {
        if (isDead) return;
        isDead = true;
        onDeath.Invoke(info);

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            // Placeholder: disable components; you can customize later
            var cc = GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
        }
    }
}