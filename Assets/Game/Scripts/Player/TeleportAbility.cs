using UnityEngine;

/// <summary>
/// Q-key teleport ability (ev.io style).
/// Level 0 = disabled. Level 1/2/3 = longer range, shorter cooldown.
/// Attach to same GameObject as PlayerController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class TeleportAbility : MonoBehaviour
{
    [Range(0, 3)]
    [Tooltip("0=off, 1-3 set by loadout")]
    public int teleportSkillLevel = 0;

    public KeyCode teleportKey = KeyCode.Q;

    [Header("Tuning")]
    public float baseDistance    = 6f;
    public float distancePerLvl  = 2f;   // added per level above 1
    public float baseCooldown    = 5f;
    public float cooldownPerLvl  = -1f;  // shorter per level (min 1s)
    public LayerMask collisionMask = ~0;

    private CharacterController _cc;
    private float _lastTeleportTime = -999f;

    private float MaxDistance =>
        Mathf.Max(0f, baseDistance + distancePerLvl * Mathf.Max(0, teleportSkillLevel - 1));

    private float Cooldown =>
        Mathf.Max(1f, baseCooldown + cooldownPerLvl * Mathf.Max(0, teleportSkillLevel - 1));

    public float CooldownRemaining =>
        Mathf.Max(0f, (_lastTeleportTime + Cooldown) - Time.time);

    private void Awake() => _cc = GetComponent<CharacterController>();

    private void Update()
    {
        if (teleportSkillLevel <= 0) return;
        if (Input.GetKeyDown(teleportKey) && CooldownRemaining <= 0f)
            TryTeleport();
    }

    private void TryTeleport()
    {
        Vector3 origin = transform.position + Vector3.up * (_cc.height * 0.5f);
        Vector3 dir    = transform.forward;

        float dist = MaxDistance;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, MaxDistance,
                            collisionMask, QueryTriggerInteraction.Ignore))
            dist = Mathf.Max(0.5f, hit.distance - 0.5f);

        _cc.enabled            = false;
        transform.position    += dir * dist;
        _cc.enabled            = true;
        _lastTeleportTime      = Time.time;
    }
}
