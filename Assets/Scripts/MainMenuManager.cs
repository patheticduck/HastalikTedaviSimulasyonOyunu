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

    [Header("Continue Save Panel")]
    public GameObject continueSavePanel;
    public Transform continueSaveContent;
    public ContinueSaveItemUI continueSaveItemPrefab;
    public TextMeshProUGUI continueSaveDetailText;
    public Button continueSelectedSaveButton;
    public Button continueSaveBackButton;

    [Header("Continue Save Delete")]
    public Button continueSaveDeleteModeButton;
    public TextMeshProUGUI continueSaveDeleteModeButtonText;
    public Button continueSaveClearAllButton;
    public GameObject confirmClearAllContinuePanel;
    public Button confirmClearAllContinueYesButton;
    public Button confirmClearAllContinueNoButton;

    [Header("History UI")]
    public Transform historyContent;
    public HistoryItemUI historyItemPrefab;

    [Header("History Delete Mode")]
    public Button deleteModeButton;
    public TextMeshProUGUI deleteModeButtonText;

    [Header("Clear All History")]
    public Button clearAllButton;
    public GameObject confirmDeleteAllPanel;
    public Button confirmDeleteYesButton;
    public Button confirmDeleteNoButton;

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
    private ScrollRect continueSaveScrollRect;

    private bool isHistoryDeleteMode = false;
    private bool isContinueSaveDeleteMode = false;

    private int selectedContinueSaveIndex = -1;
    private RuntimeSaveData selectedContinueSave = null;

    private string activeContinueSaveRunId = "";

    private void Awake()
    {
        if (historyContent != null)
            historyScrollRect = historyContent.GetComponentInParent<ScrollRect>();

        if (continueSaveContent != null)
            continueSaveScrollRect = continueSaveContent.GetComponentInParent<ScrollRect>();
    }

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OpenContinueSavePanel);
        }

        if (continueSelectedSaveButton != null)
        {
            continueSelectedSaveButton.onClick.RemoveAllListeners();
            continueSelectedSaveButton.onClick.AddListener(ContinueSelectedSave);
            continueSelectedSaveButton.interactable = false;
        }

        if (continueSaveBackButton != null)
        {
            continueSaveBackButton.onClick.RemoveAllListeners();
            continueSaveBackButton.onClick.AddListener(CloseContinueSavePanel);
        }

        if (continueSaveDeleteModeButton != null)
        {
            continueSaveDeleteModeButton.onClick.RemoveAllListeners();
            continueSaveDeleteModeButton.onClick.AddListener(ToggleContinueSaveDeleteMode);
        }

        if (continueSaveClearAllButton != null)
        {
            continueSaveClearAllButton.onClick.RemoveAllListeners();
            continueSaveClearAllButton.onClick.AddListener(OpenConfirmClearAllContinuePanel);
        }

        if (confirmClearAllContinueYesButton != null)
        {
            confirmClearAllContinueYesButton.onClick.RemoveAllListeners();
            confirmClearAllContinueYesButton.onClick.AddListener(ConfirmClearAllContinueSaves);
        }

        if (confirmClearAllContinueNoButton != null)
        {
            confirmClearAllContinueNoButton.onClick.RemoveAllListeners();
            confirmClearAllContinueNoButton.onClick.AddListener(CloseConfirmClearAllContinuePanel);
        }

        if (deleteModeButton != null)
        {
            deleteModeButton.onClick.RemoveAllListeners();
            deleteModeButton.onClick.AddListener(ToggleHistoryDeleteMode);
        }

        if (clearAllButton != null)
        {
            clearAllButton.onClick.RemoveAllListeners();
            clearAllButton.onClick.AddListener(OpenConfirmDeleteAllPanel);
        }

        if (confirmDeleteYesButton != null)
        {
            confirmDeleteYesButton.onClick.RemoveAllListeners();
            confirmDeleteYesButton.onClick.AddListener(ConfirmClearAllHistory);
        }

        if (confirmDeleteNoButton != null)
        {
            confirmDeleteNoButton.onClick.RemoveAllListeners();
            confirmDeleteNoButton.onClick.AddListener(CloseConfirmDeleteAllPanel);
        }

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        ClearContinueSaveSelection();

        UpdateDeleteModeButtonText();
        UpdateContinueSaveDeleteModeButtonText();

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 0f;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

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

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainMenuMusic();

        MoveCameraRootTo(menuCameraPoint);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        bool canContinue =
            RuntimeSaveManager.Instance != null &&
            RuntimeSaveManager.Instance.HasAnyContinueSave();

        UpdateContinueButtonState(canContinue);
        UpdateHistorySection();

        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    public void StartGameFromMenu()
    {
        Time.timeScale = 1f;

        activeContinueSaveRunId = "";

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        if (heartsPanel != null)
            heartsPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameplayMusic();

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

    public void OpenContinueSavePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        bool hasAnySave =
            RuntimeSaveManager.Instance != null &&
            RuntimeSaveManager.Instance.HasAnyContinueSave();

        if (!hasAnySave)
        {
            Debug.LogWarning("Devam edilecek kayıt bulunamadı.");
            return;
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(true);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        ClearContinueSaveSelection();
        RebuildContinueSaveUI();

        StartCoroutine(ForceContinueSaveLayoutRefreshNextFrame());
    }

    public void CloseContinueSavePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        isContinueSaveDeleteMode = false;
        UpdateContinueSaveDeleteModeButtonText();
        ClearContinueSaveSelection();

        ShowMainMenu();
    }

    private void RebuildContinueSaveUI()
    {
        if (continueSaveContent == null || continueSaveItemPrefab == null)
        {
            Debug.LogWarning("RebuildContinueSaveUI -> content veya prefab eksik.");
            return;
        }

        for (int i = continueSaveContent.childCount - 1; i >= 0; i--)
        {
            Transform child = continueSaveContent.GetChild(i);
            if (child.GetComponent<ContinueSaveItemUI>() != null)
                Destroy(child.gameObject);
        }

        if (RuntimeSaveManager.Instance == null ||
            RuntimeSaveManager.Instance.ContinueSaveList == null)
            return;

        for (int i = 0; i < RuntimeSaveManager.Instance.ContinueSaveList.Count; i++)
        {
            RuntimeSaveData save = RuntimeSaveManager.Instance.ContinueSaveList[i];
            if (save == null)
                continue;

            ContinueSaveItemUI item = Instantiate(continueSaveItemPrefab, continueSaveContent, false);
            item.gameObject.SetActive(true);
            item.Setup(save, i, SelectContinueSave, DeleteContinueSaveItemAt);
            item.SetDeleteMode(isContinueSaveDeleteMode);

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localScale = Vector3.one;
                itemRect.localRotation = Quaternion.identity;
            }
        }

        StartCoroutine(ForceContinueSaveLayoutRefreshNextFrame());
    }

    private void SelectContinueSave(int index, RuntimeSaveData save)
    {
        selectedContinueSaveIndex = index;
        selectedContinueSave = save;

        if (continueSelectedSaveButton != null)
            continueSelectedSaveButton.interactable = selectedContinueSave != null;

        if (continueSaveDetailText != null)
            continueSaveDetailText.text = BuildContinueSaveDetailText(save);

        Debug.Log("Continue save seçildi. Index: " + index);
    }

    private void ClearContinueSaveSelection()
    {
        selectedContinueSaveIndex = -1;
        selectedContinueSave = null;

        if (continueSelectedSaveButton != null)
            continueSelectedSaveButton.interactable = false;

        if (continueSaveDetailText != null)
            continueSaveDetailText.text = "Devam etmek istediğiniz kaydı seçin.";
    }

    private string BuildContinueSaveDetailText(RuntimeSaveData save)
    {
        if (save == null)
            return "Kayıt bilgisi bulunamadı.";

        int patientsPerDay = 3;
        int totalPatients = 15;

        if (gameFlowManager != null)
        {
            if (gameFlowManager.patientsPerDay > 0)
                patientsPerDay = gameFlowManager.patientsPerDay;

            if (gameFlowManager.totalPatientsInRun > 0)
                totalPatients = gameFlowManager.totalPatientsInRun;
        }

        int patientInDay = ((save.currentPatientNumber - 1) % patientsPerDay) + 1;

        return
            "Seçilen Devam Kaydı\n\n" +
            "Kayıt Tarihi: " + (string.IsNullOrEmpty(save.savedDateText) ? "Bilinmiyor" : save.savedDateText) + "\n" +
            "Gün: " + save.currentDay + "\n" +
            "Hasta: " + patientInDay + " (" + save.currentPatientNumber + "/" + totalPatients + ")\n" +
            "Toplam Doğru: " + save.totalCorrect + "\n" +
            "Toplam Yanlış: " + save.totalWrong + "\n" +
            "Kalan Can: " + save.remainingHearts + "\n" +
            "Enerji: " + save.currentEnergy + " / " + save.maxEnergy;
    }

    public void ContinueSelectedSave()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (selectedContinueSave == null)
        {
            Debug.LogWarning("Seçili devam kaydı yok.");
            return;
        }

        activeContinueSaveRunId = selectedContinueSave.runId;

        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.SetCurrentSave(selectedContinueSave);

        ContinueGameFromSelectedSave(selectedContinueSave);
    }

    private void ContinueGameFromSelectedSave(RuntimeSaveData save)
    {
        if (save == null || !save.hasSave)
        {
            Debug.LogWarning("Devam edilecek kayıt geçersiz.");
            return;
        }

        Time.timeScale = 1f;

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

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

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameplayMusic();

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

    public void ContinueGameFromMenu()
    {
        OpenContinueSavePanel();
    }

    private void ToggleContinueSaveDeleteMode()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        isContinueSaveDeleteMode = !isContinueSaveDeleteMode;

        UpdateContinueSaveDeleteModeButtonText();
        ApplyContinueSaveDeleteModeToItems();
    }

    private void UpdateContinueSaveDeleteModeButtonText()
    {
        if (continueSaveDeleteModeButtonText != null)
            continueSaveDeleteModeButtonText.text = isContinueSaveDeleteMode ? "İptal Et" : "Sil";
    }

    private void ApplyContinueSaveDeleteModeToItems()
    {
        if (continueSaveContent == null)
            return;

        foreach (Transform child in continueSaveContent)
        {
            ContinueSaveItemUI item = child.GetComponent<ContinueSaveItemUI>();
            if (item != null)
                item.SetDeleteMode(isContinueSaveDeleteMode);
        }
    }

    private void DeleteContinueSaveItemAt(int index)
    {
        if (RuntimeSaveManager.Instance == null)
            return;

        RuntimeSaveManager.Instance.RemoveContinueSaveAt(index);

        ClearContinueSaveSelection();

        if (!RuntimeSaveManager.Instance.HasAnyContinueSave())
        {
            isContinueSaveDeleteMode = false;
            UpdateContinueSaveDeleteModeButtonText();

            CloseContinueSavePanel();
            return;
        }

        RebuildContinueSaveUI();
        UpdateContinueButtonState(RuntimeSaveManager.Instance.HasAnyContinueSave());
    }

    public void OpenConfirmClearAllContinuePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(true);
    }

    public void CloseConfirmClearAllContinuePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);
    }

    public void ConfirmClearAllContinueSaves()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.ClearAllContinueSaves();

        isContinueSaveDeleteMode = false;
        UpdateContinueSaveDeleteModeButtonText();
        ClearContinueSaveSelection();

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        UpdateContinueButtonState(false);

        CloseContinueSavePanel();
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

        if (!string.IsNullOrEmpty(activeContinueSaveRunId) && RuntimeSaveManager.Instance != null)
        {
            RuntimeSaveManager.Instance.RemoveContinueSaveByRunId(activeContinueSaveRunId);
            Debug.Log("Tamamlanan oyuna ait continue save silindi. RunId: " + activeContinueSaveRunId);
            activeContinueSaveRunId = "";
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

            if (confirmDeleteAllPanel != null)
                confirmDeleteAllPanel.SetActive(false);
        }

        if (deleteModeButton != null)
            deleteModeButton.gameObject.SetActive(hasAnyHistory);

        if (clearAllButton != null)
            clearAllButton.gameObject.SetActive(hasAnyHistory);

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

    public void OpenConfirmDeleteAllPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(true);
    }

    public void CloseConfirmDeleteAllPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);
    }

    public void ConfirmClearAllHistory()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (runHistoryManager != null)
            runHistoryManager.ClearAllHistory();

        isHistoryDeleteMode = false;
        UpdateDeleteModeButtonText();

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);

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

    private IEnumerator ForceContinueSaveLayoutRefreshNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = continueSaveContent as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (continueSaveContent != null && continueSaveContent.parent is RectTransform viewportRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);

        if (continueSavePanel != null)
        {
            RectTransform panelRect = continueSavePanel.GetComponent<RectTransform>();
            if (panelRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }

        if (continueSaveScrollRect == null && continueSaveContent != null)
            continueSaveScrollRect = continueSaveContent.GetComponentInParent<ScrollRect>();

        if (continueSaveScrollRect != null)
        {
            continueSaveScrollRect.StopMovement();
            continueSaveScrollRect.horizontalNormalizedPosition = 0f;
            continueSaveScrollRect.verticalNormalizedPosition = 1f;
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