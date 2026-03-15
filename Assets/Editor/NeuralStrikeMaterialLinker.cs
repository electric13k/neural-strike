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
/// Fixes applied vs previous version:
///   - ForceImportFolder: was building path as "Assets" + absolutePath
///     which produced "AssetsD:/..." on Windows.  Now uses
///     Application.dataPath (the Assets folder absolute path) to build
///     the full disk path, and converts back to a proper "Assets/..."
///     relative path for the AssetDatabase calls.
///   - WireAnimators: dots in clip names (e.g. "mixamo.com") cause
///     Unity's AnimatorStateMachine.AddState to throw a warning and
///     skip the state.  Names are now sanitised (dots replaced with _)
///     before being used as state names.
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
        if (!AssetDatabase.IsValidFolder(MATERIALS_PATH))
            AssetDatabase.CreateFolder("Assets", "Materials");

        ForceImportFolder(MODELS_PATH);
        ForceImportFolder(TEXTURES_PATH);
        AssetDatabase.Refresh();

        var texMap = BuildTexMap();
        Debug.Log("[MatLinker] Found " + texMap.Count + " textures in " + TEXTURES_PATH);

        // Collect .obj paths by scanning disk (avoids Unity not yet knowing about them)
        string modelsDiskPath = AssetToDiskPath(MODELS_PATH);
        var objPaths = new List<string>();
        if (Directory.Exists(modelsDiskPath))
        {
            foreach (var f in Directory.GetFiles(modelsDiskPath, "*.obj", SearchOption.AllDirectories))
                objPaths.Add(DiskToAssetPath(f));
        }
        // Also add anything Unity already sees as a Mesh
        foreach (var g in AssetDatabase.FindAssets("t:Mesh", new[] { MODELS_PATH }))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (!objPaths.Contains(p)) objPaths.Add(p);
        }
        objPaths = objPaths.Distinct()
                           .Where(p => p.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase))
                           .ToList();

        int linked = 0;
        foreach (var objPath in objPaths)
        {
            AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);

            string botName  = Path.GetFileNameWithoutExtension(objPath).ToLower();
            string matFolder = MATERIALS_PATH + "/" + botName;
            if (!AssetDatabase.IsValidFolder(matFolder))
                AssetDatabase.CreateFolder(MATERIALS_PATH, botName);

            string   matPath = matFolder + "/" + botName + ".mat";
            Material mat     = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                               ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, matPath);
            }

            bool anyTex = false;
            anyTex |= TryAssignTex(mat, texMap, botName + "_basecolor", "_BaseMap",         "_MainTex");
            TryAssignTex(mat, texMap, botName + "_normal",    "_BumpMap",          null);
            TryAssignTex(mat, texMap, botName + "_metallic",  "_MetallicGlossMap",  null);
            TryAssignTex(mat, texMap, botName + "_roughness", "_SpecGlossMap",      null);

            EditorUtility.SetDirty(mat);

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
            if (go != null)
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    r.sharedMaterials = mats;
                    EditorUtility.SetDirty(go);
                }
            }

            if (anyTex) linked++;
            Debug.Log("[MatLinker] " + botName + ": mat created, textures linked=" + anyTex);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Materials Linked",
            "OBJs processed: "  + objPaths.Count + "\n" +
            "With textures: "   + linked + "\n\n" +
            (linked == 0 ? "No textures matched.  Make sure your texture files are in Assets/Textures/\n" +
                           "and are named  botname_basecolor.jpeg  etc." : "Done!"),
            "OK");
    }

    // ─────────────────────────────────────────────────
    [MenuItem("Tools/Neural Strike/4) Wire FBX Animators")]
    public static void WireAnimators()
    {
        ForceImportFolder(ANIM_PATH);
        AssetDatabase.Refresh();

        var groups = new Dictionary<string, List<(string stateName, AnimationClip clip)>>();

        string animDiskPath = AssetToDiskPath(ANIM_PATH);
        if (!Directory.Exists(animDiskPath))
        {
            EditorUtility.DisplayDialog("FBX Animators",
                "Assets/Animations folder not found.", "OK");
            return;
        }

        foreach (var fbxFile in Directory.GetFiles(animDiskPath, "*.fbx", SearchOption.AllDirectories))
        {
            string assetPath = DiskToAssetPath(fbxFile);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var clips = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                                     .OfType<AnimationClip>()
                                     .Where(c => !c.name.StartsWith("__"))
                                     .ToList();

            if (clips.Count == 0)
            {
                Debug.LogWarning("[FBXAnim] No clips in " + assetPath +
                    " — may be a Git LFS stub. Run: git lfs pull");
                continue;
            }

            string fname  = Path.GetFileNameWithoutExtension(assetPath);
            string prefix = fname.Contains('_') ? fname.Substring(0, fname.IndexOf('_')) : fname;

            if (!groups.ContainsKey(prefix))
                groups[prefix] = new List<(string, AnimationClip)>();

            foreach (var clip in clips)
            {
                // FIX: dots not allowed in AnimatorState names (e.g. "mixamo.com")
                string safeName = clip.name.Replace('.', '_').Replace(' ', '_');
                groups[prefix].Add((safeName, clip));
            }
        }

        int created = 0;
        foreach (var kv in groups)
        {
            string ctrlPath = ANIM_PATH + "/" + kv.Key + "_Controller.controller";
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath)
                       ?? AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

            var root = ctrl.layers[0].stateMachine;

            foreach (var (stateName, clip) in kv.Value)
            {
                bool exists = root.states.Any(s => s.state.name == stateName);
                if (!exists)
                {
                    var st  = root.AddState(stateName);
                    st.motion = clip;
                    Debug.Log("[FBXAnim] " + kv.Key + " ← state: " + stateName);
                }
            }

            // Prefer idle as default state
            var idleState = root.states.FirstOrDefault(s =>
                s.state.name.ToLower().Contains("idle"));
            if (idleState.state != null)
                root.defaultState = idleState.state;

            EditorUtility.SetDirty(ctrl);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("FBX Animators",
            created > 0
                ? "Created/updated " + created + " controller(s) in Assets/Animations/"
                : "No FBX clips found.\nRun  git lfs pull  then retry.",
            "OK");
    }

    // ── Helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Convert an "Assets/..." relative path to an absolute disk path.
    /// Application.dataPath already ends with "/Assets" so we strip
    /// the leading "Assets" from the input before joining.
    /// </summary>
    static string AssetToDiskPath(string assetPath)
    {
        // assetPath:  "Assets/Models"  or  "Assets/Textures"
        // dataPath:   "D:/Projects/.../Assets"
        string relative = assetPath.StartsWith("Assets/") || assetPath.StartsWith("Assets\\") 
                        ? assetPath.Substring("Assets".Length).TrimStart('/', '\\')
                        : assetPath;
        return Path.Combine(Application.dataPath, relative);
    }

    /// <summary>
    /// Convert an absolute disk path back to "Assets/..." relative path.
    /// </summary>
    static string DiskToAssetPath(string diskPath)
    {
        // dataPath = "D:/Projects/.../Assets"
        string normalized = diskPath.Replace('\\', '/');
        string dataPath   = Application.dataPath.Replace('\\', '/');

        if (normalized.StartsWith(dataPath))
            return "Assets" + normalized.Substring(dataPath.Length);

        // Fallback: try stripping up to "/Assets/"
        int idx = normalized.IndexOf("/Assets/");
        return idx >= 0 ? normalized.Substring(idx + 1) : diskPath;
    }

    static Dictionary<string, string> BuildTexMap()
    {
        var map = new Dictionary<string, string>();
        string diskPath = AssetToDiskPath(TEXTURES_PATH);
        if (!Directory.Exists(diskPath)) return map;

        var exts = new HashSet<string> { ".png", ".jpg", ".jpeg" };
        foreach (var f in Directory.GetFiles(diskPath, "*", SearchOption.AllDirectories))
        {
            if (!exts.Contains(Path.GetExtension(f).ToLower())) continue;
            string key       = Path.GetFileNameWithoutExtension(f).ToLower();
            string assetPath = DiskToAssetPath(f);
            if (!map.ContainsKey(key)) map[key] = assetPath;
        }
        return map;
    }

    static bool TryAssignTex(Material mat, Dictionary<string, string> map,
                              string key, string prop1, string prop2)
    {
        if (!map.TryGetValue(key, out string path)) return false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return false;
        mat.SetTexture(prop1, tex);
        if (prop2 != null) mat.SetTexture(prop2, tex);
        return true;
    }

    static void ForceImportFolder(string assetFolder)
    {
        string diskPath = AssetToDiskPath(assetFolder);
        if (!Directory.Exists(diskPath)) return;

        foreach (var f in Directory.GetFiles(diskPath, "*", SearchOption.AllDirectories))
        {
            if (f.EndsWith(".meta")) continue;
            string rel = DiskToAssetPath(f);
            AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
#endif
