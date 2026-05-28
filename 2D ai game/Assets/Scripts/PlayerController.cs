
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool jumpPressed;
    private bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.freezeRotation = true;
        rb.gravityScale = 3f;
    }

    void Update()
    {
        if (groundCheck != null)
        {
            grounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundRadius,
                groundLayer
            );
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        float move = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            move * moveSpeed,
            rb.linearVelocity.y
        );

        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpPressed = false;
        }
    }

    public void ApplyExplosionForce(
        Vector2 explosionPos,
        float radius,
        float force
    )
    {
        Vector2 dir =
            (Vector2)transform.position - explosionPos;

        float dist = dir.magnitude;

        if (dist > radius) return;

        float falloff =
            1f - Mathf.Clamp01(dist / radius);

        Vector2 push = dir.normalized;

        push.y = Mathf.Max(push.y, 0.35f);

        rb.AddForce(
            push.normalized * force * falloff,
            ForceMode2D.Impulse
        );
    }
}

