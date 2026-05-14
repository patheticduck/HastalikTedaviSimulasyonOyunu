using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair")]
    public GameObject crosshair;

    [Header("Panels")]
    public GameObject MainMenuPanel;
    public GameObject DialoguePanel;
    public GameObject NotebookPanel;
    public GameObject PatientResultPanel;
    public GameObject EndDayPanel;
    public GameObject DayTransitionPanel;
    public GameObject GameOverPanel;
    public GameObject AIAnalysisPanel;

    void Update()
    {
        bool shouldHide =
            IsOpen(MainMenuPanel) ||
            IsOpen(DialoguePanel) ||
            IsOpen(NotebookPanel) ||
            IsOpen(PatientResultPanel) ||
            IsOpen(EndDayPanel) ||
            IsOpen(DayTransitionPanel) ||
            IsOpen(GameOverPanel) ||
            IsOpen(AIAnalysisPanel);

        if (crosshair != null)
            crosshair.SetActive(!shouldHide);
    }

    bool IsOpen(GameObject panel)
    {
        return panel != null && panel.activeInHierarchy;
    }
}