using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI levelLabel;
    public GameObject pausePanel;

    private bool paused = false;

    void Start()
    {
        if (levelLabel != null && GameManager.Instance != null)
            levelLabel.text = "Level " + (GameManager.Instance.GetCurrentLevel() + 1);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (pausePanel != null)
            pausePanel.SetActive(paused);
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        paused = false;
        GameManager.Instance.RestartLevel();
    }

    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        paused = false;
        GameManager.Instance.LoadLevelSelect();
    }
}
