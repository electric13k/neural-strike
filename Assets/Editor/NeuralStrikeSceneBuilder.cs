#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.IO;

/// <summary>
/// Tools > Neural Strike > 2) Build FFA Scene
/// Run "1) Setup Required Tags" first.
/// Disables Egyptian Metro root, activates Flooded Grounds,
/// wires all managers + player. Never throws on missing tags.
/// </summary>
public static class NeuralStrikeSceneBuilder
{
    const string FLOODED_ROOT_NAME  = "FloodedGrounds";
    const string EGYPTIAN_ROOT_NAME = "EgyptianMetro";
    const string PLAYER_TAG         = "Player";
    const string SPAWN_PLAYER_TAG   = "SpawnPlayer";
    const string SPAWN_BOT_TAG      = "SpawnBot";
    const string TEXTURES_PATH      = "Assets/Game/Textures";
    const string MODELS_PATH        = "Assets/Game/Models";

    [MenuItem("Tools/Neural Strike/2) Build FFA Scene")]
    public static void BuildFFAScene()
    {
        TrySetActive(EGYPTIAN_ROOT_NAME, false);

        bool foundMap = TrySetActive(FLOODED_ROOT_NAME, true);
        if (!foundMap)
            Debug.LogWarning("[NeuralStrike] '" + FLOODED_ROOT_NAME +
                "' not found. Rename your Flooded Grounds root GameObject to '" +
                FLOODED_ROOT_NAME + "' and re-run.");

        var gm = GetOrCreate<GameManager>("GameManager");
        gm.matchDuration = 600f;
        gm.scoreToWin    = 30;

        var sm = GetOrCreate<SpawnManager>("SpawnManager");
        sm.playerSpawnPoints = SafeFindByTag(SPAWN_PLAYER_TAG);
        sm.botSpawnPoints    = SafeFindByTag(SPAWN_BOT_TAG);
        sm.botsToSpawn       = Mathf.Max(4, sm.botSpawnPoints.Length);
        sm.spawnDelay        = 5f;

        var dm = GetOrCreate<DeathmatchMode>("DeathmatchMode");
        dm.gameManager  = gm;
        dm.spawnManager = sm;

        var mm = GetOrCreate<MatchManager>("MatchManager");
        mm.currentGameMode = dm;
        mm.spawnManager    = sm;
        mm.autoStartGame   = true;
        mm.gameStartDelay  = 3f;

        BuildPlayer();
        AutoAssignMaterials();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "FFA Scene Built",
            "Done!\n\n" +
            "  - Egyptian Metro: disabled (files kept)\n" +
            "  - GameManager / SpawnManager / DeathmatchMode / MatchManager created\n" +
            "  - Player wired (CharacterController + Camera + Health + Weapons)\n" +
            "  - Materials auto-linked by name\n\n" +
            "NEXT STEPS:\n" +
            "  1) Rename Flooded Grounds root to 'FloodedGrounds' if not visible\n" +
            "  2) Tag spawn points SpawnPlayer / SpawnBot in Inspector\n" +
            "  3) Assign bot prefabs on SpawnManager\n" +
            "  4) Window > Rendering > Lighting > Generate Lighting\n" +
            "  5) Hit Play!",
            "OK");
    }

    private static void BuildPlayer()
    {
        GameObject playerObj = null;
        try   { playerObj = GameObject.FindWithTag(PLAYER_TAG); }
        catch { }

        if (playerObj == null)
        {
            playerObj = new GameObject("Player");
            try { playerObj.tag = PLAYER_TAG; } catch { }
        }

        var cc = EnsureComponent<CharacterController>(playerObj);
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        var pc = EnsureComponent<PlayerController>(playerObj);
        pc.gravity        = -9.81f;
        pc.jumpHeight     = 1.5f;
        pc.jumpSkillLevel = 0;

        EnsureComponent<TeleportAbility>(playerObj);
        EnsureComponent<ApplyPlayerLoadout>(playerObj);
        EnsureComponent<Health>(playerObj);

        var gc = playerObj.transform.Find("GroundCheck");
        if (gc == null)
        {
            gc = new GameObject("GroundCheck").transform;
            gc.SetParent(playerObj.transform);
            gc.localPosition = new Vector3(0, -0.9f, 0);
        }
        pc.groundCheck = gc;

        var wh = playerObj.transform.Find("WeaponHolder");
        if (wh == null)
        {
            wh = new GameObject("WeaponHolder").transform;
            wh.SetParent(playerObj.transform);
            wh.localPosition = new Vector3(0.3f, 1.2f, 0.5f);
        }
        var pwc = EnsureComponent<PlayerWeaponController>(playerObj);
        pwc.weaponHolder = wh;

        var cam = playerObj.transform.Find("MainCamera");
        if (cam == null)
        {
            cam = new GameObject("MainCamera").transform;
            cam.SetParent(playerObj.transform);
            cam.localPosition = new Vector3(0, 1.6f, 0);
            cam.gameObject.AddComponent<Camera>();
            cam.gameObject.AddComponent<AudioListener>();
            try { cam.gameObject.tag = "MainCamera"; } catch { }
        }
        EnsureComponent<MouseLook>(cam.gameObject);
    }

    [MenuItem("Tools/Neural Strike/3) Auto-Assign Materials")]
    public static void AutoAssignMaterials()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { MODELS_PATH });
        int matched = 0;

        foreach (var guid in matGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            var    mat     = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) continue;

            string   baseName = Path.GetFileNameWithoutExtension(matPath);
            string[] texGuids = AssetDatabase.FindAssets(baseName + " t:Texture2D", new[] { TEXTURES_PATH });
            if (texGuids.Length == 0) continue;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(texGuids[0]));
            if (tex == null) continue;

            mat.SetTexture("_BaseMap", tex);
            mat.SetTexture("_MainTex", tex);
            EditorUtility.SetDirty(mat);
            matched++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[MaterialLinker] Auto-assigned " + matched + " material(s).");
    }

    // ── Helpers ─────────────────────────────────────────────────────

    /// <summary>Gets existing component or adds a new one. Replaces bare ?? AddComponent pattern.</summary>
    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    static T GetOrCreate<T>(string goName) where T : Component
    {
        var go = GameObject.Find(goName) ?? new GameObject(goName);
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    static Transform[] SafeFindByTag(string tag)
    {
        try
        {
            return GameObject.FindGameObjectsWithTag(tag)
                             .Select(g => g.transform).ToArray();
        }
        catch
        {
            Debug.LogWarning("[NeuralStrike] Tag '" + tag +
                "' missing. Run Tools > Neural Strike > 1) Setup Required Tags first.");
            return new Transform[0];
        }
    }

    static bool TrySetActive(string name, bool active)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.name == name && obj.scene.IsValid())
                { go = obj; break; }
            }
        }
        if (go != null) { go.SetActive(active); return true; }
        return false;
    }
}
#endif
