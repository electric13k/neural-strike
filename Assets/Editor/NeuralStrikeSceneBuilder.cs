#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public static class NeuralStrikeSceneBuilder
{
    [MenuItem("Tools/Neural Strike/Build Scene")]
    public static void BuildScene()
    {
        if (!EditorUtility.DisplayDialog("Build Neural Strike Scene",
            "This will add the full scene setup to your current scene.\nMake sure you have a floor/ground plane already in the scene.\n\nContinue?",
            "Build It", "Cancel"))
            return;

        // ── LAYERS ──────────────────────────────────────────────
        EnsureLayer("Ground");
        EnsureLayer("Player");
        EnsureLayer("Enemy");

        int groundLayer  = LayerMask.NameToLayer("Ground");
        int playerLayer  = LayerMask.NameToLayer("Player");
        int enemyLayer   = LayerMask.NameToLayer("Enemy");

        // ── PLAYER ──────────────────────────────────────────────
        GameObject player = new GameObject("Player");
        player.layer = playerLayer;
        player.transform.position = new Vector3(0, 1, 0);

        // CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0, 0.9f, 0);

        // Health
        Health playerHealth = player.AddComponent<Health>();
        playerHealth.maxHealth = 100f;
        playerHealth.team = "Team1";

        // PlayerController
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.walkSpeed   = 5f;
        pc.sprintSpeed = 8f;
        pc.jumpHeight  = 1.5f;
        pc.gravity     = -20f;
        pc.airControlMultiplier = 0.5f;
        pc.groundCheckRadius    = 0.3f;
        pc.groundMask = LayerMask.GetMask("Ground");

        // GroundCheck
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0, 0.05f, 0);
        pc.groundCheck = groundCheck.transform;

        // WeaponHolder
        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(player.transform);
        weaponHolder.transform.localPosition = new Vector3(0.3f, 1.4f, 0.6f);
        weaponHolder.transform.localRotation = Quaternion.identity;

        // PlayerWeaponController
        PlayerWeaponController pwc = player.AddComponent<PlayerWeaponController>();
        pwc.weaponHolder = weaponHolder.transform;
        pwc.reloadKey    = KeyCode.R;
        pwc.semiAuto     = false;

        // ── CAMERA ──────────────────────────────────────────────
        // Remove existing Main Camera if any
        Camera existingCam = Camera.main;
        if (existingCam != null)
            GameObject.DestroyImmediate(existingCam.gameObject);

        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        camObj.transform.localRotation = Quaternion.identity;

        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView   = 75f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane  = 500f;
        camObj.tag = "MainCamera";

        camObj.AddComponent<AudioListener>();

        MouseLook ml = camObj.AddComponent<MouseLook>();
        ml.playerBody       = player.transform;
        ml.mouseSensitivity = 120f;
        ml.lockCursor       = true;

        // ── FLOOR ───────────────────────────────────────────────
        // Only create if no floor exists
        GameObject floor = GameObject.Find("Floor") ?? GameObject.Find("Ground") ?? GameObject.Find("Plane");
        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(10, 1, 10);
        }
        floor.layer = groundLayer;

        // ── BOT SPAWN POINTS ────────────────────────────────────
        CreateTag("SpawnPlayer");
        CreateTag("SpawnBot");

        GameObject playerSpawn = new GameObject("SpawnPoint_Player");
        playerSpawn.transform.position = new Vector3(0, 1, -15);
        playerSpawn.tag = "SpawnPlayer";

        for (int i = 0; i < 4; i++)
        {
            GameObject botSpawn = new GameObject($"SpawnPoint_Bot_{i + 1}");
            botSpawn.transform.position = new Vector3(
                Mathf.Cos(i * Mathf.PI / 2f) * 15f,
                1f,
                Mathf.Sin(i * Mathf.PI / 2f) * 15f
            );
            botSpawn.tag = "SpawnBot";
        }

        // ── HUD ─────────────────────────────────────────────────
        BuildHUD(playerHealth, pwc);

        // ── DONE ────────────────────────────────────────────────
        Selection.activeGameObject = player;
        EditorUtility.DisplayDialog("Done!",
            "Scene built successfully!\n\n" +
            "✅ Player with CharacterController, Health, MouseLook\n" +
            "✅ PlayerCamera parented to Player\n" +
            "✅ WeaponHolder created\n" +
            "✅ GroundCheck created\n" +
            "✅ Floor set to Ground layer\n" +
            "✅ 4 Bot spawn points\n" +
            "✅ HUD canvas\n\n" +
            "Next: Add weapon prefab as child of WeaponHolder,\n" +
            "then assign it to PlayerWeaponController > Current Weapon.",
            "Let's Go!");
    }

    // ── HUD BUILDER ─────────────────────────────────────────────
    static void BuildHUD(Health playerHealth, PlayerWeaponController pwc)
    {
        // Canvas
        GameObject canvasObj = new GameObject("HUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ── Health Bar background ────────────────────────────────
        GameObject healthBG = new GameObject("HealthBG");
        healthBG.transform.SetParent(canvasObj.transform, false);
        Image bgImg = healthBG.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform bgRect = healthBG.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(0, 0);
        bgRect.pivot     = new Vector2(0, 0);
        bgRect.anchoredPosition = new Vector2(20, 20);
        bgRect.sizeDelta = new Vector2(250, 22);

        // Health fill
        GameObject healthFill = new GameObject("HealthFill");
        healthFill.transform.SetParent(healthBG.transform, false);
        Image fillImg = healthFill.AddComponent<Image>();
        fillImg.color     = new Color(0.8f, 0.1f, 0.1f, 1f);
        fillImg.type      = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        RectTransform fillRect = healthFill.GetComponent<RectTransform>();
        fillRect.anchorMin        = Vector2.zero;
        fillRect.anchorMax        = Vector2.one;
        fillRect.offsetMin        = Vector2.zero;
        fillRect.offsetMax        = Vector2.zero;

        // ── Crosshair ────────────────────────────────────────────
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvasObj.transform, false);
        Image xImg = crosshair.AddComponent<Image>();
        xImg.color = new Color(1, 1, 1, 0.8f);
        RectTransform xRect = crosshair.GetComponent<RectTransform>();
        xRect.anchorMin        = new Vector2(0.5f, 0.5f);
        xRect.anchorMax        = new Vector2(0.5f, 0.5f);
        xRect.pivot            = new Vector2(0.5f, 0.5f);
        xRect.anchoredPosition = Vector2.zero;
        xRect.sizeDelta        = new Vector2(12, 12);

        // ── Ammo Text ────────────────────────────────────────────
        GameObject ammoObj = new GameObject("AmmoText");
        ammoObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI ammoTxt = ammoObj.AddComponent<TextMeshProUGUI>();
        ammoTxt.text      = "30 / 90";
        ammoTxt.fontSize  = 28;
        ammoTxt.alignment = TextAlignmentOptions.Right;
        ammoTxt.color     = Color.white;
        RectTransform ammoRect = ammoObj.GetComponent<RectTransform>();
        ammoRect.anchorMin        = new Vector2(1, 0);
        ammoRect.anchorMax        = new Vector2(1, 0);
        ammoRect.pivot            = new Vector2(1, 0);
        ammoRect.anchoredPosition = new Vector2(-20, 20);
        ammoRect.sizeDelta        = new Vector2(200, 50);

        // ── HUDManager ───────────────────────────────────────────
        // Only add if HUDManager script exists in the project
        System.Type hudType = System.Type.GetType("HUDManager");
        if (hudType != null)
        {
            var hud = canvasObj.AddComponent(hudType);
            var t = hud.GetType();
            t.GetField("healthFill")?.SetValue(hud,    fillImg);
            t.GetField("playerHealth")?.SetValue(hud,  playerHealth);
            t.GetField("ammoText")?.SetValue(hud,      ammoTxt);
            t.GetField("weaponController")?.SetValue(hud, pwc);
        }
    }

    // ── HELPERS ─────────────────────────────────────────────────
    static void EnsureLayer(string name)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty el = layers.GetArrayElementAtIndex(i);
            if (el.stringValue == name) return;   // already exists
        }
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty el = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(el.stringValue))
            {
                el.stringValue = name;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
        Debug.LogWarning($"[NeuralStrike] No empty layer slot for '{name}'. Add it manually.");
    }

    static void CreateTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
#endif
