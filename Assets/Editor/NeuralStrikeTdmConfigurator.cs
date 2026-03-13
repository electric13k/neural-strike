#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// Legacy TDM configurator — kept for reference but menu redirects to FFA builder.
/// Use Tools > Neural Strike > Build FFA Scene instead.
/// </summary>
public static class NeuralStrikeTdmConfigurator
{
    [MenuItem("Tools/Neural Strike/Configure TDM (Legacy)")]
    public static void ConfigureTdm()
    {
        // ── GAME MANAGER ───────────────────────────────────────────────
        GameObject gmObj = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        GameManager gm   = gmObj.GetComponent<GameManager>() ?? gmObj.AddComponent<GameManager>();
        gm.matchDuration = 600f;
        gm.scoreToWin    = 25;

        // ── SPAWN MANAGER ──────────────────────────────────────────────
        GameObject smObj = GameObject.Find("SpawnManager") ?? new GameObject("SpawnManager");
        SpawnManager sm  = smObj.GetComponent<SpawnManager>() ?? smObj.AddComponent<SpawnManager>();

        // Safe tag lookup — won't crash if tag doesn't exist
        try { sm.playerSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPlayer").Select(g => g.transform).ToArray(); }
        catch { sm.playerSpawnPoints = new UnityEngine.Transform[0]; Debug.LogWarning("[TDM] Tag 'SpawnPlayer' not defined — add it in Project Settings > Tags & Layers."); }

        try { sm.botSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnBot").Select(g => g.transform).ToArray(); }
        catch { sm.botSpawnPoints = new UnityEngine.Transform[0]; Debug.LogWarning("[TDM] Tag 'SpawnBot' not defined — add it in Project Settings > Tags & Layers."); }

        sm.botsToSpawn = Mathf.Clamp(sm.botSpawnPoints.Length, 0, 12);
        sm.spawnDelay  = 5f;

        // ── TEAM DEATHMATCH MODE ───────────────────────────────────────
        GameObject modeObj        = GameObject.Find("TeamDeathmatchMode") ?? new GameObject("TeamDeathmatchMode");
        TeamDeathmatchMode tdm    = modeObj.GetComponent<TeamDeathmatchMode>() ?? modeObj.AddComponent<TeamDeathmatchMode>();
        tdm.gameManager           = gm;

        // ── MATCH MANAGER ──────────────────────────────────────────────
        GameObject mmObj = GameObject.Find("MatchManager") ?? new GameObject("MatchManager");
        MatchManager mm  = mmObj.GetComponent<MatchManager>() ?? mmObj.AddComponent<MatchManager>();
        mm.currentGameMode = tdm;
        mm.spawnManager    = sm;
        mm.autoStartGame   = true;
        mm.gameStartDelay  = 3f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("TDM Configured", "Done. Use Tools > Neural Strike > Build FFA Scene for the new FFA test mode.", "OK");
    }
}
#endif
