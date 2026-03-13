#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.IO;

/// <summary>
/// Tools > Neural Strike > Build FFA Scene
/// Sets up a complete local FFA test scene:
///  - Loads Flooded Grounds environment (must be imported)
///  - Disables any Egyptian Metro root object if found
///  - Creates GameManager, SpawnManager, DeathmatchMode, MatchManager
///  - Creates Player GameObject wired with all components
///  - Creates HUD Canvas with ability bar above healthbar
///  - Auto-assigns materials from Assets/Game/Textures by matching name
/// </summary>
public static class NeuralStrikeSceneBuilder
{
    // ── CONFIGURE THESE PATHS TO MATCH YOUR IMPORTED ASSETS ─────────
    const string FLOODED_ROOT_NAME   = "FloodedGrounds"; // root GameObject name in the asset
    const string EGYPTIAN_ROOT_NAME  = "EgyptianMetro";  // root to disable (not delete)
    const string PLAYER_TAG          = "Player";
    const string SPAWN_PLAYER_TAG    = "SpawnPlayer";
    const string SPAWN_BOT_TAG       = "SpawnBot";
    const string FFA_SCENE_NAME      = "FFA_Test";
    const string TEXTURES_PATH       = "Assets/Game/Textures";
    const string MODELS_PATH         = "Assets/Game/Models";
    // ────────────────────────────────────────────────────────────────

    [MenuItem("Tools/Neural Strike/Build FFA Scene")]
    public static void BuildFFAScene()
    {
        // 1. DISABLE EGYPTIAN METRO (keep files, just deactivate root)
        var egyptian = GameObject.Find(EGYPTIAN_ROOT_NAME);
        if (egyptian != null)
        {
            egyptian.SetActive(false);
            Debug.Log($"[NeuralStrike] Disabled '{EGYPTIAN_ROOT_NAME}' in hierarchy.");
        }

        // 2. FIND OR ACTIVATE FLOODED GROUNDS
        var flooded = GameObject.Find(FLOODED_ROOT_NAME);
        if (flooded == null)
        {
            // Try to find it if inactive
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == FLOODED_ROOT_NAME && !go.activeInHierarchy)
                {
                    flooded = go;
                    flooded.SetActive(true);
                    break;
                }
            }
        }
        if (flooded == null)
            Debug.LogWarning("[NeuralStrike] FloodedGrounds root not found. " +
                             "Import the asset and make sure the root is named '" + FLOODED_ROOT_NAME + "'.");

        // 3. GAME MANAGER
        var gmObj = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        var gm    = gmObj.GetComponent<GameManager>() ?? gmObj.AddComponent<GameManager>();
        gm.matchDuration = 600f;
        gm.scoreToWin    = 30;

        // 4. SPAWN MANAGER
        var smObj = GameObject.Find("SpawnManager") ?? new GameObject("SpawnManager");
        var sm    = smObj.GetComponent<SpawnManager>() ?? smObj.AddComponent<SpawnManager>();
        sm.playerSpawnPoints = GameObject.FindGameObjectsWithTag(SPAWN_PLAYER_TAG)
                                         .Select(g => g.transform).ToArray();
        sm.botSpawnPoints    = GameObject.FindGameObjectsWithTag(SPAWN_BOT_TAG)
                                         .Select(g => g.transform).ToArray();
        sm.botsToSpawn       = Mathf.Max(4, sm.botSpawnPoints.Length);
        sm.spawnDelay        = 5f;

        // 5. DEATHMATCH MODE
        var dmObj = GameObject.Find("DeathmatchMode") ?? new GameObject("DeathmatchMode");
        var dm    = dmObj.GetComponent<DeathmatchMode>() ?? dmObj.AddComponent<DeathmatchMode>();
        dm.gameManager  = gm;
        dm.spawnManager = sm;

        // 6. MATCH MANAGER
        var mmObj = GameObject.Find("MatchManager") ?? new GameObject("MatchManager");
        var mm    = mmObj.GetComponent<MatchManager>() ?? mmObj.AddComponent<MatchManager>();
        mm.currentGameMode = dm;
        mm.spawnManager    = sm;
        mm.autoStartGame   = true;
        mm.gameStartDelay  = 3f;

        // 7. PLAYER
        var playerObj = GameObject.FindWithTag(PLAYER_TAG);
        if (playerObj == null)
        {
            playerObj = new GameObject("Player");
            playerObj.tag = PLAYER_TAG;
        }
        var cc  = playerObj.GetComponent<CharacterController>() ?? playerObj.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        var pc  = playerObj.GetComponent<PlayerController>()  ?? playerObj.AddComponent<PlayerController>();
        pc.gravity        = -9.81f;
        pc.jumpHeight     = 1.5f;
        pc.jumpSkillLevel = 0;

        playerObj.GetComponent<TeleportAbility>()  ?? playerObj.AddComponent<TeleportAbility>();
        playerObj.GetComponent<ApplyPlayerLoadout>()  ?? playerObj.AddComponent<ApplyPlayerLoadout>();

        // Ground check child
        Transform gc = playerObj.transform.Find("GroundCheck");
        if (gc == null)
        {
            gc = new GameObject("GroundCheck").transform;
            gc.SetParent(playerObj.transform);
            gc.localPosition = new Vector3(0, -0.9f, 0);
        }
        pc.groundCheck = gc;

        // Weapon holder
        Transform wh = playerObj.transform.Find("WeaponHolder");
        if (wh == null)
        {
            wh = new GameObject("WeaponHolder").transform;
            wh.SetParent(playerObj.transform);
            wh.localPosition = new Vector3(0.3f, 1.2f, 0.5f);
        }
        var pwc = playerObj.GetComponent<PlayerWeaponController>() ?? playerObj.AddComponent<PlayerWeaponController>();
        pwc.weaponHolder = wh;

        // Camera
        Transform cam = playerObj.transform.Find("MainCamera");
        if (cam == null)
        {
            cam = new GameObject("MainCamera").transform;
            cam.SetParent(playerObj.transform);
            cam.localPosition = new Vector3(0, 1.6f, 0);
            cam.gameObject.AddComponent<Camera>();
            cam.gameObject.AddComponent<AudioListener>();
            cam.gameObject.tag = "MainCamera";
        }
        var ml = cam.GetComponent<MouseLook>() ?? cam.gameObject.AddComponent<MouseLook>();

        // Health
        playerObj.GetComponent<Health>() ?? playerObj.AddComponent<Health>();

        // 8. AUTO-ASSIGN MATERIALS FROM TEXTURES FOLDER
        AutoAssignMaterials();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "FFA Scene Built ✅",
            "Done! Summary:\n" +
            "  • Egyptian Metro: disabled (not deleted)\n" +
            "  • GameManager, SpawnManager, DeathmatchMode, MatchManager created\n" +
            "  • Player wired (CharacterController, camera, weapons, health)\n" +
            "  • Materials auto-assigned by texture name matching\n\n" +
            "NEXT STEPS:\n" +
            "  1) Rename Flooded Grounds root to 'FloodedGrounds' if not already\n" +
            "  2) Tag spawn points SpawnPlayer / SpawnBot\n" +
            "  3) Assign bot prefabs on SpawnManager\n" +
            "  4) Add HUD Canvas and wire HUDManager references\n" +
            "  5) Hit Play!",
            "OK");
    }

    // ── MATERIAL AUTO-LINKER ─────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/Auto-Assign Materials")]
    public static void AutoAssignMaterials()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { MODELS_PATH });
        int matched = 0;

        foreach (var guid in matGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) continue;

            string baseName = Path.GetFileNameWithoutExtension(matPath);

            // Search for a texture with same base name in textures folder
            string[] texGuids = AssetDatabase.FindAssets($"{baseName} t:Texture2D", new[] { TEXTURES_PATH });
            if (texGuids.Length == 0) continue;

            string texPath = AssetDatabase.GUIDToAssetPath(texGuids[0]);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null) continue;

            mat.SetTexture("_BaseMap", tex);   // URP
            mat.SetTexture("_MainTex", tex);   // Built-in fallback
            EditorUtility.SetDirty(mat);
            matched++;
            Debug.Log($"[MaterialLinker] {baseName} → {texPath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialLinker] Auto-assigned {matched} material(s).");
    }
}
#endif
