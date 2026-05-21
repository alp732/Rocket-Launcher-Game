using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Check grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Queue jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jumpRequested = true;
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    /// <summary>
    /// Apply an explosion force from a world position.
    /// </summary>
    public void ApplyExplosionForce(Vector2 explosionPos, float radius, float force)
    {
        Vector2 dir = (Vector2)transform.position - explosionPos;
        float distance = dir.magnitude;

        if (distance > radius) return;

        // Falloff: closer = more force
        float falloff = 1f - Mathf.Clamp01(distance / radius);
        Vector2 pushDir = dir.normalized;

        // Always push at least slightly upward so player gets air
        pushDir.y = Mathf.Max(pushDir.y, 0.3f);
        pushDir = pushDir.normalized;

        rb.AddForce(pushDir * force * falloff, ForceMode2D.Impulse);
    }
}
