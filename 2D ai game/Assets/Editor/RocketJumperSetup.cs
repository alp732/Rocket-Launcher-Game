// Assets/Editor/RocketJumperSetup.cs
// This is an EDITOR script. Place it in Assets/Editor/ (create that folder if needed).
// After importing all scripts, go to the top menu: RocketJumper → Build Entire Project
// It will create all prefabs, all 11 scenes, and wire everything up automatically.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class RocketJumperSetup : EditorWindow
{
    [MenuItem("RocketJumper/Build Entire Project")]
    public static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("RocketJumper Setup",
            "This will create all prefabs, scenes, and layers. Existing scenes with the same names will be overwritten. Continue?",
            "Yes, Build It!", "Cancel"))
            return;

        // Order matters
        SetupLayers();
        SetupPhysicsMatrix();
        EnsureFolders();
        CreatePlatformPrefab();
        CreateExplosionPrefab();
        CreateRocketPrefab();
        CreateGoalPrefab();
        CreateLevelButtonPrefab();
        CreatePlayerPrefab();
        CreateRocketLauncherPrefab();
        LinkRocketToPrefabs();
        CreateLevelScenes();
        CreateLevelSelectScene();
        AddScenesToBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done!", "RocketJumper project built successfully!\n\nOpen LevelSelect scene and press Play.", "Great!");
    }

    // ─── Folders ───────────────────────────────────────────────
    static void EnsureFolders()
    {
        string[] dirs = { "Assets/Prefabs", "Assets/Scenes", "Assets/Materials", "Assets/Editor" };
        foreach (var d in dirs)
            if (!AssetDatabase.IsValidFolder(d))
            {
                var parts = d.Split('/');
                AssetDatabase.CreateFolder(string.Join("/", parts[..^1]), parts[^1]);
            }
        AssetDatabase.Refresh();
    }

    // ─── Layers ────────────────────────────────────────────────
    static void SetupLayers()
    {
        AddLayer("Ground");
        AddLayer("Rocket");
    }

    static void AddLayer(string name)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty elem = layers.GetArrayElementAtIndex(i);
            if (elem.stringValue == name) return; // already exists
        }
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty elem = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(elem.stringValue))
            {
                elem.stringValue = name;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[RocketJumper] Added layer: {name}");
                return;
            }
        }
        Debug.LogWarning($"[RocketJumper] Could not add layer '{name}' — no free slots.");
    }

    static void SetupPhysicsMatrix()
    {
        // Disable Rocket <-> Player collision
        int playerLayer = LayerMask.NameToLayer("Default"); // Player stays on Default for simplicity
        int rocketLayer = LayerMask.NameToLayer("Rocket");
        if (rocketLayer >= 0)
            Physics2D.IgnoreLayerCollision(rocketLayer, playerLayer, true);
    }

    // ─── Materials ─────────────────────────────────────────────
    static Material GetOrCreateMaterial(string assetPath, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, assetPath);
        }
        return mat;
    }

    // ─── Platform Prefab ───────────────────────────────────────
    static void CreatePlatformPrefab()
    {
        string path = "Assets/Prefabs/Platform.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("Platform");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color = new Color(0.3f, 0.6f, 1f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 0.3f);

        go.transform.localScale = new Vector3(3f, 0.3f, 1f);
        go.layer = LayerMask.NameToLayer("Ground");
        if (go.layer < 0) go.layer = 0;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created Platform prefab");
    }

    // ─── Explosion Effect Prefab ───────────────────────────────
    static void CreateExplosionPrefab()
    {
        string path = "Assets/Prefabs/ExplosionEffect.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("ExplosionEffect");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color = new Color(1f, 0.5f, 0f, 0.9f);
        go.transform.localScale = Vector3.one * 0.3f;
        go.AddComponent<ExplosionEffect>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created ExplosionEffect prefab");
    }

    // ─── Rocket Prefab ─────────────────────────────────────────
    static void CreateRocketPrefab()
    {
        string path = "Assets/Prefabs/Rocket.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("Rocket");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color = new Color(1f, 0.2f, 0.1f);
        go.transform.localScale = new Vector3(0.45f, 0.14f, 1f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D cap = go.AddComponent<CapsuleCollider2D>();
        cap.size = new Vector2(1f, 1f);
        cap.direction = CapsuleDirection2D.Horizontal;

        Rocket rocketScript = go.AddComponent<Rocket>();
        rocketScript.speed = 14f;
        rocketScript.explosionRadius = 2.5f;
        rocketScript.explosionForce = 18f;
        rocketScript.lifetime = 5f;

        int rocketLayer = LayerMask.NameToLayer("Rocket");
        if (rocketLayer >= 0) go.layer = rocketLayer;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created Rocket prefab");
    }

    // ─── Goal Prefab ───────────────────────────────────────────
    static void CreateGoalPrefab()
    {
        string path = "Assets/Prefabs/Goal.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("Goal");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color = new Color(1f, 0.9f, 0f);
        go.transform.localScale = Vector3.one * 0.8f;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        go.AddComponent<LevelGoal>();

        // Pulse animator child
        GameObject pulse = new GameObject("Pulse");
        pulse.transform.SetParent(go.transform);
        pulse.transform.localPosition = Vector3.zero;
        SpriteRenderer psr = pulse.AddComponent<SpriteRenderer>();
        psr.sprite = GetDefaultSprite();
        psr.color = new Color(1f, 1f, 0f, 0.3f);
        pulse.transform.localScale = Vector3.one * 1.5f;
        pulse.AddComponent<GoalPulse>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created Goal prefab");
    }

    // ─── Level Button Prefab ───────────────────────────────────
    static void CreateLevelButtonPrefab()
    {
        string path = "Assets/Prefabs/LevelButtonPrefab.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("LevelButton");
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f);
        Button btn = go.AddComponent<Button>();

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform);
        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Level";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 18;
        tmp.color = Color.white;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created LevelButton prefab");
    }

    // ─── Player Prefab ─────────────────────────────────────────
    static void CreatePlayerPrefab()
    {
        string path = "Assets/Prefabs/Player.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("Player");
        go.tag = "Player";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color = new Color(0.4f, 0.8f, 0.4f);
        go.transform.localScale = new Vector3(0.5f, 1f, 1f);

        CapsuleCollider2D cap = go.AddComponent<CapsuleCollider2D>();
        cap.size = new Vector2(1f, 2f);
        cap.direction = CapsuleDirection2D.Vertical;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        PlayerController pc = go.AddComponent<PlayerController>();
        pc.moveSpeed = 6f;
        pc.jumpForce = 12f;

        // Ground check child
        GameObject gc = new GameObject("GroundCheck");
        gc.transform.SetParent(go.transform);
        gc.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        pc.groundCheck = gc.transform;
        pc.groundCheckRadius = 0.15f;

        // Set ground layer mask
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
            pc.groundLayer = 1 << groundLayer;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created Player prefab");
    }

    // ─── Rocket Launcher Prefab ────────────────────────────────
    static void CreateRocketLauncherPrefab()
    {
        string path = "Assets/Prefabs/RocketLauncher.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        GameObject go = new GameObject("RocketLauncher");
        go.AddComponent<LauncherVisual>();

        RocketLauncher rl = go.AddComponent<RocketLauncher>();
        rl.orbitRadius = 0.9f;
        rl.fireRate = 0.4f;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RocketJumper] Created RocketLauncher prefab");
    }

    // ─── Link Rocket Prefab references ─────────────────────────
    static void LinkRocketToPrefabs()
    {
        // Assign explosion effect to Rocket prefab
        GameObject rocketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rocket.prefab");
        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExplosionEffect.prefab");
        GameObject launcherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RocketLauncher.prefab");

        if (rocketPrefab != null && explosionPrefab != null)
        {
            Rocket r = rocketPrefab.GetComponent<Rocket>();
            if (r != null && r.explosionEffectPrefab == null)
            {
                r.explosionEffectPrefab = explosionPrefab;
                EditorUtility.SetDirty(rocketPrefab);
            }
        }

        if (launcherPrefab != null && rocketPrefab != null)
        {
            RocketLauncher rl = launcherPrefab.GetComponent<RocketLauncher>();
            if (rl != null && rl.rocketPrefab == null)
            {
                rl.rocketPrefab = rocketPrefab;
                EditorUtility.SetDirty(launcherPrefab);
            }
        }

        AssetDatabase.SaveAssets();
    }

    // ─── Level Scenes ──────────────────────────────────────────
    static void CreateLevelScenes()
    {
        for (int i = 0; i < 10; i++)
            BuildLevelScene(i);
    }

    static void BuildLevelScene(int index)
    {
        string scenePath = $"Assets/Scenes/Level_{index}.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = $"Level_{index}";

        // ── Camera ──
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.07f, 0.07f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.transform.position = new Vector3(0, 3, -10);
        camGO.AddComponent<AudioListener>();
        CameraFollow cf = camGO.AddComponent<CameraFollow>();
        cf.smoothSpeed = 5f;
        cf.offset = new Vector3(0, 2, -10);

        // ── Lighting ──
        GameObject lightGO = new GameObject("GlobalLight");
        var light = lightGO.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        light.intensity = 1f;

        // ── LevelGenerator ──
        GameObject genGO = new GameObject("LevelGenerator");
        LevelGenerator gen = genGO.AddComponent<LevelGenerator>();
        gen.levelIndex = index;
        gen.platformPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Platform.prefab");
        gen.goalPrefab     = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Goal.prefab");
        gen.playerPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        gen.rocketLauncherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RocketLauncher.prefab");

        // ── Death Zone ──
        GameObject dzGO = new GameObject("DeathZone");
        dzGO.transform.position = new Vector3(0, -25, 0);
        BoxCollider2D dzCol = dzGO.AddComponent<BoxCollider2D>();
        dzCol.isTrigger = true;
        dzCol.size = new Vector2(200, 4);
        dzGO.AddComponent<DeathZone>();

        // ── HUD Canvas ──
        BuildHUD(scene, index);

        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.CloseScene(scene, true);
        Debug.Log($"[RocketJumper] Built Level_{index} scene");
    }

    static void BuildHUD(Scene scene, int levelIndex)
    {
        GameObject canvasGO = new GameObject("HUD");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        HUDController hud = canvasGO.AddComponent<HUDController>();

        // Level label
        GameObject labelGO = new GameObject("LevelLabel");
        labelGO.transform.SetParent(canvasGO.transform);
        RectTransform lrt = labelGO.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 1f);
        lrt.anchorMax = new Vector2(0f, 1f);
        lrt.pivot = new Vector2(0f, 1f);
        lrt.anchoredPosition = new Vector2(20, -20);
        lrt.sizeDelta = new Vector2(200, 50);
        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = "Level " + (levelIndex + 1);
        labelTMP.fontSize = 24;
        labelTMP.color = Color.white;
        hud.levelLabel = labelTMP;

        // Pause Panel
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasGO.transform);
        RectTransform pprt = pausePanel.AddComponent<RectTransform>();
        pprt.anchorMin = new Vector2(0.3f, 0.3f);
        pprt.anchorMax = new Vector2(0.7f, 0.7f);
        pprt.offsetMin = Vector2.zero;
        pprt.offsetMax = Vector2.zero;
        Image ppImg = pausePanel.AddComponent<Image>();
        ppImg.color = new Color(0, 0, 0, 0.85f);
        pausePanel.SetActive(false);
        hud.pausePanel = pausePanel;

        // Restart button inside pause panel
        AddButton(pausePanel.transform, "RestartButton", "Restart", new Vector2(0f, 30f),
            new Vector2(160, 50), hud, "OnRestartButton");

        // Menu button inside pause panel
        AddButton(pausePanel.transform, "MenuButton", "Main Menu", new Vector2(0f, -30f),
            new Vector2(160, 50), hud, "OnMenuButton");
    }

    static void AddButton(Transform parent, string name, string label, Vector2 anchoredPos,
        Vector2 size, HUDController hud, string methodName)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent);
        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f);
        Button btn = btnGO.AddComponent<Button>();

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(btnGO.transform);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 18;
        tmp.color = Color.white;

        // Wire button click
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction),
                hud, methodName) as UnityEngine.Events.UnityAction);
    }

    // ─── LevelSelect Scene ─────────────────────────────────────
    static void CreateLevelSelectScene()
    {
        string scenePath = "Assets/Scenes/LevelSelect.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = "LevelSelect";

        // Camera
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // Lighting
        GameObject lightGO = new GameObject("GlobalLight");
        var light = lightGO.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        light.intensity = 1f;

        // GameManager (persistent)
        GameObject gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        // Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background panel
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform);
        RectTransform bgrt = bg.AddComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.07f, 0.07f, 0.18f);

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform);
        RectTransform trt = titleGO.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0.85f);
        trt.anchorMax = new Vector2(0.5f, 0.95f);
        trt.sizeDelta = new Vector2(600, 80);
        trt.anchoredPosition = Vector2.zero;
        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "ROCKET JUMPER";
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontSize = 52;
        titleTMP.color = new Color(1f, 0.5f, 0.1f);
        titleTMP.fontStyle = FontStyles.Bold;

        // Subtitle
        GameObject subGO = new GameObject("Subtitle");
        subGO.transform.SetParent(canvasGO.transform);
        RectTransform srt = subGO.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 0.78f);
        srt.anchorMax = new Vector2(0.5f, 0.86f);
        srt.sizeDelta = new Vector2(500, 50);
        srt.anchoredPosition = Vector2.zero;
        TextMeshProUGUI subTMP = subGO.AddComponent<TextMeshProUGUI>();
        subTMP.text = "Select a Level";
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.fontSize = 24;
        subTMP.color = new Color(0.7f, 0.7f, 1f);

        // Button grid container
        GameObject container = new GameObject("ButtonContainer");
        container.transform.SetParent(canvasGO.transform);
        RectTransform crt = container.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.15f, 0.15f);
        crt.anchorMax = new Vector2(0.85f, 0.75f);
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;
        GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160, 70);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // LevelSelectUI component
        GameObject uiMgr = new GameObject("LevelSelectUI");
        LevelSelectUI lsui = uiMgr.AddComponent<LevelSelectUI>();
        lsui.buttonContainer = container.transform;
        lsui.levelButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LevelButtonPrefab.prefab");

        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.CloseScene(scene, true);
        Debug.Log("[RocketJumper] Built LevelSelect scene");
    }

    // ─── Build Settings ────────────────────────────────────────
    static void AddScenesToBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/LevelSelect.unity", true));
        for (int i = 0; i < 10; i++)
            scenes.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level_{i}.unity", true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[RocketJumper] Added all scenes to Build Settings");
    }

    // ─── Utility ───────────────────────────────────────────────
    static Sprite GetDefaultSprite()
    {
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd")
            ?? Resources.GetBuiltinResource<Sprite>("Sprites/Default");
    }
}
#endif
