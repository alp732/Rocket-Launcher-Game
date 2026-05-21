using UnityEngine;
using System.Collections;

/// <summary>
/// Add to Main Camera. Call ScreenShake.Instance.Shake() from Rocket.cs on explosion.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Shake the camera. duration in seconds, magnitude in world units.
    /// </summary>
    public void Shake(float duration = 0.2f, float magnitude = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        CameraFollow cf = GetComponent<CameraFollow>();
        Vector3 originalOffset = cf != null ? cf.offset : transform.localPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            Vector3 shake = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );
            if (cf != null)
                cf.offset = originalOffset + shake;
            else
                transform.localPosition = originalOffset + shake;

            yield return null;
        }

        if (cf != null)
            cf.offset = originalOffset;
        else
            transform.localPosition = originalOffset;
    }
}
