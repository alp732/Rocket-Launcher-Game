using UnityEngine;

/// <summary>
/// Spawns small fading dots behind the rocket to simulate a smoke trail.
/// Attach to the Rocket prefab.
/// </summary>
public class RocketTrail : MonoBehaviour
{
    public float spawnRate = 0.03f;
    public float trailLifetime = 0.4f;
    public float trailSize = 0.18f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            timer = 0f;
            SpawnDot();
        }
    }

    void SpawnDot()
    {
        GameObject dot = new GameObject("TrailDot");
        dot.transform.position = transform.position;

        SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color = new Color(1f, 0.7f, 0.2f, 0.8f);
        dot.transform.localScale = Vector3.one * trailSize;

        dot.AddComponent<TrailDot>().lifetime = trailLifetime;
    }
}

/// <summary>
/// Fades and shrinks a trail dot then destroys it.
/// </summary>
public class TrailDot : MonoBehaviour
{
    public float lifetime = 0.4f;
    private float age;
    private SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        age += Time.deltaTime;
        float t = age / lifetime;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.8f, 0f, t);
            sr.color = c;
        }
        transform.localScale = Vector3.one * Mathf.Lerp(0.18f, 0f, t);
        if (age >= lifetime) Destroy(gameObject);
    }
}
