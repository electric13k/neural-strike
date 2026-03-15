using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  HUD MANAGER  — Neural Strike
//  Updated: references GrenadeThrowController (renamed from
//  GrenadeController to fix CS0101 duplicate class error).
// ============================================================

public class HUDManager : MonoBehaviour
{
    [Header("Player References  (auto-found if null)")]
    public HealthSystem           playerHealth;
    public WeaponBase             playerWeapon;
    public GrenadeThrowController grenadeCtrl;   // renamed class
    public TeleportAbility        teleport;

    [Header("UI Elements")]
    public Slider healthSlider;
    public Slider armourSlider;
    public Text   ammoText;
    public Text   grenadeText;
    public Image  teleportFill;      // Image.Type=Filled, FillMethod=Radial360
    public Text   timerText;

    void Start()
    {
        TryAutoFind();
        if (healthSlider) healthSlider.maxValue = playerHealth ? playerHealth.maxHealth : 100f;
        if (armourSlider) armourSlider.maxValue  = playerHealth ? playerHealth.maxArmour  :  50f;
    }

    void Update()
    {
        // Re-try every frame until player spawns
        if (playerHealth == null) TryAutoFind();

        RefreshHealth();
        RefreshAmmo();
        RefreshGrenade();
        RefreshTeleport();
    }

    void TryAutoFind()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) return;
        if (playerHealth == null) playerHealth = p.GetComponent<HealthSystem>();
        if (playerWeapon  == null) playerWeapon  = p.GetComponentInChildren<WeaponBase>();
        if (grenadeCtrl  == null) grenadeCtrl   = p.GetComponent<GrenadeThrowController>();
        if (teleport     == null) teleport       = p.GetComponent<TeleportAbility>();
    }

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
        grenadeText.text = $"{grenadeCtrl.SlotName}  x{grenadeCtrl.CurrentCount}  [T=cycle  G=throw]";
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
