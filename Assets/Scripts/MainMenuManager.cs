using UnityEngine.UI;
using UnityEngine;
using Transform = UnityEngine.Transform;
using System.Collections;

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

        // Sadece HistoryItemUI olan child'ları temizle
        for (int i = historyContent.childCount - 1; i >= 0; i--)
        {
            Transform child = historyContent.GetChild(i);
            if (child.GetComponent<HistoryItemUI>() != null)
                Destroy(child.gameObject);
        }

        Canvas.ForceUpdateCanvases();

        // En yeni üstte gözüksün diye sondan başa doğru basıyoruz
        for (int i = runHistoryManager.SessionHistoryList.Count - 1; i >= 0; i--)
        {
            SessionHistoryData session = runHistoryManager.SessionHistoryList[i];
            if (session == null)
                continue;

            HistoryItemUI item = Instantiate(historyItemPrefab, historyContent);

            item.gameObject.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localPosition = Vector3.zero;

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.anchorMin = new Vector2(0f, 1f);
                itemRect.anchorMax = new Vector2(1f, 1f);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.anchoredPosition = Vector2.zero;
                itemRect.offsetMin = new Vector2(itemRect.offsetMin.x, itemRect.offsetMin.y);
                itemRect.offsetMax = new Vector2(itemRect.offsetMax.x, itemRect.offsetMax.y);
            }

            item.SetHistoryContentRect(historyContent as RectTransform);
            item.Setup(session);

            Debug.Log("History item oluşturuldu: " + session.completedDateText);
        }

        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    private IEnumerator ForceHistoryLayoutRefreshNextFrame()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = historyContent as RectTransform;
        if (contentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

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

        Canvas.ForceUpdateCanvases();
    }

    private void UpdateContinueButtonState(bool canContinue)
    {
        if (continueButton == null)
            return;

        continueButton.interactable = canContinue;
    }
}