using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ev.io-style ability overlay: [Q] teleport | [G][U][V][E] grenades.
/// Attach to HUD Canvas alongside HUDManager.
/// </summary>
public class AbilityBarHUD : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySlot
    {
        public Image           icon;
        public TextMeshProUGUI countText;
        public TextMeshProUGUI hotkeyLabel;
        public Image           cooldownOverlay;  // radial/filled Image
    }

    [Header("Teleport Slot (Q)")]
    public AbilitySlot    teleportSlot;
    public TeleportAbility teleportAbility;

    [Header("Grenade Slots (G U V E)")]
    public AbilitySlot[]     grenadeSlots = new AbilitySlot[4];
    public GrenadeController grenadeController;

    [Header("Colors")]
    public Color readyColor    = Color.white;
    public Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color emptyColor    = new Color(0.2f, 0.2f, 0.2f, 0.6f);

    private static readonly string[] GrenadeKeys = { "G", "U", "V", "E" };

    private void Update()
    {
        UpdateTeleport();
        UpdateGrenades();
    }

    private void UpdateTeleport()
    {
        if (teleportSlot == null) return;

        bool hasAbility = teleportAbility != null && teleportAbility.teleportSkillLevel > 0;
        if (teleportSlot.hotkeyLabel) teleportSlot.hotkeyLabel.text = "Q";

        if (!hasAbility)
        {
            SetColor(teleportSlot, emptyColor);
            if (teleportSlot.countText) teleportSlot.countText.text = "";
            return;
        }

        // CooldownNormalized: 0 = just used (on cooldown), 1 = ready
        float readyFrac = teleportAbility.CooldownNormalized;
        bool  onCooldown = readyFrac < 1f;

        SetColor(teleportSlot, onCooldown ? cooldownColor : readyColor);

        if (teleportSlot.cooldownOverlay)
            teleportSlot.cooldownOverlay.fillAmount = onCooldown ? (1f - readyFrac) : 0f;

        if (teleportSlot.countText)
            teleportSlot.countText.text = onCooldown
                ? Mathf.CeilToInt((1f - readyFrac) * teleportAbility.baseCooldown).ToString()
                : "";
    }

    private void UpdateGrenades()
    {
        for (int i = 0; i < grenadeSlots.Length; i++)
        {
            var slot = grenadeSlots[i];
            if (slot == null) continue;

            if (slot.hotkeyLabel) slot.hotkeyLabel.text = GrenadeKeys[i];

            bool hasSlot  = grenadeController != null && i < grenadeController.GetSlotCount();
            int  rem      = hasSlot ? grenadeController.GetRemaining(i) : 0;

            SetColor(slot, (!hasSlot || rem <= 0) ? emptyColor : readyColor);
            if (slot.countText) slot.countText.text = rem.ToString();
            if (slot.cooldownOverlay) slot.cooldownOverlay.fillAmount = 0f;
        }
    }

    private void SetColor(AbilitySlot slot, Color c)
    {
        if (slot.icon) slot.icon.color = c;
    }
}
