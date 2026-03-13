using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Main Menu loadout screen.
/// Skills: 3 pts total (jump 0-3, teleport 0-3).
/// Grenades: 4 pts across up to 4 types.
/// Max 1 weapon + 1 melee.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Loadout Reference")]
    public PlayerLoadout loadout;

    [Header("Skill UI")]
    public TMP_Text jumpLevelText;
    public TMP_Text teleportLevelText;
    public TMP_Text skillPointsLeftText;

    [Header("Grenade Slot UI (4 slots)")]
    public TMP_Text[] grenadeCountTexts;
    public TMP_Text   grenadePointsLeftText;

    [Header("Weapon Choices")]
    public List<GameObject> availableWeapons;
    public TMP_Text weaponNameText;
    private int weaponIndex;

    [Header("Melee Choices")]
    public List<GameObject> availableMelees;
    public TMP_Text meleeNameText;
    private int meleeIndex;

    [Header("Bot Choices")]
    public List<GameObject> availableBots;
    public TMP_Text botNameText;
    private int botIndex;

    [Header("Scene")]
    public string ffaSceneName = "FFA_Test";
    public Button playButton;

    private void Start()
    {
        if (loadout == null)
#if UNITY_2023_1_OR_NEWER
            loadout = Object.FindFirstObjectByType<PlayerLoadout>();
#else
            loadout = FindObjectOfType<PlayerLoadout>();
#endif

        if (loadout == null)
        {
            var go = new GameObject("LoadoutManager");
            loadout = go.AddComponent<PlayerLoadout>();
        }

        RefreshAll();
        playButton?.onClick.AddListener(OnPlay);
    }

    public void OnIncreaseJump()     => TrySkill(loadout.jumpSkillPoints + 1, loadout.teleportSkillPoints);
    public void OnDecreaseJump()     => TrySkill(loadout.jumpSkillPoints - 1, loadout.teleportSkillPoints);
    public void OnIncreaseTeleport() => TrySkill(loadout.jumpSkillPoints,     loadout.teleportSkillPoints + 1);
    public void OnDecreaseTeleport() => TrySkill(loadout.jumpSkillPoints,     loadout.teleportSkillPoints - 1);

    private void TrySkill(int j, int t) { loadout.TrySetSkills(j, t); RefreshSkillUI(); }

    private void RefreshSkillUI()
    {
        if (jumpLevelText)      jumpLevelText.text      = loadout.jumpSkillPoints.ToString();
        if (teleportLevelText)  teleportLevelText.text  = loadout.teleportSkillPoints.ToString();
        if (skillPointsLeftText) skillPointsLeftText.text = $"{loadout.MaxSkillPoints - loadout.CurrentSkillPoints} pts left";
    }

    public void OnIncreaseGrenade(int i) { loadout.TrySetGrenadePoints(i, loadout.grenades[i].points + 1); RefreshGrenadeUI(); }
    public void OnDecreaseGrenade(int i) { loadout.TrySetGrenadePoints(i, loadout.grenades[i].points - 1); RefreshGrenadeUI(); }

    private void RefreshGrenadeUI()
    {
        for (int i = 0; i < grenadeCountTexts.Length && i < loadout.grenades.Count; i++)
            if (grenadeCountTexts[i]) grenadeCountTexts[i].text = loadout.grenades[i].points.ToString();
        if (grenadePointsLeftText)
            grenadePointsLeftText.text = $"{loadout.MaxGrenadePoints - loadout.CurrentGrenadePoints} pts left";
    }

    public void OnNextWeapon() { if (availableWeapons.Count == 0) return; weaponIndex = (weaponIndex + 1) % availableWeapons.Count; loadout.primaryWeaponPrefab = availableWeapons[weaponIndex]; RefreshWeaponUI(); }
    public void OnPrevWeapon() { if (availableWeapons.Count == 0) return; weaponIndex = (weaponIndex - 1 + availableWeapons.Count) % availableWeapons.Count; loadout.primaryWeaponPrefab = availableWeapons[weaponIndex]; RefreshWeaponUI(); }
    private void RefreshWeaponUI() { if (weaponNameText && availableWeapons.Count > 0) weaponNameText.text = availableWeapons[weaponIndex].name; }

    public void OnNextMelee() { if (availableMelees.Count == 0) return; meleeIndex = (meleeIndex + 1) % availableMelees.Count; loadout.meleePrefab = availableMelees[meleeIndex]; RefreshMeleeUI(); }
    public void OnPrevMelee() { if (availableMelees.Count == 0) return; meleeIndex = (meleeIndex - 1 + availableMelees.Count) % availableMelees.Count; loadout.meleePrefab = availableMelees[meleeIndex]; RefreshMeleeUI(); }
    private void RefreshMeleeUI() { if (meleeNameText && availableMelees.Count > 0) meleeNameText.text = availableMelees[meleeIndex].name; }

    public void OnNextBot() { if (availableBots.Count == 0) return; botIndex = (botIndex + 1) % availableBots.Count; loadout.botPrefab = availableBots[botIndex]; RefreshBotUI(); }
    public void OnPrevBot() { if (availableBots.Count == 0) return; botIndex = (botIndex - 1 + availableBots.Count) % availableBots.Count; loadout.botPrefab = availableBots[botIndex]; RefreshBotUI(); }
    private void RefreshBotUI() { if (botNameText && availableBots.Count > 0) botNameText.text = availableBots[botIndex].name; }

    private void RefreshAll() { RefreshSkillUI(); RefreshGrenadeUI(); RefreshWeaponUI(); RefreshMeleeUI(); RefreshBotUI(); }

    private void OnPlay() => SceneManager.LoadScene(ffaSceneName);
}
