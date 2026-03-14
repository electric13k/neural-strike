#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using System.Linq;
using System.IO;

/// <summary>
/// Tools > Neural Strike
///   1) Setup Required Tags
///   2) Build FFA Scene   <- main tool
///   3) Auto-Assign Materials
///   4) Wire FBX Animators
/// Run in order 1 -> 2.
/// </summary>
public static class NeuralStrikeSceneBuilder
{
    // ── Paths ───────────────────────────────────────────────────
    const string FLOODED_ROOT   = "FloodedGrounds";
    const string EGYPTIAN_ROOT  = "EgyptianMetro";
    const string PLAYER_TAG     = "Player";
    const string SPAWN_PLAYER   = "SpawnPlayer";
    const string SPAWN_BOT      = "SpawnBot";
    const string TEX_PATH       = "Assets/Textures";
    const string MODELS_PATH    = "Assets/Models";
    const string ANIM_PATH      = "Assets/Animations";

    // ────────────────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/2) Build FFA Scene")]
    public static void BuildFFAScene()
    {
        // 1. Egyptian Metro -> disabled, not deleted
        int disableCount = 0;
        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!obj.scene.IsValid()) continue;
            if (obj.transform.parent != null) continue; // only root objects
            string n = obj.name.ToLower();
            if (n.Contains("egyptian") || n.Contains("metro") || n == "map_egyptian_metro")
            {
                obj.SetActive(false);
                EditorUtility.SetDirty(obj);
                disableCount++;
                Debug.Log("[NeuralStrike] Disabled: " + obj.name);
            }
        }
        if (disableCount == 0)
            Debug.LogWarning("[NeuralStrike] No Egyptian Metro root found to disable.");

        // 2. Flooded Grounds -> find or warn
        bool foundMap = false;
        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!obj.scene.IsValid()) continue;
            if (obj.transform.parent != null) continue;
            string n = obj.name.ToLower();
            if (n.Contains("flooded") || n.Contains("flood") || n == FLOODED_ROOT.ToLower())
            {
                obj.SetActive(true);
                EditorUtility.SetDirty(obj);
                foundMap = true;
                Debug.Log("[NeuralStrike] Activated map: " + obj.name);
                break;
            }
        }
        if (!foundMap)
            Debug.LogWarning("[NeuralStrike] Flooded Grounds root not in scene yet. " +
                "Drag Assets/FloodedGrounds/Scenes/FloodedGrounds.unity into your scene hierarchy, then re-run.");

        // 3. Managers
        var gm = EnsureGO<GameManager>("GameManager");
        gm.matchDuration = 600f;
        gm.scoreToWin    = 30;

        var sm = EnsureGO<SpawnManager>("SpawnManager");
        sm.playerSpawnPoints = SafeTag(SPAWN_PLAYER);
        sm.botSpawnPoints    = SafeTag(SPAWN_BOT);
        sm.botsToSpawn       = Mathf.Max(4, sm.botSpawnPoints.Length);
        sm.spawnDelay        = 5f;

        var dm = EnsureGO<DeathmatchMode>("DeathmatchMode");
        dm.gameManager  = gm;
        dm.spawnManager = sm;

        var mm = EnsureGO<MatchManager>("MatchManager");
        mm.currentGameMode = dm;
        mm.spawnManager    = sm;
        mm.autoStartGame   = true;
        mm.gameStartDelay  = 3f;

        // 4. Player
        BuildPlayer();

        // 5. Materials
        int matCount = AutoAssignMaterials();

        // 6. FBX animators
        int animCount = WireFbxAnimators();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("FFA Scene Built",
            "Egyptian Metro roots disabled: " + disableCount + "\n" +
            "Flooded Grounds active: " + foundMap + "\n" +
            "Materials linked: " + matCount + "\n" +
            "FBX animators wired: " + animCount + "\n\n" +
            "NEXT:\n" +
            "  - If map missing: drag FloodedGrounds scene into Hierarchy\n" +
            "  - Tag spawn points SpawnPlayer / SpawnBot\n" +
            "  - Assign bot prefabs on SpawnManager\n" +
            "  - Window > Rendering > Lighting > Generate Lighting\n" +
            "  - Hit Play!", "OK");
    }

    // ────────────────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/3) Auto-Assign Materials")]
    public static int AutoAssignMaterials()
    {
        // Find ALL materials anywhere under Assets
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        int matched = 0;

        // Build a fast lookup: textureName(lower, no ext) -> assetPath
        var texMap = new System.Collections.Generic.Dictionary<string, string>();
        string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { TEX_PATH });
        foreach (var tg in texGuids)
        {
            string tp = AssetDatabase.GUIDToAssetPath(tg);
            string tn = Path.GetFileNameWithoutExtension(tp).ToLower();
            if (!texMap.ContainsKey(tn)) texMap[tn] = tp;
        }

        foreach (var mg in matGuids)
        {
            string mp  = AssetDatabase.GUIDToAssetPath(mg);
            // Skip packages and FloodedGrounds PostProcessing
            if (mp.StartsWith("Packages/")) continue;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (mat == null) continue;

            string baseName = Path.GetFileNameWithoutExtension(mp).ToLower();

            // Try exact match first, then suffix matches
            string[] suffixes = { "", "_basecolor", "_albedo", "_diffuse" };
            foreach (var suf in suffixes)
            {
                string key = baseName + suf;
                if (texMap.TryGetValue(key, out string texPath))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    if (tex != null)
                    {
                        mat.SetTexture("_BaseMap", tex);
                        mat.SetTexture("_MainTex", tex);

                        // Also try to link normal / metallic / roughness
                        TrySetTex(mat, texMap, baseName + "_normal",    "_BumpMap");
                        TrySetTex(mat, texMap, baseName + "_metallic",   "_MetallicGlossMap");
                        TrySetTex(mat, texMap, baseName + "_roughness",  "_SpecGlossMap");

                        EditorUtility.SetDirty(mat);
                        matched++;
                        Debug.Log("[MaterialLinker] " + Path.GetFileName(mp) + " -> " + Path.GetFileName(texPath));
                        break;
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[MaterialLinker] Linked " + matched + " material(s).");
        return matched;
    }

    static void TrySetTex(Material mat,
        System.Collections.Generic.Dictionary<string,string> map,
        string key, string prop)
    {
        if (map.TryGetValue(key, out string p))
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (t != null) mat.SetTexture(prop, t);
        }
    }

    // ────────────────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/4) Wire FBX Animators")]
    public static int WireFbxAnimators()
    {
        // For each .fbx in Assets/Animations, create an AnimatorController
        // that has one state per FBX clip, named by the file.
        string[] fbxGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { ANIM_PATH });

        // Group by bot prefix (everything before first '_')
        var groups = new System.Collections.Generic.Dictionary<
            string,
            System.Collections.Generic.List<AnimationClip>>();

        foreach (var fg in fbxGuids)
        {
            string fp   = AssetDatabase.GUIDToAssetPath(fg);
            var    clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fp);
            if (clip == null) continue;

            string fname  = Path.GetFileNameWithoutExtension(fp);
            string prefix = fname.Contains('_') ? fname.Substring(0, fname.IndexOf('_')) : fname;

            if (!groups.ContainsKey(prefix))
                groups[prefix] = new System.Collections.Generic.List<AnimationClip>();
            groups[prefix].Add(clip);
        }

        int count = 0;
        foreach (var kv in groups)
        {
            string ctrlPath = ANIM_PATH + "/" + kv.Key + "_Controller.controller";
            AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (ctrl == null)
                ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

            var root = ctrl.layers[0].stateMachine;

            foreach (var clip in kv.Value)
            {
                string stateName = clip.name;
                // Avoid duplicate states
                bool exists = root.states.Any(s => s.state.name == stateName);
                if (!exists)
                {
                    var state = root.AddState(stateName);
                    state.motion = clip;
                }
            }

            EditorUtility.SetDirty(ctrl);
            Debug.Log("[Animator] " + kv.Key + " controller: " + kv.Value.Count + " clip(s)");
            count++;
        }

        AssetDatabase.SaveAssets();
        return count;
    }

    // ── Player builder ───────────────────────────────────────────────
    static void BuildPlayer()
    {
        GameObject p = null;
        try { p = GameObject.FindWithTag(PLAYER_TAG); } catch { }
        if (p == null) { p = new GameObject("Player"); try { p.tag = PLAYER_TAG; } catch { } }

        var cc = EnsureComp<CharacterController>(p);
        cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0, 0.9f, 0);

        var pc = EnsureComp<PlayerController>(p);
        pc.gravity = -9.81f; pc.jumpHeight = 1.5f; pc.jumpSkillLevel = 0;

        EnsureComp<TeleportAbility>(p);
        EnsureComp<ApplyPlayerLoadout>(p);
        EnsureComp<Health>(p);

        var gc = p.transform.Find("GroundCheck");
        if (gc == null) { gc = new GameObject("GroundCheck").transform; gc.SetParent(p.transform); gc.localPosition = new Vector3(0,-0.9f,0); }
        pc.groundCheck = gc;

        var wh = p.transform.Find("WeaponHolder");
        if (wh == null) { wh = new GameObject("WeaponHolder").transform; wh.SetParent(p.transform); wh.localPosition = new Vector3(0.3f,1.2f,0.5f); }
        EnsureComp<PlayerWeaponController>(p).weaponHolder = wh;

        var cam = p.transform.Find("MainCamera");
        if (cam == null)
        {
            cam = new GameObject("MainCamera").transform;
            cam.SetParent(p.transform);
            cam.localPosition = new Vector3(0, 1.6f, 0);
            cam.gameObject.AddComponent<Camera>();
            cam.gameObject.AddComponent<AudioListener>();
            try { cam.gameObject.tag = "MainCamera"; } catch { }
        }
        EnsureComp<MouseLook>(cam.gameObject);
    }

    // ── Helpers ────────────────────────────────────────────────────
    static T EnsureGO<T>(string name) where T : Component
    {
        var go = GameObject.Find(name) ?? new GameObject(name);
        var c  = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    static T EnsureComp<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    static Transform[] SafeTag(string tag)
    {
        try { return GameObject.FindGameObjectsWithTag(tag).Select(g => g.transform).ToArray(); }
        catch { return new Transform[0]; }
    }
}
#endif
