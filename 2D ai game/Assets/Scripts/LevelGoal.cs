using UnityEngine;
using TMPro;

public class LevelGoal : MonoBehaviour
{
    [Header("Optional UI (auto-created if null)")]
    public GameObject winPanel;
    public TextMeshProUGUI levelCompleteText;

    private bool levelWon = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (levelWon) return;
        if (!other.CompareTag("Player")) return;
        levelWon = true;
        WinLevel();
    }

    void WinLevel()
    {
        Time.timeScale = 0.25f;
        if (winPanel == null) BuildWinUI();
        if (winPanel != null) winPanel.SetActive(true);
        if (levelCompleteText != null && GameManager.Instance != null)
            levelCompleteText.text = "Level " + (GameManager.Instance.GetCurrentLevel() + 1) + " Complete!";
        Invoke(nameof(ProceedToNext), 2.0f);
    }

    void ProceedToNext()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.GoToNextLevel();
    }

    void BuildWinUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cGO = new GameObject("WinCanvas");
            canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            cGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(canvas.transform);
        RectTransform rt = winPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0.35f);
        rt.anchorMax = new Vector2(0.8f, 0.65f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        UnityEngine.UI.Image img = winPanel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject textGO = new GameObject("WinText");
        textGO.transform.SetParent(winPanel.transform);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        levelCompleteText = textGO.AddComponent<TextMeshProUGUI>();
        levelCompleteText.text = "Level Complete!";
        levelCompleteText.alignment = TextAlignmentOptions.Center;
        levelCompleteText.fontSize = 36;
        levelCompleteText.color = new Color(1f, 0.9f, 0.1f);
        levelCompleteText.fontStyle = FontStyles.Bold;
    }
}
