using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu controller.
/// Uses 3D Modern Menu UI package buttons (assign in Inspector).
/// Persists loadout via PlayerLoadout singleton.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Loadout Reference")]
    public PlayerLoadout loadout;

    [Header("Skill Display")]
    public TextMeshProUGUI jumpValueText;
    public TextMeshProUGUI teleportValueText;
    public TextMeshProUGUI skillPointsLeftText;

    [Header("Grenade Rows (optional - wire 6 rows)")]
    public TextMeshProUGUI[] grenadeCountTexts;   // one per GrenadeSlot
    public TextMeshProUGUI   grenadePointsLeftText;

    [Header("Weapon / Melee / Bot (assign prefabs)")]
    public GameObject[] weaponPrefabOptions;      // populate in Inspector
    public GameObject[] meleePrefabOptions;
    public GameObject[] botPrefabOptions;

    [Header("Navigation")]
    public Button playFfaButton;
    public string ffaSceneName = "FFA_Test";

    private void Start()
    {
        if (loadout == null)
            loadout = FindObjectOfType<PlayerLoadout>();

        // Default weapon/bot selection if nothing is chosen yet
        if (loadout.primaryWeaponPrefab == null && weaponPrefabOptions.Length > 0)
            loadout.primaryWeaponPrefab = weaponPrefabOptions[0];
        if (loadout.botPrefab == null && botPrefabOptions.Length > 0)
            loadout.botPrefab = botPrefabOptions[0];

        playFfaButton.onClick.AddListener(OnPlayFFA);
        RefreshAll();
    }

    // ── Skill buttons ───────────────────────────────────────────────
    public void OnIncreaseJump()     => TrySkills(loadout.jumpSkillPoints + 1, loadout.teleportSkillPoints);
    public void OnDecreaseJump()     => TrySkills(Mathf.Max(0, loadout.jumpSkillPoints - 1), loadout.teleportSkillPoints);
    public void OnIncreaseTeleport() => TrySkills(loadout.jumpSkillPoints, loadout.teleportSkillPoints + 1);
    public void OnDecreaseTeleport() => TrySkills(loadout.jumpSkillPoints, Mathf.Max(0, loadout.teleportSkillPoints - 1));

    private void TrySkills(int j, int t)
    {
        loadout.TrySetSkills(j, t);
        RefreshAll();
    }

    // ── Grenade buttons (call with slot index from UI) ─────────────────
    public void IncreaseGrenade(int slotIndex)
    {
        int pts = slotIndex < loadout.grenadeSlots.Count
                  ? loadout.grenadeSlots[slotIndex].points + 1 : 0;
        loadout.TrySetGrenadePoints(slotIndex, pts);
        RefreshAll();
    }

    public void DecreaseGrenade(int slotIndex)
    {
        if (slotIndex >= loadout.grenadeSlots.Count) return;
        int pts = Mathf.Max(0, loadout.grenadeSlots[slotIndex].points - 1);
        loadout.TrySetGrenadePoints(slotIndex, pts);
        RefreshAll();
    }

    // ── Weapon / Melee / Bot selectors ───────────────────────────────
    public void SelectWeapon(int index)
    {
        if (index >= 0 && index < weaponPrefabOptions.Length)
            loadout.primaryWeaponPrefab = weaponPrefabOptions[index];
    }

    public void SelectMelee(int index)
    {
        if (index >= 0 && index < meleePrefabOptions.Length)
            loadout.meleeWeaponPrefab = meleePrefabOptions[index];
    }

    public void SelectBot(int index)
    {
        if (index >= 0 && index < botPrefabOptions.Length)
            loadout.botPrefab = botPrefabOptions[index];
    }

    // ── Play ─────────────────────────────────────────────────────────
    private void OnPlayFFA()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(ffaSceneName);
    }

    // ── Refresh UI ─────────────────────────────────────────────────
    private void RefreshAll()
    {
        if (jumpValueText)      jumpValueText.text      = loadout.jumpSkillPoints.ToString();
        if (teleportValueText)  teleportValueText.text  = loadout.teleportSkillPoints.ToString();
        if (skillPointsLeftText)
            skillPointsLeftText.text = (PlayerLoadout.MaxSkillPoints - loadout.CurrentSkillPoints).ToString() + " left";

        for (int i = 0; i < grenadeCountTexts?.Length; i++)
        {
            if (grenadeCountTexts[i] == null || i >= loadout.grenadeSlots.Count) continue;
            grenadeCountTexts[i].text = loadout.grenadeSlots[i].points.ToString();
        }

        if (grenadePointsLeftText)
            grenadePointsLeftText.text = (PlayerLoadout.MaxGrenadePoints - loadout.CurrentGrenadePoints).ToString() + " left";
    }
}
