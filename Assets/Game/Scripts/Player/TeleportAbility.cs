using UnityEngine;

/// <summary>
/// Q-key teleport ability. Skill level 0=off, 1/2/3 = increasing distance & shorter cooldown.
/// Inspired by ev.io's teleport mechanic: player snaps forward along look direction,
/// stopping just before geometry.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class TeleportAbility : MonoBehaviour
{
    [Range(0, 3)]
    [Tooltip("0=disabled, 1-3=stronger teleport")]
    public int teleportSkillLevel = 0;

    public KeyCode teleportKey = KeyCode.Q;

    [Header("Tuning")]
    public float baseDistance     = 6f;
    public float distancePerLevel = 2f;   // extra metres per level above 1
    public float baseCooldown     = 5f;
    public float cooldownPerLevel = -1f;  // negative = faster at higher level

    public LayerMask collisionMask = ~0;

    // Cooldown visual hook – HUDAbilityBar reads this.
    public float CooldownNormalized =>
        Mathf.Clamp01((Time.time - lastTeleportTime) / Cooldown);

    private CharacterController controller;
    private float lastTeleportTime = -999f;

    private float MaxDistance =>
        baseDistance + distancePerLevel * Mathf.Max(0, teleportSkillLevel - 1);

    private float Cooldown =>
        Mathf.Max(0.5f, baseCooldown + cooldownPerLevel * Mathf.Max(0, teleportSkillLevel - 1));

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (teleportSkillLevel <= 0) return;
        if (Input.GetKeyDown(teleportKey) && Time.time >= lastTeleportTime + Cooldown)
            TryTeleport();
    }

    private void TryTeleport()
    {
        Vector3 origin = transform.position + Vector3.up * (controller.height * 0.5f);
        Vector3 dir    = transform.forward;

        float dist = MaxDistance;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, MaxDistance, collisionMask,
                QueryTriggerInteraction.Ignore))
            dist = Mathf.Max(0.5f, hit.distance - 0.5f);

        controller.enabled = false;
        transform.position += dir * dist;
        controller.enabled = true;

        lastTeleportTime = Time.time;
    }
}
