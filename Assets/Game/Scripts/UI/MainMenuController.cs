using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Main Menu loadout screen:
///  - Choose weapon / melee / bot (drag prefabs to arrays in Inspector)
///  - Skill points: 3 total for jump(0-3) and teleport(0-3)
///  - Grenade points: 4 total across up to 4 types
///  - Play button loads FFA_Test scene
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Loadout Reference")]
    public PlayerLoadout loadout;

    // ── SKILLS ──────────────────────────────────────────────────────
    [Header("Skill UI")]
    public TMP_Text jumpLevelText;
    public TMP_Text teleportLevelText;
    public TMP_Text skillPointsLeftText;

    // ── GRENADE SLOTS ────────────────────────────────────────────────
    [Header("Grenade Slot UI (4 slots)")]
    public TMP_Text[] grenadeCountTexts;  // one per grenade slot
    public TMP_Text   grenadePointsLeftText;

    // ── WEAPON / MELEE / BOT SELECTION ──────────────────────────────
    [Header("Weapon Choices")]
    public List<GameObject> availableWeapons;
    public TMP_Text weaponNameText;
    private int weaponIndex = 0;

    [Header("Melee Choices")]
    public List<GameObject> availableMelees;
    public TMP_Text meleeNameText;
    private int meleeIndex = 0;

    [Header("Bot Choices")]
    public List<GameObject> availableBots;
    public TMP_Text botNameText;
    private int botIndex = 0;

    // ── SCENE ────────────────────────────────────────────────────────
    [Header("Scene")]
    public string ffaSceneName = "FFA_Test";
    public Button playButton;

    private void Start()
    {
        if (loadout == null)
            loadout = FindObjectOfType<PlayerLoadout>();

        if (loadout == null)
        {
            var go = new GameObject("LoadoutManager");
            loadout = go.AddComponent<PlayerLoadout>();
        }

        RefreshAll();
        playButton?.onClick.AddListener(OnPlay);
    }

    // ── SKILLS ──
    public void OnIncreaseJump()     => TrySkill(loadout.jumpSkillPoints + 1, loadout.teleportSkillPoints);
    public void OnDecreaseJump()     => TrySkill(loadout.jumpSkillPoints - 1, loadout.teleportSkillPoints);
    public void OnIncreaseTeleport() => TrySkill(loadout.jumpSkillPoints,     loadout.teleportSkillPoints + 1);
    public void OnDecreaseTeleport() => TrySkill(loadout.jumpSkillPoints,     loadout.teleportSkillPoints - 1);

    private void TrySkill(int j, int t)
    {
        loadout.TrySetSkills(j, t);
        RefreshSkillUI();
    }

    private void RefreshSkillUI()
    {
        if (jumpLevelText)     jumpLevelText.text     = loadout.jumpSkillPoints.ToString();
        if (teleportLevelText) teleportLevelText.text = loadout.teleportSkillPoints.ToString();
        int left = loadout.MaxSkillPoints - loadout.CurrentSkillPoints;
        if (skillPointsLeftText) skillPointsLeftText.text = $"{left} pts left";
    }

    // ── GRENADES ──
    public void OnIncreaseGrenade(int slot) { loadout.TrySetGrenadePoints(slot, loadout.grenades[slot].points + 1); RefreshGrenadeUI(); }
    public void OnDecreaseGrenade(int slot) { loadout.TrySetGrenadePoints(slot, loadout.grenades[slot].points - 1); RefreshGrenadeUI(); }

    private void RefreshGrenadeUI()
    {
        for (int i = 0; i < grenadeCountTexts.Length && i < loadout.grenades.Count; i++)
        {
            if (grenadeCountTexts[i] != null)
                grenadeCountTexts[i].text = loadout.grenades[i].points.ToString();
        }
        int left = loadout.MaxGrenadePoints - loadout.CurrentGrenadePoints;
        if (grenadePointsLeftText) grenadePointsLeftText.text = $"{left} pts left";
    }

    // ── WEAPON ──
    public void OnNextWeapon() { if (availableWeapons.Count == 0) return; weaponIndex = (weaponIndex + 1) % availableWeapons.Count; loadout.primaryWeaponPrefab = availableWeapons[weaponIndex]; RefreshWeaponUI(); }
    public void OnPrevWeapon() { if (availableWeapons.Count == 0) return; weaponIndex = (weaponIndex - 1 + availableWeapons.Count) % availableWeapons.Count; loadout.primaryWeaponPrefab = availableWeapons[weaponIndex]; RefreshWeaponUI(); }
    private void RefreshWeaponUI() { if (weaponNameText && availableWeapons.Count > 0) weaponNameText.text = availableWeapons[weaponIndex].name; }

    // ── MELEE ──
    public void OnNextMelee() { if (availableMelees.Count == 0) return; meleeIndex = (meleeIndex + 1) % availableMelees.Count; loadout.meleePrefab = availableMelees[meleeIndex]; RefreshMeleeUI(); }
    public void OnPrevMelee() { if (availableMelees.Count == 0) return; meleeIndex = (meleeIndex - 1 + availableMelees.Count) % availableMelees.Count; loadout.meleePrefab = availableMelees[meleeIndex]; RefreshMeleeUI(); }
    private void RefreshMeleeUI() { if (meleeNameText && availableMelees.Count > 0) meleeNameText.text = availableMelees[meleeIndex].name; }

    // ── BOT ──
    public void OnNextBot() { if (availableBots.Count == 0) return; botIndex = (botIndex + 1) % availableBots.Count; loadout.botPrefab = availableBots[botIndex]; RefreshBotUI(); }
    public void OnPrevBot() { if (availableBots.Count == 0) return; botIndex = (botIndex - 1 + availableBots.Count) % availableBots.Count; loadout.botPrefab = availableBots[botIndex]; RefreshBotUI(); }
    private void RefreshBotUI() { if (botNameText && availableBots.Count > 0) botNameText.text = availableBots[botIndex].name; }

    private void RefreshAll()
    {
        RefreshSkillUI();
        RefreshGrenadeUI();
        RefreshWeaponUI();
        RefreshMeleeUI();
        RefreshBotUI();
    }

    private void OnPlay()
    {
        SceneManager.LoadScene(ffaSceneName);
    }
}
