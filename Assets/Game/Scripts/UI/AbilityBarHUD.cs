using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  ABILITY BAR HUD  — Neural Strike
//  ev.io-style overlay: [Q] teleport | [G][U][V][E] grenades.
//  Updated: uses GrenadeThrowController (renamed from GrenadeController).
// ============================================================

public class AbilityBarHUD : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySlot
    {
        public Image           icon;
        public TextMeshProUGUI countText;
        public TextMeshProUGUI hotkeyLabel;
        public Image           cooldownOverlay;   // radial / filled Image
    }

    [Header("Teleport Slot  (Q)")]
    public AbilitySlot     teleportSlot;
    public TeleportAbility teleportAbility;

    [Header("Grenade Slots  (G U V E)")]
    public AbilitySlot[]          grenadeSlots = new AbilitySlot[4];
    public GrenadeThrowController grenadeController;   // ← renamed class

    [Header("Colors")]
    public Color readyColor    = Color.white;
    public Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color emptyColor    = new Color(0.2f, 0.2f, 0.2f, 0.6f);

    private static readonly string[] GrenadeKeys = { "G", "U", "V", "E" };

    // Slot order matches GrenadeThrowController: 0=frag 1=sticky 2=vortex 3=(unused)
    void Update()
    {
        // Auto-find if not wired in Inspector
        if (teleportAbility == null || grenadeController == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                if (teleportAbility  == null) teleportAbility  = p.GetComponent<TeleportAbility>();
                if (grenadeController == null) grenadeController = p.GetComponent<GrenadeThrowController>();
            }
        }

        UpdateTeleport();
        UpdateGrenades();
    }

    void UpdateTeleport()
    {
        if (teleportSlot == null) return;
        if (teleportSlot.hotkeyLabel) teleportSlot.hotkeyLabel.text = "Q";

        bool hasAbility = teleportAbility != null && teleportAbility.skillLevel > 0;
        if (!hasAbility)
        {
            SetSlotColor(teleportSlot, emptyColor);
            if (teleportSlot.countText) teleportSlot.countText.text = "";
            if (teleportSlot.cooldownOverlay) teleportSlot.cooldownOverlay.fillAmount = 0f;
            return;
        }

        float readyFrac  = teleportAbility.CooldownNormalized;  // 0=on cooldown, 1=ready
        bool  onCooldown = readyFrac < 1f;

        SetSlotColor(teleportSlot, onCooldown ? cooldownColor : readyColor);

        if (teleportSlot.cooldownOverlay)
            teleportSlot.cooldownOverlay.fillAmount = onCooldown ? (1f - readyFrac) : 0f;

        if (teleportSlot.countText)
            teleportSlot.countText.text = onCooldown
                ? Mathf.CeilToInt((1f - readyFrac) * teleportAbility.Cooldown).ToString()
                : "";
    }

    void UpdateGrenades()
    {
        for (int i = 0; i < grenadeSlots.Length; i++)
        {
            var slot = grenadeSlots[i];
            if (slot == null) continue;

            if (slot.hotkeyLabel) slot.hotkeyLabel.text = GrenadeKeys[i];

            int rem = (grenadeController != null) ? GetRemaining(i) : 0;

            SetSlotColor(slot, rem <= 0 ? emptyColor : readyColor);
            if (slot.countText) slot.countText.text = rem > 0 ? rem.ToString() : "-";
            if (slot.cooldownOverlay) slot.cooldownOverlay.fillAmount = 0f;
        }
    }

    // Maps slot index to the correct count on GrenadeThrowController
    int GetRemaining(int slot)
    {
        if (grenadeController == null) return 0;
        switch (slot)
        {
            case 0: return grenadeController.fragCount;
            case 1: return grenadeController.stickyCount;
            case 2: return grenadeController.vortexCount;
            default: return 0;
        }
    }

    void SetSlotColor(AbilitySlot slot, Color c)
    {
        if (slot.icon) slot.icon.color = c;
    }
}
