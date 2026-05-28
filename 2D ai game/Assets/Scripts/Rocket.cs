
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rocket : MonoBehaviour
{
    [Header("Rocket")]
    public float speed = 15f;

    [Header("Explosion")]
    public float explosionRadius = 3f;
    public float explosionForce = 20f;

    [Header("Lifetime")]
    public float lifetime = 5f;

    private Rigidbody2D rb;
    private bool exploded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;

        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        rb.linearVelocity =
            dir.normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (exploded) return;

        exploded = true;

        PlayerController player =
            FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            player.ApplyExplosionForce(
                transform.position,
                explosionRadius,
                explosionForce
            );
        }

        Destroy(gameObject);
    }
}

