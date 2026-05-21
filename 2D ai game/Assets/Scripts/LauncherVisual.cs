using UnityEngine;

/// <summary>
/// Procedurally builds a rocket-launcher mesh with:
///  - a rectangular body
///  - a narrower barrel extending to the right
///  - a downward grip tab on the left
/// Attach this to the RocketLauncher prefab root (instead of using a sprite).
/// Assign a material using the Sprites/Default or Unlit/Color shader.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LauncherVisual : MonoBehaviour
{
    [Header("Color")]
    public Color launcherColor = new Color(0.25f, 0.25f, 0.25f);

    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = BuildMesh();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        // Create a material at runtime using the built-in sprite shader
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = launcherColor;
        mr.material = mat;
    }

    Mesh BuildMesh()
    {
        Mesh m = new Mesh();
        m.name = "RocketLauncherMesh";

        // All coords are local — origin is the launcher pivot point
        // The launcher "fires" to the right (+X direction at 0 degrees)

        m.vertices = new Vector3[]
        {
            // --- Body (main rectangular block) ---
            new Vector3(-0.35f,  0.13f, 0f),   // 0 top-left
            new Vector3( 0.20f,  0.13f, 0f),   // 1 top-right
            new Vector3( 0.20f, -0.13f, 0f),   // 2 bottom-right
            new Vector3(-0.35f, -0.13f, 0f),   // 3 bottom-left

            // --- Barrel (narrow rectangle extending right from body) ---
            new Vector3( 0.20f,  0.07f, 0f),   // 4 top-left of barrel
            new Vector3( 0.58f,  0.07f, 0f),   // 5 top-right (muzzle)
            new Vector3( 0.58f, -0.07f, 0f),   // 6 bottom-right (muzzle)
            new Vector3( 0.20f, -0.07f, 0f),   // 7 bottom-left of barrel

            // --- Grip (downward tab on bottom-left of body) ---
            new Vector3(-0.12f, -0.13f, 0f),   // 8 top-left of grip
            new Vector3( 0.08f, -0.13f, 0f),   // 9 top-right of grip
            new Vector3( 0.05f, -0.30f, 0f),   // 10 bottom-right (slight taper)
            new Vector3(-0.10f, -0.30f, 0f),   // 11 bottom-left

            // --- Scope (small raised block on top of body, center) ---
            new Vector3(-0.05f,  0.13f, 0f),   // 12
            new Vector3( 0.12f,  0.13f, 0f),   // 13
            new Vector3( 0.12f,  0.22f, 0f),   // 14
            new Vector3(-0.05f,  0.22f, 0f),   // 15
        };

        m.triangles = new int[]
        {
            // Body
            0, 1, 2,   0, 2, 3,
            // Barrel
            4, 5, 6,   4, 6, 7,
            // Grip
            8, 9, 10,  8, 10, 11,
            // Scope
            12, 13, 14,  12, 14, 15,
        };

        // Basic UVs (not critical for solid-color material)
        m.uv = new Vector2[m.vertices.Length];
        for (int i = 0; i < m.uv.Length; i++)
            m.uv[i] = new Vector2(m.vertices[i].x, m.vertices[i].y);

        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
