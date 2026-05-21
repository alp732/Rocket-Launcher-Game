using UnityEngine;

/// <summary>
/// Pulses the goal's glow ring to make it visually obvious.
/// Attach to the Pulse child GameObject inside the Goal prefab.
/// </summary>
public class GoalPulse : MonoBehaviour
{
    public float speed = 2f;
    public float minScale = 1.2f;
    public float maxScale = 2.0f;

    private float time;

    void Update()
    {
        time += Time.deltaTime * speed;
        float t = (Mathf.Sin(time) + 1f) / 2f; // 0..1
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = Vector3.one * s;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(0.05f, 0.35f, 1f - t);
            sr.color = c;
        }
    }
}
