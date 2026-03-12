#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class NeuralStrikeSceneBuilder
{
    [MenuItem("Tools/Neural Strike/Build Scene")]
    public static void BuildScene()
    {
        if (!EditorUtility.DisplayDialog("Build Neural Strike Scene",
            "This will build the full scene in the CURRENT open scene.\n" +
            "If a Player already exists it will be replaced.\n\nContinue?",
            "Build It", "Cancel")) return;

        // ── Destroy old Player if rebuilding ────────────────────
        var oldPlayer = GameObject.Find("Player");
        if (oldPlayer != null) Object.DestroyImmediate(oldPlayer);
        var oldHUD = GameObject.Find("HUD");
        if (oldHUD != null) Object.DestroyImmediate(oldHUD);

        // ── LAYERS ──────────────────────────────────────────────
        EnsureLayer("Ground");
        EnsureLayer("Player");
        EnsureLayer("Enemy");
        int groundLayer = LayerMask.NameToLayer("Ground");
        int playerLayer = LayerMask.NameToLayer("Player");

        // ── FLOOR ───────────────────────────────────────────────
        GameObject floor = GameObject.Find("Floor")
                        ?? GameObject.Find("Ground")
                        ?? GameObject.Find("Plane");
        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position   = Vector3.zero;
            floor.transform.localScale = new Vector3(10, 1, 10);
        }
        floor.layer = groundLayer;

        // ── PLAYER ──────────────────────────────────────────────
        GameObject player = new GameObject("Player");
        player.layer = playerLayer;
        player.transform.position = new Vector3(0, 1, 0);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0.9f, 0);

        Health playerHealth = player.AddComponent<Health>();
        playerHealth.maxHealth = 100f;
        playerHealth.team = "Team1";

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.walkSpeed            = 5f;
        pc.sprintSpeed          = 8f;
        pc.jumpHeight           = 1.5f;
        pc.gravity              = -20f;
        pc.airControlMultiplier = 0.5f;
        pc.groundCheckRadius    = 0.3f;
        pc.groundMask           = LayerMask.GetMask("Ground");

        // GroundCheck child
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, 0.05f, 0);
        pc.groundCheck = groundCheck.transform;

        // WeaponHolder child
        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(player.transform);
        weaponHolder.transform.localPosition = new Vector3(0.3f, 1.4f, 0.6f);
        weaponHolder.transform.localRotation = Quaternion.identity;

        PlayerWeaponController pwc = player.AddComponent<PlayerWeaponController>();
        pwc.weaponHolder = weaponHolder.transform;
        pwc.reloadKey    = KeyCode.R;
        pwc.semiAuto     = false;

        // ── CAMERA ──────────────────────────────────────────────
        // Remove stale Main Camera
        Camera existingCam = Camera.main;
        if (existingCam != null) Object.DestroyImmediate(existingCam.gameObject);

        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        camObj.transform.localRotation = Quaternion.identity;
        camObj.tag = "MainCamera";

        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView   = 75f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane  = 500f;

        camObj.AddComponent<AudioListener>();

        MouseLook ml = camObj.AddComponent<MouseLook>();
        ml.playerBody       = player.transform;
        ml.mouseSensitivity = 120f;
        ml.lockCursor       = true;

        // ── SPAWN POINTS ────────────────────────────────────────
        CreateTag("SpawnPlayer");
        CreateTag("SpawnBot");

        if (GameObject.Find("SpawnPoint_Player") == null)
        {
            var sp = new GameObject("SpawnPoint_Player");
            sp.transform.position = new Vector3(0, 1, -15);
            sp.tag = "SpawnPlayer";
        }
        for (int i = 0; i < 4; i++)
        {
            if (GameObject.Find($"SpawnPoint_Bot_{i + 1}") != null) continue;
            var bs = new GameObject($"SpawnPoint_Bot_{i + 1}");
            bs.transform.position = new Vector3(
                Mathf.Cos(i * Mathf.PI / 2f) * 15f, 1f,
                Mathf.Sin(i * Mathf.PI / 2f) * 15f);
            bs.tag = "SpawnBot";
        }

        // ── HUD ─────────────────────────────────────────────────
        GameObject canvasObj = new GameObject("HUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Health BG
        GameObject healthBG = new GameObject("HealthBG");
        healthBG.transform.SetParent(canvasObj.transform, false);
        Image bgImg = healthBG.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        var bgRect = healthBG.GetComponent<RectTransform>();
        bgRect.anchorMin = bgRect.anchorMax = bgRect.pivot = new Vector2(0, 0);
        bgRect.anchoredPosition = new Vector2(20, 20);
        bgRect.sizeDelta = new Vector2(250, 22);

        // Health Fill
        GameObject healthFillGO = new GameObject("HealthFill");
        healthFillGO.transform.SetParent(healthBG.transform, false);
        Image fillImg = healthFillGO.AddComponent<Image>();
        fillImg.color      = Color.red;
        fillImg.type       = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        var fillRect = healthFillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;

        // Crosshair
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvasObj.transform, false);
        Image xImg = crosshair.AddComponent<Image>();
        xImg.color = new Color(1, 1, 1, 0.8f);
        var xRect = crosshair.GetComponent<RectTransform>();
        xRect.anchorMin = xRect.anchorMax = xRect.pivot = new Vector2(0.5f, 0.5f);
        xRect.anchoredPosition = Vector2.zero;
        xRect.sizeDelta = new Vector2(12, 12);

        // Ammo Text
        GameObject ammoGO = new GameObject("AmmoText");
        ammoGO.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI ammoTxt = ammoGO.AddComponent<TextMeshProUGUI>();
        ammoTxt.text      = "30 / 90";
        ammoTxt.fontSize  = 28;
        ammoTxt.alignment = TextAlignmentOptions.Right;
        ammoTxt.color     = Color.white;
        var ammoRect = ammoGO.GetComponent<RectTransform>();
        ammoRect.anchorMin = ammoRect.anchorMax = ammoRect.pivot = new Vector2(1, 0);
        ammoRect.anchoredPosition = new Vector2(-20, 20);
        ammoRect.sizeDelta = new Vector2(200, 50);

        // HUDManager — direct reference, fully wired
        HUDManager hud = canvasObj.AddComponent<HUDManager>();
        hud.healthFill       = fillImg;
        hud.playerHealth     = playerHealth;
        hud.ammoText         = ammoTxt;
        hud.weaponController = pwc;

        // ── EventSystem ─────────────────────────────────────────
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── DONE ────────────────────────────────────────────────
        EditorUtility.SetDirty(canvasObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = player;

        EditorUtility.DisplayDialog("Done ✅",
            "Scene built!\n\n" +
            "Controls:\n" +
            "WASD       - Move\n" +
            "Mouse      - Look\n" +
            "Space      - Jump\n" +
            "Left Shift - Sprint\n" +
            "LMB        - Fire\n" +
            "R          - Reload\n" +
            "Escape     - Unlock cursor\n\n" +
            "Next step: create weapon prefab and assign to\n" +
            "Player > PlayerWeaponController > Current Weapon",
            "Let's Go!");
    }

    // ── HELPERS ─────────────────────────────────────────────────
    static void EnsureLayer(string name)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == name) return;
        for (int i = 8; i < layers.arraySize; i++)
        {
            var el = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(el.stringValue)) continue;
            el.stringValue = name;
            tagManager.ApplyModifiedProperties();
            return;
        }
        Debug.LogWarning($"[NeuralStrike] No free layer slot for '{name}' — add manually.");
    }

    static void CreateTag(string tag)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
#endif
