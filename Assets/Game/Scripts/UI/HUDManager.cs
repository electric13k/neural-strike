using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires HUD elements to live game state every frame.
/// Attach to the HUD Canvas.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    public Image    healthFill;
    public Health   playerHealth;

    [Header("Ammo")]
    public TextMeshProUGUI      ammoText;
    public PlayerWeaponController weaponController;

    void Update()
    {
        UpdateHealth();
        UpdateAmmo();
    }

    void UpdateHealth()
    {
        if (healthFill == null || playerHealth == null) return;
        healthFill.fillAmount = playerHealth.HealthPercent;

        // colour shift: green -> yellow -> red
        float hp = playerHealth.HealthPercent;
        healthFill.color = Color.Lerp(Color.red, Color.green, hp);
    }

    void UpdateAmmo()
    {
        if (ammoText == null || weaponController == null) return;
        Weapon w = weaponController.currentWeapon;
        if (w == null) { ammoText.text = "-- / --"; return; }
        ammoText.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
        ammoText.color = w.CurrentAmmo == 0 ? Color.red : Color.white;
    }
}
