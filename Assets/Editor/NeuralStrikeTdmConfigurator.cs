#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// One-click wiring for Team Deathmatch.
/// Run AFTER Tools > Neural Strike > Build Scene so spawn points already exist.
/// </summary>
public static class NeuralStrikeTdmConfigurator
{
    [MenuItem("Tools/Neural Strike/Configure TDM Gamemode")]
    public static void ConfigureTdm()
    {
        // ── GAME MANAGER ───────────────────────────────────────────────
        GameObject gmObj = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        GameManager gm = gmObj.GetComponent<GameManager>() ?? gmObj.AddComponent<GameManager>();
        gm.matchDuration = 600f;  // 10 minutes
        gm.scoreToWin    = 25;    // first to 25 kills wins

        // ── SPAWN MANAGER ──────────────────────────────────────────────
        GameObject smObj = GameObject.Find("SpawnManager") ?? new GameObject("SpawnManager");
        SpawnManager sm = smObj.GetComponent<SpawnManager>() ?? smObj.AddComponent<SpawnManager>();

        sm.playerSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPlayer")
            .Select(go => go.transform).ToArray();
        sm.botSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnBot")
            .Select(go => go.transform).ToArray();
        sm.botsToSpawn = Mathf.Clamp(sm.botSpawnPoints.Length, 0, 12);
        sm.spawnDelay  = 5f;

        // ── TEAM DEATHMATCH MODE ───────────────────────────────────────
        GameObject modeObj = GameObject.Find("TeamDeathmatchMode") ?? new GameObject("TeamDeathmatchMode");
        TeamDeathmatchMode tdm = modeObj.GetComponent<TeamDeathmatchMode>()
                                 ?? modeObj.AddComponent<TeamDeathmatchMode>();
        tdm.gameManager = gm;

        // ── MATCH MANAGER ──────────────────────────────────────────────
        GameObject mmObj = GameObject.Find("MatchManager") ?? new GameObject("MatchManager");
        MatchManager mm = mmObj.GetComponent<MatchManager>() ?? mmObj.AddComponent<MatchManager>();
        mm.currentGameMode = tdm;
        mm.spawnManager    = sm;
        mm.autoStartGame   = true;
        mm.gameStartDelay  = 3f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "TDM Configured ✅",
            "Created / updated:\n" +
            "  GameManager (10 min, first to 25)\n" +
            "  SpawnManager (bot spawns wired by tag)\n" +
            "  TeamDeathmatchMode\n" +
            "  MatchManager (auto-start 3s)\n\n" +
            "NEXT: assign Player prefab + Bot prefab(s)\n" +
            "on SpawnManager, then hit Play.",
            "OK");
    }
}
#endif
