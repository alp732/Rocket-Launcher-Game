using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject rocketPrefab;

    [Header("Launcher Settings")]
    public float orbitRadius = 0.9f;   // Distance from player center
    public float fireRate = 0.4f;       // Seconds between shots

    private float fireCooldown = 0f;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        AimAtMouse();

        fireCooldown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = fireRate;
        }
    }

    void AimAtMouse()
    {
        if (player == null) return;

        // Get mouse in world space
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // Direction from player to mouse
        Vector2 dir = (mouseWorld - player.position).normalized;

        // Position launcher offset from player
        transform.position = player.position + (Vector3)(dir * orbitRadius);

        // Rotate launcher to face mouse direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Fire()
    {
        if (rocketPrefab == null) return;

        // Spawn rocket at launcher tip (forward tip = right direction since 0° = right)
        Vector2 launchDir = transform.right;
        Vector3 spawnPos = transform.position + (Vector3)(launchDir * 0.3f);

        GameObject rocket = Instantiate(rocketPrefab, spawnPos, transform.rotation);
        Rocket r = rocket.GetComponent<Rocket>();
        if (r != null)
            r.SetDirection(launchDir);
    }
}
