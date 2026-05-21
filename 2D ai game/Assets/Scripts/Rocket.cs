using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rocket : MonoBehaviour
{
    [Header("Rocket Settings")]
    public float speed = 14f;
    public float explosionRadius = 2.5f;
    public float explosionForce = 18f;
    public float lifetime = 5f;

    [Header("Effects")]
    public GameObject explosionEffectPrefab;

    private Rigidbody2D rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Add trail automatically
        if (GetComponent<RocketTrail>() == null)
            gameObject.AddComponent<RocketTrail>();
    }

    public void SetDirection(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Don't self-explode on trigger zones
        if (col.collider.isTrigger) return;
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // VFX
        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Screen shake
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.Shake(0.22f, 0.18f);

        // Push player
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.ApplyExplosionForce(transform.position, explosionRadius, explosionForce);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
