using UnityEngine;

/// <summary>
/// Place a trigger collider far below the level. If the player falls into it, restart the level.
/// </summary>
public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.RestartLevel();
        }
    }
}
