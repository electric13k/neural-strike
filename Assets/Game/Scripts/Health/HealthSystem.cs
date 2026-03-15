using UnityEngine;
using UnityEngine.Events;

// ============================================================
//  HEALTH SYSTEM  — Neural Strike
//  Works for players, bots, and destructible objects.
//  Exposes UnityEvents so any UI / audio can subscribe without
//  tight coupling.
// ============================================================

public class HealthSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth  = 100f;
    public float maxArmour  =  50f;
    public bool  isInvincible;

    [Header("Regen (leave 0 to disable)")]
    public float regenPerSec      = 0f;
    public float regenDelaySec    = 5f;     // seconds after last damage before regen starts

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;   // (current, max)
    public UnityEvent<float>        onDamaged;         // damage amount
    public UnityEvent               onDeath;
    public UnityEvent               onRevive;

    // ── State ──────────────────────────────────────────────────
    private float _hp;
    private float _armour;
    private float _lastDamageTime = -999f;
    private bool  _dead;

    public float HP     => _hp;
    public float Armour => _armour;
    public bool  IsDead => _dead;

    void Awake()
    {
        _hp     = maxHealth;
        _armour = maxArmour;
    }

    void Update()
    {
        if (_dead || regenPerSec <= 0f) return;
        if (Time.time < _lastDamageTime + regenDelaySec) return;
        if (_hp >= maxHealth) return;

        Heal(regenPerSec * Time.deltaTime);
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>Apply damage. Armour absorbs 50 % of incoming damage.</summary>
    public void TakeDamage(float amount, GameObject source = null)
    {
        if (_dead || isInvincible || amount <= 0f) return;

        // armour absorbs half, then is drained
        float armourAbsorb = Mathf.Min(_armour, amount * 0.5f);
        _armour           -= armourAbsorb;
        float netDamage    = amount - armourAbsorb;

        _hp              = Mathf.Max(0f, _hp - netDamage);
        _lastDamageTime  = Time.time;

        onDamaged?.Invoke(amount);
        onHealthChanged?.Invoke(_hp, maxHealth);

        if (_hp <= 0f) Die(source);
    }

    /// <summary>Restore HP (clamped to maxHealth).</summary>
    public void Heal(float amount)
    {
        if (_dead || amount <= 0f) return;
        _hp = Mathf.Min(maxHealth, _hp + amount);
        onHealthChanged?.Invoke(_hp, maxHealth);
    }

    /// <summary>Add armour (clamped to maxArmour).</summary>
    public void AddArmour(float amount)
    {
        _armour = Mathf.Min(maxArmour, _armour + amount);
    }

    void Die(GameObject killer)
    {
        if (_dead) return;
        _dead = true;
        onDeath?.Invoke();
        Debug.Log($"{name} was killed by {killer?.name ?? "environment"}");
    }

    /// <summary>Revive at given HP (used by Medic bot / respawn).</summary>
    public void Revive(float hp = -1f)
    {
        _dead   = false;
        _hp     = hp > 0f ? Mathf.Min(hp, maxHealth) : maxHealth;
        _armour = maxArmour;
        onRevive?.Invoke();
        onHealthChanged?.Invoke(_hp, maxHealth);
    }
}
