using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform buttonContainer;
    public GameObject levelButtonPrefab;

    void Start()
    {
        BuildLevelButtons();
    }

    void BuildLevelButtons()
    {
        // Clear old buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < 10; i++)
        {
            int levelIndex = i; // capture for lambda
            GameObject btn = Instantiate(levelButtonPrefab, buttonContainer);

            // Set button label
            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = "Level " + (levelIndex + 1);

            // Wire up click
            Button button = btn.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => GameManager.Instance.LoadLevel(levelIndex));
        }
    }
}
