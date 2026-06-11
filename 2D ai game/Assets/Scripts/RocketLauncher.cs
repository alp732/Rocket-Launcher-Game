
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject rocketPrefab;

    [Header("Settings")]
    public float orbitRadius = 2f;
    public float fireCooldown = 0.1f;

    private float timer;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (player == null)
        {
            GameObject p = GameObject.Find("Player");

            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        Aim();

        timer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) &&
            timer <= 0f)
        {
            Shoot();
            timer = fireCooldown;
        }
    }

    void Aim()
    {
        Vector3 mouse =
            cam.ScreenToWorldPoint(
                Input.mousePosition
            );

        mouse.z = 0f;

        Vector2 dir =
            (mouse - player.position).normalized;

        transform.position =
            player.position +
            (Vector3)(dir * orbitRadius);

        float angle =
            Mathf.Atan2(dir.y, dir.x) *
            Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        if (rocketPrefab == null) return;

        Vector2 dir = transform.right;

        Vector3 spawnPos =
            transform.position +
            (Vector3)(dir * 0.6f);

        GameObject rocket =
            Instantiate(
                rocketPrefab,
                spawnPos,
                transform.rotation
            );

        Rocket r = rocket.GetComponent<Rocket>();

        if (r != null)
        {
            r.SetDirection(dir);
        }
    }
}

