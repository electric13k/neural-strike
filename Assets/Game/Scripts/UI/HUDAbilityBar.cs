using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the ev.io-style ability bar above the healthbar:
///   Q = teleport | G U V E = grenade slots (max 4 equipped)
/// Shows key label, icon, count badge, and a cooldown dark overlay.
/// Wire up in Inspector: one AbilitySlot entry per key.
/// </summary>
public class HUDAbilityBar : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySlot
    {
        public string keyLabel;      // "Q", "G", "U", "V", "E"
        public Image  iconImage;
        public Image  cooldownOverlay; // fill=0 ready, fill=1 on cooldown
        public TMP_Text countBadge;  // shows grenade count
        [HideInInspector] public bool active;
    }

    [Header("Slots: Q, G, U, V, E")]
    public AbilitySlot teleportSlot;   // Q
    public AbilitySlot[] grenadeSlots; // G U V E

    [Header("References")]
    public TeleportAbility teleportAbility;

    // Call this after spawning to populate from loadout
    public void RefreshFromLoadout()
    {
        var loadout = PlayerLoadout.Instance;

        // Q slot
        if (teleportSlot != null)
        {
            bool hasTeleport = loadout != null && loadout.teleportSkillPoints > 0;
            teleportSlot.active = hasTeleport;
            SetSlotActive(teleportSlot, hasTeleport);
        }

        // G U V E grenade slots
        if (loadout == null || grenadeSlots == null) return;
        int slot = 0;
        foreach (var gs in loadout.grenades)
        {
            if (slot >= grenadeSlots.Length) break;
            bool equipped = gs.points > 0;
            grenadeSlots[slot].active = equipped;
            SetSlotActive(grenadeSlots[slot], equipped);
            if (grenadeSlots[slot].countBadge != null)
                grenadeSlots[slot].countBadge.text = equipped ? gs.points.ToString() : "0";
            slot++;
        }
    }

    private void SetSlotActive(AbilitySlot s, bool on)
    {
        if (s.iconImage    != null) s.iconImage.color    = on ? Color.white : new Color(1,1,1,0.25f);
        if (s.cooldownOverlay != null) s.cooldownOverlay.fillAmount = 0f;
        if (s.countBadge   != null) s.countBadge.gameObject.SetActive(on);
    }

    private void Update()
    {
        // Update teleport cooldown overlay
        if (teleportAbility != null && teleportSlot != null && teleportSlot.cooldownOverlay != null)
        {
            float ready = teleportAbility.CooldownNormalized; // 0=just used, 1=ready
            teleportSlot.cooldownOverlay.fillAmount = 1f - ready;
        }
    }
}
