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
    public Image  healthFill;
    public Health playerHealth;

    [Header("Ammo")]
    public TextMeshProUGUI       ammoText;
    public PlayerWeaponController weaponController;

    [Header("Match Info")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameManager     gameManager;

    [Header("Ability Bar (above healthbar)")]
    public HUDAbilityBar abilityBar;

    private void Start()
    {
        if (abilityBar != null)
            abilityBar.RefreshFromLoadout();
    }

    private void Update()
    {
        UpdateHealth();
        UpdateAmmo();
        UpdateMatchInfo();
    }

    private void UpdateHealth()
    {
        if (healthFill == null || playerHealth == null) return;
        healthFill.fillAmount = playerHealth.HealthPercent;
        healthFill.color = Color.Lerp(Color.red, Color.green, playerHealth.HealthPercent);
    }

    private void UpdateAmmo()
    {
        if (ammoText == null || weaponController == null) return;
        Weapon w = weaponController.currentWeapon;
        ammoText.text  = w != null ? $"{w.CurrentAmmo} / {w.ReserveAmmo}" : "-- / --";
        ammoText.color = (w != null && w.CurrentAmmo == 0) ? Color.red : Color.white;
    }

    private void UpdateMatchInfo()
    {
        if (gameManager == null) return;

        if (timerText != null)
        {
            float t = gameManager.TimeRemaining;
            timerText.text = $"{Mathf.FloorToInt(t / 60):00}:{Mathf.FloorToInt(t % 60):00}";
        }

        if (scoreText != null)
            scoreText.text = $"Score: {gameManager.GetScore("Player")}";
    }
}
