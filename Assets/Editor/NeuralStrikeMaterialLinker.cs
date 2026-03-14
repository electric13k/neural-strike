#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Tools > Neural Strike > 3) Link Materials + Textures
/// Tools > Neural Strike > 4) Wire FBX Animators
///
/// Tool 3: For every .obj in Assets/Models:
///   - Forces Unity to re-import it (generates .meta)
///   - Finds matching .mtl in Assets/Textures
///   - Creates/updates a Material in Assets/Materials/<botname>/
///   - Assigns basecolor -> _BaseMap/_MainTex
///              normal   -> _BumpMap
///              metallic -> _MetallicGlossMap
///              roughness-> _SpecGlossMap
///   - Assigns that material to the OBJ's MeshRenderer
///
/// Tool 4: For every .fbx in Assets/Animations:
///   - Forces re-import
///   - Groups by bot prefix
///   - Creates AnimatorController at Assets/Animations/<bot>_Controller.controller
///   - Adds one state per AnimationClip found in the FBX
/// </summary>
public static class NeuralStrikeMaterialLinker
{
    const string MODELS_PATH    = "Assets/Models";
    const string TEXTURES_PATH  = "Assets/Textures";
    const string ANIM_PATH      = "Assets/Animations";
    const string MATERIALS_PATH = "Assets/Materials";

    // ─────────────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/3) Link Materials + Textures")]
    public static void LinkMaterials()
    {
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(MATERIALS_PATH))
            AssetDatabase.CreateFolder("Assets", "Materials");

        // Step 1: Force-import everything in Models and Textures so .meta exist
        ForceImportFolder(MODELS_PATH);
        ForceImportFolder(TEXTURES_PATH);
        AssetDatabase.Refresh();

        // Step 2: Build texture lookup: "botname_suffix" -> Texture2D
        var texMap = BuildTexMap();
        Debug.Log("[MatLinker] Found " + texMap.Count + " textures in " + TEXTURES_PATH);

        // Step 3: Process each .obj
        string[] objGuids = AssetDatabase.FindAssets("t:Mesh", new[] { MODELS_PATH });
        // Also scan by file extension since Unity may not yet see them as Mesh
        var objPaths = Directory.GetFiles(
            Path.Combine(Application.dataPath, "Models"), "*.obj", SearchOption.AllDirectories)
            .Select(p => "Assets/Models/" + Path.GetFileName(p))
            .ToList();

        // Add any Unity already knows about
        foreach (var g in objGuids)
            objPaths.Add(AssetDatabase.GUIDToAssetPath(g));
        objPaths = objPaths.Distinct().ToList();

        int linked = 0;
        foreach (var objPath in objPaths)
        {
            if (!objPath.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase)) continue;

            // Force import this specific file
            AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);

            string botName = Path.GetFileNameWithoutExtension(objPath).ToLower(); // e.g. "courierbot"

            // Create material folder per bot
            string matFolder = MATERIALS_PATH + "/" + botName;
            if (!AssetDatabase.IsValidFolder(matFolder))
                AssetDatabase.CreateFolder(MATERIALS_PATH, botName);

            string matPath = matFolder + "/" + botName + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, matPath);
            }

            bool anyTex = false;
            anyTex |= TryAssignTex(mat, texMap, botName + "_basecolor", "_BaseMap", "_MainTex");
            TryAssignTex(mat, texMap, botName + "_normal",    "_BumpMap",         null);
            TryAssignTex(mat, texMap, botName + "_metallic",  "_MetallicGlossMap",null);
            TryAssignTex(mat, texMap, botName + "_roughness", "_SpecGlossMap",     null);

            EditorUtility.SetDirty(mat);

            // Assign to OBJ renderer if it has one
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
            if (go != null)
            {
                var renderers = go.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    r.sharedMaterials = mats;
                }
                EditorUtility.SetDirty(go);
            }

            if (anyTex) linked++;
            Debug.Log("[MatLinker] " + botName + ": mat created, textures linked=" + anyTex);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Materials Linked",
            "OBJs processed: " + objPaths.Count(p => p.EndsWith(".obj",System.StringComparison.OrdinalIgnoreCase)) + "\n" +
            "Materials with textures: " + linked + "\n\n" +
            "If count is 0, run 'git pull' and re-import in Unity first, then retry.",
            "OK");
    }

    // ─────────────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/4) Wire FBX Animators")]
    public static void WireAnimators()
    {
        ForceImportFolder(ANIM_PATH);
        AssetDatabase.Refresh();

        // Collect all AnimationClips from FBX files in ANIM_PATH
        var groups = new Dictionary<string, List<AnimationClip>>();

        var fbxFiles = Directory.GetFiles(
            Path.Combine(Application.dataPath, "Animations"), "*.fbx",
            SearchOption.AllDirectories);

        foreach (var fbxFile in fbxFiles)
        {
            string assetPath = "Assets/Animations/" + Path.GetFileName(fbxFile);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            // Load all assets from this FBX
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var clips  = assets.OfType<AnimationClip>()
                               .Where(c => !c.name.StartsWith("__")) // skip internal Unity clips
                               .ToList();

            if (clips.Count == 0)
            {
                Debug.LogWarning("[FBXAnim] No clips in: " + assetPath +
                    " (file may be a Git LFS stub - run git lfs pull)");
                continue;
            }

            string fname  = Path.GetFileNameWithoutExtension(assetPath);
            string prefix = fname.Contains('_') ? fname.Substring(0, fname.IndexOf('_')) : fname;

            if (!groups.ContainsKey(prefix))
                groups[prefix] = new List<AnimationClip>();
            groups[prefix].AddRange(clips);
        }

        int created = 0;
        foreach (var kv in groups)
        {
            string ctrlPath = ANIM_PATH + "/" + kv.Key + "_Controller.controller";
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath)
                       ?? AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

            var root = ctrl.layers[0].stateMachine;

            foreach (var clip in kv.Value)
            {
                bool exists = root.states.Any(s => s.state.name == clip.name);
                if (!exists)
                {
                    var st = root.AddState(clip.name);
                    st.motion = clip;
                    Debug.Log("[FBXAnim] " + kv.Key + " <- " + clip.name);
                }
            }

            // Set first non-idle clip as default, or idle if present
            var idleState = root.states.FirstOrDefault(s =>
                s.state.name.ToLower().Contains("idle"));
            if (idleState.state != null)
                root.defaultState = idleState.state;

            EditorUtility.SetDirty(ctrl);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string msg = created > 0
            ? "Created/updated " + created + " AnimatorController(s) in Assets/Animations/"
            : "No clips found.\n\nYour FBX files may be Git LFS stubs.\n" +
              "Run in terminal:\n  git lfs install\n  git lfs pull\nThen retry.";

        EditorUtility.DisplayDialog("FBX Animators", msg, "OK");
    }

    // ── Helpers ───────────────────────────────────────────────
    static Dictionary<string, string> BuildTexMap()
    {
        var map = new Dictionary<string, string>();
        if (!Directory.Exists(Path.Combine(Application.dataPath, "Textures"))) return map;

        var files = Directory.GetFiles(
            Path.Combine(Application.dataPath, "Textures"),
            "*", SearchOption.AllDirectories)
            .Where(f => new[]{"png","jpg","jpeg"}.Contains(
                Path.GetExtension(f).TrimStart('.').ToLower()));

        foreach (var f in files)
        {
            string key = Path.GetFileNameWithoutExtension(f).ToLower();
            string assetPath = "Assets/Textures/" + Path.GetFileName(f);
            if (!map.ContainsKey(key)) map[key] = assetPath;
        }
        return map;
    }

    static bool TryAssignTex(Material mat, Dictionary<string,string> map,
                              string key, string prop1, string prop2)
    {
        if (!map.TryGetValue(key, out string path)) return false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return false;
        mat.SetTexture(prop1, tex);
        if (prop2 != null) mat.SetTexture(prop2, tex);
        return true;
    }

    static void ForceImportFolder(string assetFolder)
    {
        string fullPath = Path.Combine(
            Application.dataPath.Replace("/Assets",""),
            assetFolder.Replace("/",Path.DirectorySeparatorChar.ToString()));

        if (!Directory.Exists(fullPath)) return;

        foreach (var f in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
        {
            if (f.EndsWith(".meta")) continue;
            string rel = "Assets" + f
                .Replace(Application.dataPath, "")
                .Replace("\\", "/");
            AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
#endif
