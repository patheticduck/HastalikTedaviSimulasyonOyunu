using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private GameObject previousPanel;

    public void OpenSettings(GameObject fromPanel)
    {
        previousPanel = fromPanel;

        fromPanel.SetActive(false);
        gameObject.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);

        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }

        Time.timeScale = 1f;
    }
}