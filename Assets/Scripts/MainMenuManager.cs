using UnityEngine;
using Transform = UnityEngine.Transform;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public GameObject historySection;

    [Header("History UI")]
    public Transform historyContent;
    public HistoryItemUI historyItemPrefab;

    [Header("Game Panels")]
    public GameObject dialoguePanel;
    public GameObject endDayPanel;
    public GameObject dayTransitionPanel;
    public GameObject heartsPanel;

    [Header("Manager")]
    public GameFlowManager gameFlowManager;
    public RunHistoryManager runHistoryManager;

    [Header("Player Look")]
    public MouseLookLimited mouseLook;
    public Transform doctorLookRoot;
    public Transform menuCameraPoint;
    public Transform gameplayCameraPoint;

    private bool hasPlayedGame = false;

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

        UpdateHistorySection();
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

        if (runHistoryManager != null && gameFlowManager != null)
        {
            SessionHistoryData session = gameFlowManager.BuildSessionHistoryData();
            runHistoryManager.AddSession(session);
        }

        ShowMainMenu();
    }

    private void MoveCameraRootTo(Transform targetPoint)
    {
        if (doctorLookRoot == null || targetPoint == null)
            return;

        doctorLookRoot.position = targetPoint.position;
        doctorLookRoot.rotation = targetPoint.rotation;
    }

    private void UpdateHistorySection()
    {
        if (historySection == null)
            return;

        historySection.SetActive(hasPlayedGame);

        if (hasPlayedGame)
            RebuildHistoryUI();
    }

    private void RebuildHistoryUI()
    {
        if (historyContent == null || historyItemPrefab == null || runHistoryManager == null)
            return;

        for (int i = historyContent.childCount - 1; i >= 0; i--)
        {
            Destroy(historyContent.GetChild(i).gameObject);
        }

        foreach (var session in runHistoryManager.SessionHistoryList)
        {
            HistoryItemUI item = Instantiate(historyItemPrefab, historyContent);
            item.Setup(session);
        }
    }
}