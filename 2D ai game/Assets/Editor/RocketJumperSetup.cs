
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RocketJumperSetup : EditorWindow
{
    [MenuItem("RocketJumper/Build Entire Project")]
    public static void BuildGame()
    {
        CreateFolders();

        CreatePlatformPrefab();
        CreatePlayerPrefab();
        CreateRocketPrefab();
        CreateLauncherPrefab();
        CreateButtonPrefab();

        CreateLevels();
        CreateMenuScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Done",
            "Rocket Jumper was built successfully.",
            "OK"
        );
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    static Sprite CreateSprite()
    {
        Texture2D tex = Texture2D.whiteTexture;

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    static void CreatePlatformPrefab()
    {
        GameObject obj = new GameObject("Platform");

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = Color.blue;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();

        obj.transform.localScale = new Vector3(4, 0.5f, 1);

        PrefabUtility.SaveAsPrefabAsset(
            obj,
            "Assets/Prefabs/Platform.prefab"
        );

        Object.DestroyImmediate(obj);
    }

    static void CreatePlayerPrefab()
    {
        GameObject player = new GameObject("Player");

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = Color.green;

        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        player.transform.localScale = new Vector3(1, 2, 1);

        player.AddComponent<PlayerController>();

        PrefabUtility.SaveAsPrefabAsset(
            player,
            "Assets/Prefabs/Player.prefab"
        );

        Object.DestroyImmediate(player);
    }

    static void CreateRocketPrefab()
    {
        GameObject rocket = new GameObject("Rocket");

        SpriteRenderer sr = rocket.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = Color.red;

        Rigidbody2D rb = rocket.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        BoxCollider2D col = rocket.AddComponent<BoxCollider2D>();

        rocket.transform.localScale = new Vector3(0.5f, 0.2f, 1);

        rocket.AddComponent<Rocket>();

        PrefabUtility.SaveAsPrefabAsset(
            rocket,
            "Assets/Prefabs/Rocket.prefab"
        );

        Object.DestroyImmediate(rocket);
    }

    static void CreateLauncherPrefab()
    {
        GameObject launcher = new GameObject("Launcher");

        SpriteRenderer sr = launcher.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSprite();
        sr.color = Color.gray;

        launcher.transform.localScale = new Vector3(1.5f, 0.3f, 1);

        RocketLauncher rl = launcher.AddComponent<RocketLauncher>();

        rl.rocketPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Rocket.prefab"
        );

        PrefabUtility.SaveAsPrefabAsset(
            launcher,
            "Assets/Prefabs/Launcher.prefab"
        );

        Object.DestroyImmediate(launcher);
    }

    static void CreateButtonPrefab()
    {
        GameObject btn = new GameObject("LevelButton");

        RectTransform rt = btn.AddComponent<RectTransform>();

        Image img = btn.AddComponent<Image>();
        img.color = Color.cyan;

        btn.AddComponent<Button>();

        GameObject text = new GameObject("Text");
        text.transform.SetParent(btn.transform);

        RectTransform trt = text.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = text.AddComponent<TextMeshProUGUI>();
        tmp.text = "LEVEL";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.black;

        PrefabUtility.SaveAsPrefabAsset(
            btn,
            "Assets/Prefabs/LevelButton.prefab"
        );

        Object.DestroyImmediate(btn);
    }

    static void CreateLevels()
    {
        for (int i = 0; i < 10; i++)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            scene.name = "Level_" + i;

            CreateCamera();

            GameObject player =
                PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/Prefabs/Player.prefab"
                    )
                ) as GameObject;

            player.transform.position = Vector3.zero;

            GameObject launcher =
                PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/Prefabs/Launcher.prefab"
                    )
                ) as GameObject;

            launcher.transform.SetParent(player.transform);

            for (int p = 0; p < 10; p++)
            {
                GameObject plat =
                    PrefabUtility.InstantiatePrefab(
                        AssetDatabase.LoadAssetAtPath<GameObject>(
                            "Assets/Prefabs/Platform.prefab"
                        )
                    ) as GameObject;

                plat.transform.position =
                    new Vector3(
                        Random.Range(-8, 8),
                        p * 3,
                        0
                    );
            }

            EditorSceneManager.SaveScene(
                scene,
                "Assets/Scenes/Level_" + i + ".unity"
            );
        }
    }

    static void CreateMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single
        );

        scene.name = "LevelSelect";

        CreateCamera();

        GameObject canvas = new GameObject("Canvas");

        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        for (int i = 0; i < 10; i++)
        {
            GameObject btn =
                PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/Prefabs/LevelButton.prefab"
                    )
                ) as GameObject;

            btn.transform.SetParent(canvas.transform);

            RectTransform rt =
                btn.GetComponent<RectTransform>();

            rt.sizeDelta = new Vector2(200, 80);

            rt.anchoredPosition =
                new Vector2(
                    (i % 2) * 250 - 125,
                    -(i / 2) * 100
                );

            TextMeshProUGUI txt =
                btn.GetComponentInChildren<TextMeshProUGUI>();

            txt.text = "Level " + (i + 1);

            int index = i;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                EditorSceneManager.OpenScene(
                    "Assets/Scenes/Level_" + index + ".unity"
                );
            });
        }

        EditorSceneManager.SaveScene(
            scene,
            "Assets/Scenes/LevelSelect.unity"
        );
    }

    static void CreateCamera()
    {
        GameObject cam = new GameObject("Main Camera");

        Camera c = cam.AddComponent<Camera>();

        c.orthographic = true;
        c.orthographicSize = 6;

        cam.tag = "MainCamera";

        cam.transform.position =
            new Vector3(0, 0, -10);
    }
}

#endif

