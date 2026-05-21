using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float duration = 0.35f;
    public float maxScale = 2.2f;

    private float timer = 0f;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        // Scale up then fade out
        float scale = Mathf.Lerp(0.1f, maxScale, Mathf.Sin(t * Mathf.PI));
        transform.localScale = Vector3.one * scale;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }

        if (timer >= duration)
            Destroy(gameObject);
    }
}
