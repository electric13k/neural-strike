using UnityEngine;

// ============================================================
//  DAMAGEABLE  — Neural Strike
//  Thin adapter: any collider can have this component and it
//  will forward damage to the HealthSystem on the root.
//  Useful for headshot multipliers (place on head bone collider).
// ============================================================

public class Damageable : MonoBehaviour
{
    [Tooltip("Damage multiplier for this specific hit zone.")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;

    private HealthSystem _hs;

    void Awake()
    {
        _hs = GetComponentInParent<HealthSystem>();
        if (_hs == null)
            Debug.LogWarning($"[Damageable] No HealthSystem found above {name}");
    }

    public void TakeDamage(float rawDamage, GameObject source = null)
    {
        _hs?.TakeDamage(rawDamage * damageMultiplier, source);
    }
}
