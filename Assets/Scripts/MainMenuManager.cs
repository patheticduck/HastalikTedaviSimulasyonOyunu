using UnityEngine.UI;
using UnityEngine;
using Transform = UnityEngine.Transform;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public GameObject historySection;

    [Header("Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;

    [Header("History UI")]
    public Transform historyContent;
    public HistoryItemUI historyItemPrefab;

    [Header("History Delete Mode")]
    public Button deleteModeButton;
    public TextMeshProUGUI deleteModeButtonText;

    [Header("Game Panels")]
    public GameObject dialoguePanel;
    public GameObject endDayPanel;
    public GameObject dayTransitionPanel;
    public GameObject heartsPanel;
    public GameObject notebookPanel;
    public GameObject doctorBookPanel;
    public GameObject energyConfirmPanel;

    [Header("Manager")]
    public GameFlowManager gameFlowManager;
    public RunHistoryManager runHistoryManager;

    [Header("Player Look")]
    public MouseLookLimited mouseLook;
    public Transform doctorLookRoot;
    public Transform menuCameraPoint;
    public Transform gameplayCameraPoint;

    private ScrollRect historyScrollRect;
    private bool isHistoryDeleteMode = false;

    private void Awake()
    {
        if (historyContent != null)
            historyScrollRect = historyContent.GetComponentInParent<ScrollRect>();
    }

    private void Start()
    {
        if (deleteModeButton != null)
        {
            deleteModeButton.onClick.RemoveAllListeners();
            deleteModeButton.onClick.AddListener(ToggleHistoryDeleteMode);
        }

        UpdateDeleteModeButtonText();
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
        
        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);
        
        if (energyConfirmPanel != null)
            energyConfirmPanel.SetActive(false);

        MoveCameraRootTo(menuCameraPoint);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        bool canContinue =
            RuntimeSaveManager.Instance != null &&
            RuntimeSaveManager.Instance.currentSave != null &&
            RuntimeSaveManager.Instance.currentSave.hasSave;

        UpdateContinueButtonState(canContinue);

        UpdateHistorySection();

        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
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

        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.ClearSave();

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

    public void ContinueGameFromMenu()
    {
        if (RuntimeSaveManager.Instance == null ||
            RuntimeSaveManager.Instance.currentSave == null ||
            !RuntimeSaveManager.Instance.currentSave.hasSave)
        {
            Debug.LogWarning("Devam edilecek kayıt bulunamadı.");
            return;
        }

        RuntimeSaveData save = RuntimeSaveManager.Instance.currentSave;

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
            gameFlowManager.ApplyRuntimeSaveData(save);
            gameFlowManager.RefreshGameUIAfterLoad();

            if (save.diagnosisSubmittedForCurrentPatient)
                gameFlowManager.ContinueAfterLoadedSave();
            else
                gameFlowManager.StartGameplayFlow();
        }
        else
        {
            Debug.LogWarning("GameFlowManager bağlı değil!");
        }
    }

    public void ReturnToMenu(int correct, int wrong)
    {
        if (runHistoryManager != null && gameFlowManager != null)
        {
            SessionHistoryData session = gameFlowManager.BuildSessionHistoryData();

            if (session != null && session.days != null && session.days.Count > 0)
            {
                runHistoryManager.AddSession(session);
                Debug.Log("History eklendi. Toplam session sayısı: " + runHistoryManager.SessionHistoryList.Count);
            }
            else
            {
                Debug.LogWarning("History session boş geldi, history'ye eklenmedi.");
            }
        }
        else
        {
            Debug.LogWarning("ReturnToMenu -> runHistoryManager veya gameFlowManager null.");
        }

        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.ClearSave();

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

        bool hasAnyHistory =
            runHistoryManager != null &&
            runHistoryManager.SessionHistoryList != null &&
            runHistoryManager.SessionHistoryList.Count > 0;

        historySection.SetActive(hasAnyHistory);

        if (!hasAnyHistory)
        {
            isHistoryDeleteMode = false;
            UpdateDeleteModeButtonText();
        }

        if (deleteModeButton != null)
            deleteModeButton.gameObject.SetActive(hasAnyHistory);

        Debug.Log("History görünür mü? " + hasAnyHistory);

        if (hasAnyHistory)
            RebuildHistoryUI();
    }

    private void RebuildHistoryUI()
    {
        if (historyContent == null || historyItemPrefab == null || runHistoryManager == null)
        {
            Debug.LogWarning("RebuildHistoryUI -> historyContent / historyItemPrefab / runHistoryManager eksik.");
            return;
        }

        for (int i = historyContent.childCount - 1; i >= 0; i--)
        {
            Transform child = historyContent.GetChild(i);
            if (child.GetComponent<HistoryItemUI>() != null)
                Destroy(child.gameObject);
        }

        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < runHistoryManager.SessionHistoryList.Count; i++)
        {
            SessionHistoryData session = runHistoryManager.SessionHistoryList[i];
            if (session == null)
                continue;

            HistoryItemUI item = Instantiate(historyItemPrefab, historyContent, false);
            item.gameObject.SetActive(true);

            item.SetHistoryContentRect(historyContent as RectTransform);
            item.Setup(session, i, DeleteHistoryItemAt);
            item.SetDeleteMode(isHistoryDeleteMode);

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localScale = Vector3.one;
                itemRect.localRotation = Quaternion.identity;
            }

            Debug.Log("History item oluşturuldu: " + session.completedDateText);
        }

        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    private void ToggleHistoryDeleteMode()
    {
        isHistoryDeleteMode = !isHistoryDeleteMode;
        UpdateDeleteModeButtonText();
        ApplyDeleteModeToHistoryItems();
    }

    private void UpdateDeleteModeButtonText()
    {
        if (deleteModeButtonText != null)
            deleteModeButtonText.text = isHistoryDeleteMode ? "İptal Et" : "Sil";
    }

    private void ApplyDeleteModeToHistoryItems()
    {
        if (historyContent == null)
            return;

        foreach (Transform child in historyContent)
        {
            HistoryItemUI item = child.GetComponent<HistoryItemUI>();
            if (item != null)
                item.SetDeleteMode(isHistoryDeleteMode);
        }
    }

    private void DeleteHistoryItemAt(int index)
    {
        if (runHistoryManager == null)
            return;

        runHistoryManager.RemoveSessionAt(index);

        if (runHistoryManager.SessionHistoryList.Count == 0)
        {
            isHistoryDeleteMode = false;
            UpdateDeleteModeButtonText();
        }

        UpdateHistorySection();
        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    private IEnumerator ForceHistoryLayoutRefreshNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = historyContent as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (historyContent != null && historyContent.parent is RectTransform viewportRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);

        if (historySection != null)
        {
            RectTransform historySectionRect = historySection.GetComponent<RectTransform>();
            if (historySectionRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(historySectionRect);
        }

        if (mainMenuPanel != null)
        {
            RectTransform mainMenuRect = mainMenuPanel.GetComponent<RectTransform>();
            if (mainMenuRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(mainMenuRect);
        }

        if (historyScrollRect == null && historyContent != null)
            historyScrollRect = historyContent.GetComponentInParent<ScrollRect>();

        if (historyScrollRect != null)
        {
            historyScrollRect.StopMovement();
            historyScrollRect.horizontalNormalizedPosition = 0f;
            historyScrollRect.verticalNormalizedPosition = 1f;
        }

        Canvas.ForceUpdateCanvases();
    }

    private void UpdateContinueButtonState(bool canContinue)
    {
        if (continueButton == null)
            return;

        continueButton.interactable = canContinue;
    }
}