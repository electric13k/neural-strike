using UnityEngine;

/// <summary>
/// Interface for entities that can receive damage.
/// </summary>
public interface IDamageable
{
    void ApplyDamage(float amount, DamageInfo info);
    bool IsDead { get; }
}

/// <summary>
/// Helper component that forwards damage to a Health component.
/// Attach to colliders that should receive damage (e.g. hitboxes).
/// </summary>
public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private Health healthComponent;
    [SerializeField] private float damageMultiplier = 1f;

    public bool IsDead => healthComponent != null && healthComponent.IsDead;

    void Awake()
    {
        if (healthComponent == null)
            healthComponent = GetComponentInParent<Health>();
    }

    public void ApplyDamage(float amount, DamageInfo info)
    {
        if (healthComponent != null)
            healthComponent.ApplyDamage(amount * damageMultiplier, info);
    }

    public void SetMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }
}
