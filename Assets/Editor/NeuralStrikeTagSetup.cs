#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools > Neural Strike > 1) Setup Required Tags
/// Adds SpawnPlayer, SpawnBot tags to the project if missing.
/// Run this BEFORE Build FFA Scene.
/// </summary>
public static class NeuralStrikeTagSetup
{
    static readonly string[] RequiredTags = { "SpawnPlayer", "SpawnBot" };

    [MenuItem("Tools/Neural Strike/1) Setup Required Tags")]
    public static void SetupTags()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        int added = 0;

        foreach (string tag in RequiredTags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                { found = true; break; }
            }

            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                added++;
                Debug.Log("[NeuralStrike] Added tag: " + tag);
            }
        }

        tagManager.ApplyModifiedProperties();

        EditorUtility.DisplayDialog(
            "Tags Setup Complete",
            added > 0
                ? "Added " + added + " tag(s): SpawnPlayer, SpawnBot.\n\nNow run: Tools > Neural Strike > 2) Build FFA Scene"
                : "All required tags already exist.\n\nRun: Tools > Neural Strike > 2) Build FFA Scene",
            "OK");
    }
}
#endif
