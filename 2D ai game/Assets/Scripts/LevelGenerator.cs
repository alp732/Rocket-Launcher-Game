using UnityEngine;

/// <summary>
/// Generates deterministic, unique floating platforms for each level using a seeded RNG.
/// Each level always looks the same because the seed is fixed per level index.
/// Attach to an empty GameObject in each Level_X scene and assign prefab references.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject platformPrefab;
    public GameObject goalPrefab;
    public GameObject playerPrefab;
    public GameObject rocketLauncherPrefab;

    [Header("Level Index (0-9)")]
    public int levelIndex = 0;

    // Per-level layout configs
    private static readonly LevelConfig[] Configs = new LevelConfig[]
    {
        new LevelConfig(6,  -4f, 4f,  1.8f, 1f,   2f,  4f),   // Level 1
        new LevelConfig(7,  -5f, 5f,  2.2f, 1f,   1.5f,3f),   // Level 2
        new LevelConfig(10, -5f, 5f,  1.6f, 1f,   1f,  2.5f), // Level 3
        new LevelConfig(8,  -4f, 4f,  2.0f, 0.5f, 1f,  5f),   // Level 4
        new LevelConfig(7,  -5f, 5f,  2.8f, 1f,   1f,  2f),   // Level 5
        new LevelConfig(9,  -5f, 5f,  1.9f, 0.5f, 1.5f,3f),   // Level 6
        new LevelConfig(12, -5f, 5f,  1.4f, 0.5f, 1f,  2f),   // Level 7
        new LevelConfig(6,  -5f, 5f,  3.2f, 1.5f, 1f,  2.5f), // Level 8
        new LevelConfig(10, -5f, 5f,  2.0f, 0.5f, 1f,  4f),   // Level 9
        new LevelConfig(14, -4f, 4f,  2.0f, 0.5f, 1f,  3f),   // Level 10
    };

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // Fixed seed per level = same layout every time
        Random.InitState(levelIndex * 1337 + 42);

        LevelConfig cfg = Configs[Mathf.Clamp(levelIndex, 0, Configs.Length - 1)];

        float y = cfg.yStart;
        float lastX = 0f;

        // Ground / spawn floor
        SpawnPlatform(new Vector2(0f, 0f), 10f);

        // Player
        GameObject player = null;
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab, new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            player.tag = "Player";
        }

        // Rocket launcher (follows player via RocketLauncher script)
        if (rocketLauncherPrefab != null && player != null)
        {
            GameObject launcher = Instantiate(rocketLauncherPrefab, player.transform.position, Quaternion.identity);
            RocketLauncher rl = launcher.GetComponent<RocketLauncher>();
            if (rl != null)
                rl.player = player.transform;
        }

        // Platforms
        for (int i = 0; i < cfg.platformCount; i++)
        {
            y += cfg.yStep + Random.Range(-0.3f, 0.4f);

            // Alternate sides more aggressively to make levels interesting
            float x;
            if (i % 2 == 0)
                x = Random.Range(cfg.xMin, cfg.xMin + (cfg.xMax - cfg.xMin) * 0.5f);
            else
                x = Random.Range(cfg.xMin + (cfg.xMax - cfg.xMin) * 0.5f, cfg.xMax);

            float width = Random.Range(cfg.widthMin, cfg.widthMax);
            SpawnPlatform(new Vector2(x, y), width);
            lastX = x;
        }

        // Goal on last platform
        if (goalPrefab != null)
            Instantiate(goalPrefab, new Vector3(lastX, y + 0.8f, 0f), Quaternion.identity);

        // Wire camera
        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null && player != null)
            cam.target = player.transform;

        // Add ScreenShake to camera if missing
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<ScreenShake>() == null)
            mainCam.gameObject.AddComponent<ScreenShake>();
    }

    void SpawnPlatform(Vector2 pos, float width)
    {
        if (platformPrefab == null) return;
        GameObject p = Instantiate(platformPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        Vector3 s = p.transform.localScale;
        s.x = width;
        p.transform.localScale = s;
    }

    private struct LevelConfig
    {
        public int platformCount;
        public float xMin, xMax, yStep, yStart, widthMin, widthMax;
        public LevelConfig(int c, float x0, float x1, float ys, float yStart, float wMin, float wMax)
        { platformCount=c; xMin=x0; xMax=x1; yStep=ys; this.yStart=yStart; widthMin=wMin; widthMax=wMax; }
    }
}
