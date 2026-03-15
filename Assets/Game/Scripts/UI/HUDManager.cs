using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  HUD MANAGER  — Neural Strike
//
//  HOW TO WIRE IN UNITY
//  1. Create a Screen-Space Overlay Canvas in the Game scene.
//  2. Under the Canvas add:
//       - healthSlider    (Slider) for HP bar
//       - armourSlider    (Slider) for Armour bar
//       - ammoText        (Text)   e.g. "24 / 120"
//       - grenadeText     (Text)   current grenade type & count
//       - teleportFill    (Image, Image Type = Filled) cooldown ring
//       - crosshair       (Image)  small dot or cross
//       - timerText       (Text)   match countdown
//  3. Create empty "HUDManager" object, attach this script.
//  4. Drag the player prefab (or FindPlayer at runtime) into
//     playerHealth, playerWeapon, grenadeCtrl, teleport.
// ============================================================

public class HUDManager : MonoBehaviour
{
    [Header("Player References (auto-found if null)")]
    public HealthSystem      playerHealth;
    public WeaponBase        playerWeapon;   // active weapon
    public GrenadeController grenadeCtrl;
    public TeleportAbility   teleport;

    [Header("UI Elements")]
    public Slider healthSlider;
    public Slider armourSlider;
    public Text   ammoText;
    public Text   grenadeText;
    public Image  teleportFill;     // Image.Type = Filled, FillMethod = Radial360
    public Text   timerText;

    // ── Lifecycle ─────────────────────────────────────────────

    void Start()
    {
        TryAutoFind();
        if (healthSlider)  healthSlider.maxValue  = playerHealth  ? playerHealth.maxHealth  : 100f;
        if (armourSlider)  armourSlider.maxValue   = playerHealth  ? playerHealth.maxArmour   :  50f;
    }

    void Update()
    {
        RefreshHealth();
        RefreshAmmo();
        RefreshGrenade();
        RefreshTeleport();
    }

    // ── Auto-find from tagged Player ──────────────────────────

    void TryAutoFind()
    {
        if (playerHealth == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) return;
            playerHealth = p.GetComponent<HealthSystem>();
            playerWeapon  = p.GetComponentInChildren<WeaponBase>();
            grenadeCtrl  = p.GetComponent<GrenadeController>();
            teleport     = p.GetComponent<TeleportAbility>();
        }
    }

    // ── Refresh methods ───────────────────────────────────────

    void RefreshHealth()
    {
        if (playerHealth == null) return;
        if (healthSlider) healthSlider.value = playerHealth.HP;
        if (armourSlider) armourSlider.value  = playerHealth.Armour;
    }

    void RefreshAmmo()
    {
        if (ammoText == null || playerWeapon == null) return;
        ammoText.text = playerWeapon.IsReloading
            ? "RELOADING..."
            : $"{playerWeapon.Ammo} / {playerWeapon.ReserveAmmo}";
    }

    void RefreshGrenade()
    {
        if (grenadeText == null || grenadeCtrl == null) return;
        grenadeText.text = $"FRAG:{grenadeCtrl.fragCount}  STICKY:{grenadeCtrl.stickyCount}  VORTEX:{grenadeCtrl.vortexCount}";
    }

    void RefreshTeleport()
    {
        if (teleportFill == null || teleport == null) return;
        teleportFill.fillAmount = teleport.CooldownNormalized;
    }

    public void SetTimerText(string txt)
    {
        if (timerText) timerText.text = txt;
    }
}
