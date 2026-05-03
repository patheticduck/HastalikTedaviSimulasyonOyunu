using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    private GameObject previousPanel;

    [Header("References")]
    public GameObject settingsPanel;
    public GameObject pausePanel;
    public GameObject mainMenuPanel;
    public GameObject mainMenuContentBox;

    private RectTransform settingsRect;
    private Image settingsPanelImage;

    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector2 originalAnchoredPosition;
    private Vector2 originalSizeDelta;
    private Vector3 originalScale;
    private int originalSiblingIndex;

    private readonly Color pauseBackgroundColor = new Color(0f, 0f, 0f, 150f / 255f);
    private readonly Color mainMenuBackgroundColor = new Color(0f, 0f, 0f, 0f);

    private void Awake()
    {
        if (settingsPanel != null)
        {
            settingsRect = settingsPanel.GetComponent<RectTransform>();
            settingsPanelImage = settingsPanel.GetComponent<Image>();

            originalSiblingIndex = settingsPanel.transform.GetSiblingIndex();

            if (settingsRect != null)
            {
                originalAnchorMin = settingsRect.anchorMin;
                originalAnchorMax = settingsRect.anchorMax;
                originalPivot = settingsRect.pivot;
                originalAnchoredPosition = settingsRect.anchoredPosition;
                originalSizeDelta = settingsRect.sizeDelta;
                originalScale = settingsRect.localScale;
            }

            settingsPanel.SetActive(false);
        }
    }

    public void OpenFromPause()
    {
        previousPanel = pausePanel;

        RestoreOriginalTransform();
        SetPanelBackground(pauseBackgroundColor);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }

    public void OpenFromMainMenu()
    {
        previousPanel = mainMenuPanel;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        ApplyMainMenuContentBoxTransform();
        SetPanelBackground(mainMenuBackgroundColor);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (previousPanel == pausePanel)
        {
            RestoreOriginalTransform();

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                pausePanel.transform.SetAsLastSibling();
            }
        }
        else if (previousPanel == mainMenuPanel)
        {
            RestoreOriginalTransform();

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void SetPanelBackground(Color color)
    {
        if (settingsPanelImage == null && settingsPanel != null)
            settingsPanelImage = settingsPanel.GetComponent<Image>();

        if (settingsPanelImage == null)
            return;

        settingsPanelImage.enabled = true;
        settingsPanelImage.color = color;
    }

    private void ApplyMainMenuContentBoxTransform()
    {
        if (settingsRect == null && settingsPanel != null)
            settingsRect = settingsPanel.GetComponent<RectTransform>();

        if (settingsRect == null || mainMenuContentBox == null)
            return;

        RectTransform contentRect = mainMenuContentBox.GetComponent<RectTransform>();

        if (contentRect == null)
            return;

        settingsRect.anchorMin = contentRect.anchorMin;
        settingsRect.anchorMax = contentRect.anchorMax;
        settingsRect.pivot = contentRect.pivot;
        settingsRect.anchoredPosition = contentRect.anchoredPosition;
        settingsRect.sizeDelta = contentRect.sizeDelta;
        settingsRect.localScale = contentRect.localScale;

        settingsPanel.transform.SetSiblingIndex(originalSiblingIndex);
    }

    private void RestoreOriginalTransform()
    {
        if (settingsRect == null && settingsPanel != null)
            settingsRect = settingsPanel.GetComponent<RectTransform>();

        if (settingsRect == null)
            return;

        settingsRect.anchorMin = originalAnchorMin;
        settingsRect.anchorMax = originalAnchorMax;
        settingsRect.pivot = originalPivot;
        settingsRect.anchoredPosition = originalAnchoredPosition;
        settingsRect.sizeDelta = originalSizeDelta;
        settingsRect.localScale = originalScale;

        if (settingsPanel != null)
            settingsPanel.transform.SetSiblingIndex(originalSiblingIndex);
    }
}