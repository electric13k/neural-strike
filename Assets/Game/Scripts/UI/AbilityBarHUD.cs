using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Extends HUDManager to draw the ev.io-style overlay:
/// [Q] teleport  |  [G][U][V][E] grenade slots  over the healthbar.
/// Attach to the HUD Canvas alongside HUDManager.
/// </summary>
public class AbilityBarHUD : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySlot
    {
        public Image             icon;
        public TextMeshProUGUI   countText;
        public TextMeshProUGUI   hotkeyLabel;
        public Image             cooldownOverlay;  // optional dark fill
    }

    [Header("Teleport Slot (Q)")]
    public AbilitySlot teleportSlot;
    public TeleportAbility teleportAbility;

    [Header("Grenade Slots (G U V E)")]
    public AbilitySlot[]      grenadeSlots = new AbilitySlot[4];
    public GrenadeController  grenadeController;

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

        bool enabled  = teleportAbility != null && teleportAbility.teleportSkillLevel > 0;
        float coolRem = teleportAbility != null ? teleportAbility.CooldownRemaining : 0f;
        float coolMax = teleportAbility != null
            ? (teleportAbility.baseCooldown + teleportAbility.cooldownPerLvl *
               Mathf.Max(0, teleportAbility.teleportSkillLevel - 1))
            : 1f;

        if (teleportSlot.hotkeyLabel) teleportSlot.hotkeyLabel.text = "Q";

        if (!enabled)
        {
            SetSlotColor(teleportSlot, emptyColor);
            if (teleportSlot.countText) teleportSlot.countText.text = "";
            return;
        }

        if (coolRem > 0f)
        {
            SetSlotColor(teleportSlot, cooldownColor);
            if (teleportSlot.cooldownOverlay)
                teleportSlot.cooldownOverlay.fillAmount = coolRem / Mathf.Max(0.01f, coolMax);
            if (teleportSlot.countText)
                teleportSlot.countText.text = Mathf.CeilToInt(coolRem).ToString();
        }
        else
        {
            SetSlotColor(teleportSlot, readyColor);
            if (teleportSlot.cooldownOverlay) teleportSlot.cooldownOverlay.fillAmount = 0f;
            if (teleportSlot.countText) teleportSlot.countText.text = "";
        }
    }

    private void UpdateGrenades()
    {
        for (int i = 0; i < grenadeSlots.Length; i++)
        {
            var slot = grenadeSlots[i];
            if (slot == null) continue;

            if (slot.hotkeyLabel) slot.hotkeyLabel.text = GrenadeKeys[i];

            bool exists   = grenadeController != null && i < grenadeController.GetSlotCount();
            int remaining = exists ? grenadeController.GetRemaining(i) : 0;

            if (!exists || remaining <= 0)
            {
                SetSlotColor(slot, emptyColor);
                if (slot.countText) slot.countText.text = "0";
            }
            else
            {
                SetSlotColor(slot, readyColor);
                if (slot.countText) slot.countText.text = remaining.ToString();
            }
        }
    }

    private void SetSlotColor(AbilitySlot slot, Color c)
    {
        if (slot.icon)  slot.icon.color = c;
    }
}
