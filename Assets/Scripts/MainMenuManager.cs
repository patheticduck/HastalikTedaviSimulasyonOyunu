using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public GameObject lastGameInfoPanel;
    public TextMeshProUGUI lastGameDateText;
    public TextMeshProUGUI lastGameStatsText;

    [Header("Game Panels")]
    public GameObject dialoguePanel;
    public GameObject endDayPanel;
    public GameObject dayTransitionPanel;
    public GameObject heartsPanel;

    [Header("Manager")]
    public GameFlowManager gameFlowManager;

    [Header("Player Look")]
    public MouseLookLimited mouseLook;
    public Transform doctorLookRoot;
    public Transform menuCameraPoint;
    public Transform gameplayCameraPoint;

    private bool hasPlayedGame = false;
    private string lastGameDate = "";
    private int lastCorrect = 0;
    private int lastWrong = 0;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        if (heartsPanel != null)
            heartsPanel.SetActive(false);

        MoveCameraRootTo(menuCameraPoint);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        UpdateLastGameUI();
    }

    public void StartGameFromMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        if (heartsPanel != null)
            heartsPanel.SetActive(true);

        MoveCameraRootTo(gameplayCameraPoint);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(true);

        if (gameFlowManager != null)
        {
            gameFlowManager.ResetGame();
            gameFlowManager.StartGameplayFlow();
        }
        else
        {
            Debug.LogWarning("GameFlowManager bağlı değil!");
        }
    }

    public void ReturnToMenu(int correct, int wrong)
    {
        hasPlayedGame = true;
        lastGameDate = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        lastCorrect = correct;
        lastWrong = wrong;

        ShowMainMenu();
    }

    private void MoveCameraRootTo(Transform targetPoint)
    {
        if (doctorLookRoot == null || targetPoint == null)
            return;

        doctorLookRoot.position = targetPoint.position;
        doctorLookRoot.rotation = targetPoint.rotation;
    }

    private void UpdateLastGameUI()
    {
        if (lastGameInfoPanel == null || lastGameDateText == null || lastGameStatsText == null)
            return;

        if (!hasPlayedGame)
        {
            lastGameInfoPanel.SetActive(false);
            lastGameDateText.text = "";
            lastGameStatsText.text = "";
            return;
        }

        lastGameInfoPanel.SetActive(true);

        lastGameDateText.text = "Son oyun: " + lastGameDate;
        lastGameStatsText.text =
            "Toplam Doğru: " + lastCorrect + "\n" +
            "Toplam Yanlış: " + lastWrong;
    }
}