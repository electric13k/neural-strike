using UnityEngine;

// ============================================================
//  TELEPORT ABILITY  — Neural Strike
//  Key Q  |  Skill 0 = disabled, 1-3 = increasing range & speed.
//  Casts a ray forward, snaps player just before any obstacle.
//  CooldownNormalized (0..1) drives the HUD cooldown ring.
// ============================================================

[RequireComponent(typeof(PlayerController))]
public class TeleportAbility : MonoBehaviour
{
    [Header("Skill level  (0=off  1-3=active)")]
    [Range(0,3)] public int skillLevel = 0;

    [Header("Tuning")]
    public KeyCode   key              = KeyCode.Q;
    public float     baseDistance     = 6f;
    public float     distancePerLevel = 2f;
    public float     baseCooldown     = 5f;
    public float     cooldownPerLevel = -1f;    // negative = faster at higher level
    public LayerMask blockMask        = ~0;

    private PlayerController _pc;
    private float            _lastUse = -999f;

    // Properties read by HUD
    public float Cooldown          => Mathf.Max(0.5f, baseCooldown + cooldownPerLevel * Mathf.Max(0, skillLevel - 1));
    public float CooldownNormalized => Mathf.Clamp01((Time.time - _lastUse) / Cooldown);
    public bool  IsReady           => skillLevel > 0 && Time.time >= _lastUse + Cooldown;

    void Awake() => _pc = GetComponent<PlayerController>();

    void Update()
    {
        if (skillLevel <= 0) return;
        if (Input.GetKeyDown(key) && IsReady)
            DoTeleport();
    }

    void DoTeleport()
    {
        float maxDist = baseDistance + distancePerLevel * Mathf.Max(0, skillLevel - 1);
        Vector3 origin = transform.position + Vector3.up * 1f;  // chest-height ray
        Vector3 dir    = transform.forward;

        float travel = maxDist;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, blockMask, QueryTriggerInteraction.Ignore))
            travel = Mathf.Max(0.4f, hit.distance - 0.5f);

        Vector3 dest = transform.position + dir * travel;
        _pc.Warp(dest);
        _lastUse = Time.time;
    }
}
