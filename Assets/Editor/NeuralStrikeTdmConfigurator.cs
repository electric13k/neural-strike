#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

/// <summary>
/// Legacy TDM configurator - kept for reference.
/// Use Tools > Neural Strike > 2) Build FFA Scene for FFA mode.
/// </summary>
public static class NeuralStrikeTdmConfigurator
{
    [MenuItem("Tools/Neural Strike/Configure TDM (Legacy)")]
    public static void ConfigureTdm()
    {
        GameObject gmObj = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        GameManager gm   = gmObj.GetComponent<GameManager>() ?? gmObj.AddComponent<GameManager>();
        gm.matchDuration = 600f;
        gm.scoreToWin    = 25;

        GameObject smObj = GameObject.Find("SpawnManager") ?? new GameObject("SpawnManager");
        SpawnManager sm  = smObj.GetComponent<SpawnManager>() ?? smObj.AddComponent<SpawnManager>();

        try { sm.playerSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPlayer").Select(g => g.transform).ToArray(); }
        catch { sm.playerSpawnPoints = new UnityEngine.Transform[0]; Debug.LogWarning("[TDM] Tag 'SpawnPlayer' not defined. Run Tools > Neural Strike > 1) Setup Required Tags."); }

        try { sm.botSpawnPoints = GameObject.FindGameObjectsWithTag("SpawnBot").Select(g => g.transform).ToArray(); }
        catch { sm.botSpawnPoints = new UnityEngine.Transform[0]; Debug.LogWarning("[TDM] Tag 'SpawnBot' not defined. Run Tools > Neural Strike > 1) Setup Required Tags."); }

        sm.botsToSpawn = Mathf.Clamp(sm.botSpawnPoints.Length, 0, 12);
        sm.spawnDelay  = 5f;

        GameObject modeObj     = GameObject.Find("TeamDeathmatchMode") ?? new GameObject("TeamDeathmatchMode");
        TeamDeathmatchMode tdm = modeObj.GetComponent<TeamDeathmatchMode>() ?? modeObj.AddComponent<TeamDeathmatchMode>();
        tdm.gameManager        = gm;

        GameObject mmObj = GameObject.Find("MatchManager") ?? new GameObject("MatchManager");
        MatchManager mm  = mmObj.GetComponent<MatchManager>() ?? mmObj.AddComponent<MatchManager>();
        mm.currentGameMode = tdm;
        mm.spawnManager    = sm;
        mm.autoStartGame   = true;
        mm.gameStartDelay  = 3f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("TDM Configured",
            "Done.\n\nFor FFA mode use: Tools > Neural Strike > 2) Build FFA Scene", "OK");
    }
}
#endif
