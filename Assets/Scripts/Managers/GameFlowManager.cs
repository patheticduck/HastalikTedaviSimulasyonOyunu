using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class PatientAnalysisEntry
{
    public int dayNumber;
    public int patientNumberInDay;
    public int globalPatientNumber;
    public bool isCorrect;
    public string correctDiagnosisName;
    public string playerDiagnosisName;
}

public class GameFlowManager : MonoBehaviour
{
    [Header("Main Managers")]
    public MainMenuManager mainMenuManager;

    [Header("Interaction Mode")]
    public PlayerInteractionModeManager interactionModeManager;

    [Header("Patient")]
    public PatientMovement patientMovement;

    [Header("Patient Pools")]
    public List<PatientCaseData> respiratoryCases = new List<PatientCaseData>();
    public List<PatientCaseData> gastroCases = new List<PatientCaseData>();
    public List<PatientCaseData> specialCases = new List<PatientCaseData>();

    [Header("Old Patient Cases - Deprecated")]
    [Tooltip("Eski sistemden kalma. Yeni sistemde respiratoryCases listesine ekle.")]
    public PatientCaseData gripCase;

    [Tooltip("Eski sistemden kalma. Yeni sistemde respiratoryCases listesine ekle.")]
    public PatientCaseData coldCase;

    private readonly List<PatientCaseData> plannedPatientCases = new List<PatientCaseData>();
    private readonly List<string> plannedPatientCaseIds = new List<string>();

    [Header("Panels")]
    public GameObject dialoguePanel;
    public GameObject endDayPanel;
    public GameObject gameOverPanel;
    public GameObject dayTransitionPanel;
    public GameObject heartsPanel;

    [Header("Patient Result Panel")]
    public GameObject patientResultPanel;
    public TextMeshProUGUI patientResultTitleText;
    public TextMeshProUGUI patientResultBodyText;
    public Button patientResultContinueButton;

    [Header("Patient Result Title Colors")]
    [SerializeField] private Color correctTitleColor = new Color32(80, 200, 120, 255);
    [SerializeField] private Color wrongTitleColor = new Color32(220, 70, 70, 255);

    [Header("End Day UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    [Header("Game Over UI")]
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverResultText;
    public Button gameOverContinueButton;

    [Header("Game Over Title Colors")]
    [SerializeField] private Color gameWinTitleColor = new Color32(90, 220, 120, 255);
    [SerializeField] private Color gameLoseTitleColor = new Color32(255, 60, 60, 255);

    [Header("Day Transition UI")]
    public TextMeshProUGUI dayTransitionText;
    public CanvasGroup dayTransitionCanvasGroup;

    [Header("Heart UI")]
    public GameObject[] hearts;

    [Header("Energy UI")]
    public GameObject energyPanel;
    public EnergyUIManager energyUIManager;
    public int maxEnergy = 5;
    public int currentEnergy = 5;

    private bool doctorBookUnlockedForCurrentPatient = false;

    [Header("Notebook Access")]
    [SerializeField] private bool blockNotebookInteraction = true;

    [Header("Game Data")]
    public int currentDay = 1;
    public int maxHearts = 5;
    public int remainingHearts = 5;

    public int totalCorrect = 0;
    public int totalWrong = 0;

    [Header("Patient Flow")]
    public int totalPatientsInRun = 15;
    public int patientsPerDay = 3;
    public int currentPatientNumber = 1;

    private bool gameEnded = false;
    private bool waitingForDayContinue = false;
    private bool waitingForPatientResultContinue = false;
    private bool pendingLoseGameOver = false;

    private bool patientTransitionInProgress = false;
    private bool diagnosisFlowInProgress = false;
    private bool diagnosisSubmittedForCurrentPatient = false;
    private bool currentPatientDiagnosisResolved = false;

    private int dayCorrect = 0;
    private int dayWrong = 0;

    private bool lastDiagnosisWasCorrect = false;
    private string lastDiagnosisResultText = "";
    private string lastCorrectDiagnosisName = "";

    private readonly List<PatientAnalysisEntry> patientAnalysisHistory = new List<PatientAnalysisEntry>();

    private string currentRunId = "";

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (heartsPanel != null) heartsPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        SetEnergyPanelVisible(false);

        if (dayTransitionCanvasGroup != null)
            dayTransitionCanvasGroup.alpha = 0f;

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);

        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        currentEnergy = maxEnergy;
        ResetDoctorBookForNewPatient();

        UpdateHeartsUI();
        UpdateEnergyUI();
    }

    public void ResetGame()
    {
        currentRunId = System.Guid.NewGuid().ToString();

        currentDay = 1;
        currentPatientNumber = 1;

        remainingHearts = maxHearts;
        currentEnergy = maxEnergy;
        ResetDoctorBookForNewPatient();

        totalCorrect = 0;
        totalWrong = 0;

        dayCorrect = 0;
        dayWrong = 0;

        lastDiagnosisWasCorrect = false;
        lastDiagnosisResultText = "";
        lastCorrectDiagnosisName = "";

        waitingForDayContinue = false;
        waitingForPatientResultContinue = false;

        pendingLoseGameOver = false;
        diagnosisSubmittedForCurrentPatient = false;
        currentPatientDiagnosisResolved = false;
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;
        gameEnded = false;

        patientAnalysisHistory.Clear();

        GeneratePlannedPatientCases();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (heartsPanel != null) heartsPanel.SetActive(true);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        SetEnergyPanelVisible(true);

        if (dayTransitionCanvasGroup != null)
            dayTransitionCanvasGroup.alpha = 0f;

        UpdateHeartsUI();
        UpdateEnergyUI();
        SetNotebookBlocked(true);

        if (patientMovement != null)
            patientMovement.HidePatient();

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        Debug.Log("Yeni oyun başlatıldı. Rastgele hasta sırası oluşturuldu.");
    }

    private void GeneratePlannedPatientCases()
    {
        plannedPatientCases.Clear();
        plannedPatientCaseIds.Clear();

        List<PatientCaseData> respiratoryPool = BuildCleanPool(respiratoryCases);
        List<PatientCaseData> gastroPool = BuildCleanPool(gastroCases);
        List<PatientCaseData> specialPool = BuildCleanPool(specialCases);

        AddDeprecatedCasesIfNeeded(respiratoryPool);

        AddRandomCasesFromPool(respiratoryPool, 6, "Solunum");
        AddRandomCasesFromPool(gastroPool, 6, "Mide/Gastro");
        AddRandomCasesFromPool(specialPool, 3, "Özel");

        totalPatientsInRun = plannedPatientCases.Count;

        foreach (PatientCaseData patientCase in plannedPatientCases)
            plannedPatientCaseIds.Add(GetCaseSaveId(patientCase));

        Debug.Log("Planlanan hasta sayısı: " + plannedPatientCases.Count);
    }

    private List<PatientCaseData> BuildCleanPool(List<PatientCaseData> source)
    {
        List<PatientCaseData> cleanPool = new List<PatientCaseData>();

        if (source == null)
            return cleanPool;

        foreach (PatientCaseData patientCase in source)
        {
            if (patientCase == null)
                continue;

            if (!cleanPool.Contains(patientCase))
                cleanPool.Add(patientCase);
        }

        return cleanPool;
    }

    private void AddDeprecatedCasesIfNeeded(List<PatientCaseData> respiratoryPool)
    {
        if (gripCase != null && !respiratoryPool.Contains(gripCase))
            respiratoryPool.Add(gripCase);

        if (coldCase != null && !respiratoryPool.Contains(coldCase))
            respiratoryPool.Add(coldCase);
    }

    private void AddRandomCasesFromPool(List<PatientCaseData> pool, int targetCount, string poolName)
    {
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning(poolName + " hasta havuzu boş.");
            return;
        }

        if (pool.Count < targetCount)
        {
            Debug.LogWarning(poolName + " havuzunda yeterli hasta yok. İstenen: " + targetCount + ", Var olan: " + pool.Count);
        }

        List<PatientCaseData> tempPool = new List<PatientCaseData>(pool);
        int count = Mathf.Min(targetCount, tempPool.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            PatientCaseData selectedCase = tempPool[randomIndex];

            plannedPatientCases.Add(selectedCase);
            tempPool.RemoveAt(randomIndex);
        }
    }

    private string GetCaseSaveId(PatientCaseData patientCase)
    {
        if (patientCase == null)
            return "";

        if (!string.IsNullOrEmpty(patientCase.caseId))
            return patientCase.caseId;

        return patientCase.name;
    }

    private PatientCaseData FindCaseBySaveId(string caseId)
    {
        if (string.IsNullOrEmpty(caseId))
            return null;

        List<PatientCaseData> allCases = new List<PatientCaseData>();
        allCases.AddRange(BuildCleanPool(respiratoryCases));
        allCases.AddRange(BuildCleanPool(gastroCases));
        allCases.AddRange(BuildCleanPool(specialCases));

        AddDeprecatedCasesIfNeeded(allCases);

        foreach (PatientCaseData patientCase in allCases)
        {
            if (patientCase == null)
                continue;

            if (patientCase.caseId == caseId || patientCase.name == caseId)
                return patientCase;
        }

        return null;
    }

    private PatientCaseData GetCurrentPlannedPatientCase()
    {
        if (plannedPatientCases.Count == 0)
            GeneratePlannedPatientCases();

        int index = currentPatientNumber - 1;

        if (index < 0 || index >= plannedPatientCases.Count)
        {
            Debug.LogError("Hasta index hatası. currentPatientNumber: " + currentPatientNumber + ", planned count: " + plannedPatientCases.Count);
            return null;
        }

        return plannedPatientCases[index];
    }

    public void StartGameplayFlow()
    {
        if (gameEnded) return;

        SetEnergyPanelVisible(true);

        diagnosisSubmittedForCurrentPatient = false;
        currentPatientDiagnosisResolved = false;
        ResetDoctorBookForNewPatient();
        patientTransitionInProgress = true;
        diagnosisFlowInProgress = false;

        Debug.Log("Oyun akışı başlatıldı. Hasta No: " + currentPatientNumber);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        SetNotebookBlocked(true);

        AssignCurrentPatientCase();

        if (patientMovement != null)
            patientMovement.BeginWalk();
        else
            Debug.LogWarning("PatientMovement bağlı değil!");
    }

    private void AssignCurrentPatientCase()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager bulunamadı, vaka atanamadı.");
            return;
        }

        PatientCaseData selectedCase = GetCurrentPlannedPatientCase();

        if (selectedCase != null)
        {
            dialogueManager.currentCase = selectedCase;
            Debug.Log("Atanan vaka: " + selectedCase.name);
            return;
        }

        Debug.LogWarning("Planlanan hasta bulunamadı. Vaka atanamadı.");
    }

    public RuntimeSaveData BuildRuntimeSaveData()
    {
        RuntimeSaveData save = new RuntimeSaveData();

        save.currentDay = currentDay;
        save.currentPatientNumber = currentPatientNumber;
        save.totalCorrect = totalCorrect;
        save.totalWrong = totalWrong;
        save.remainingHearts = remainingHearts;
        save.maxEnergy = maxEnergy;
        save.currentEnergy = currentEnergy;
        save.doctorBookUnlockedForCurrentPatient = doctorBookUnlockedForCurrentPatient;
        save.dayCorrect = dayCorrect;
        save.dayWrong = dayWrong;
        save.diagnosisSubmittedForCurrentPatient = diagnosisSubmittedForCurrentPatient;
        save.patientResultPanelPendingAfterLoad = diagnosisSubmittedForCurrentPatient;
        save.lastDiagnosisWasCorrect = lastDiagnosisWasCorrect;
        save.lastDiagnosisResultText = lastDiagnosisResultText;
        save.lastCorrectDiagnosisName = lastCorrectDiagnosisName;
        save.pendingLoseGameOver = pendingLoseGameOver;
        save.hasSave = true;
        save.runId = currentRunId;

        if (DoctorProfileManager.Instance != null)
        {
            save.selectedDoctorSpecialization = DoctorProfileManager.Instance.GetSelectedSpecialization();
        }
        else
        {
            save.selectedDoctorSpecialization = DoctorSpecialization.None;
        }

        save.patientAnalysisHistory = ClonePatientAnalysisHistory();
        save.plannedPatientCaseIds = new List<string>(plannedPatientCaseIds);

        return save;
    }

    public void ApplyRuntimeSaveData(RuntimeSaveData save)
    {
        if (save == null)
            return;

        if (DoctorProfileManager.Instance != null)
            DoctorProfileManager.Instance.SelectSpecialization(save.selectedDoctorSpecialization);

        currentRunId = save.runId;

        if (string.IsNullOrEmpty(currentRunId))
            currentRunId = System.Guid.NewGuid().ToString();

        currentDay = save.currentDay;
        currentPatientNumber = save.currentPatientNumber;
        totalCorrect = save.totalCorrect;
        totalWrong = save.totalWrong;
        remainingHearts = save.remainingHearts;

        maxEnergy = save.maxEnergy > 0 ? save.maxEnergy : maxEnergy;
        currentEnergy = Mathf.Clamp(save.currentEnergy, 0, maxEnergy);
        doctorBookUnlockedForCurrentPatient = save.doctorBookUnlockedForCurrentPatient;

        if (!save.diagnosisSubmittedForCurrentPatient &&
            save.doctorBookUnlockedForCurrentPatient)
        {       
        currentEnergy = Mathf.Clamp(currentEnergy + 1, 0, maxEnergy);
        doctorBookUnlockedForCurrentPatient = false;
        }

        dayCorrect = save.dayCorrect;
        dayWrong = save.dayWrong;

        diagnosisSubmittedForCurrentPatient = save.diagnosisSubmittedForCurrentPatient;

        lastDiagnosisWasCorrect = save.lastDiagnosisWasCorrect;
        lastDiagnosisResultText = save.lastDiagnosisResultText;
        lastCorrectDiagnosisName = save.lastCorrectDiagnosisName;
        pendingLoseGameOver = save.pendingLoseGameOver;

        currentPatientDiagnosisResolved = save.diagnosisSubmittedForCurrentPatient;

        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        waitingForDayContinue = false;
        waitingForPatientResultContinue = false;
        gameEnded = false;

        RestorePlannedPatientCasesFromSave(save.plannedPatientCaseIds);

        patientAnalysisHistory.Clear();

        if (save.patientAnalysisHistory != null)
        {
            foreach (PatientAnalysisEntry entry in save.patientAnalysisHistory)
            {
                if (entry == null)
                    continue;

                patientAnalysisHistory.Add(new PatientAnalysisEntry
                {
                    dayNumber = entry.dayNumber,
                    patientNumberInDay = entry.patientNumberInDay,
                    globalPatientNumber = entry.globalPatientNumber,
                    isCorrect = entry.isCorrect,
                    correctDiagnosisName = entry.correctDiagnosisName,
                    playerDiagnosisName = entry.playerDiagnosisName
                });
            }
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        SetEnergyPanelVisible(true);

        if (patientMovement != null)
            patientMovement.HidePatient();

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        SetNotebookBlocked(true);
        UpdateHeartsUI();
        UpdateEnergyUI();
    }

    private void RestorePlannedPatientCasesFromSave(List<string> savedCaseIds)
    {
        plannedPatientCases.Clear();
        plannedPatientCaseIds.Clear();

        if (savedCaseIds == null || savedCaseIds.Count == 0)
        {
            Debug.LogWarning("Save içinde hasta sırası bulunamadı. Yeni sıra oluşturuluyor.");
            GeneratePlannedPatientCases();
            return;
        }

        foreach (string caseId in savedCaseIds)
        {
            PatientCaseData patientCase = FindCaseBySaveId(caseId);

            if (patientCase == null)
            {
                Debug.LogWarning("Save içindeki hasta bulunamadı: " + caseId);
                continue;
            }

            plannedPatientCases.Add(patientCase);
            plannedPatientCaseIds.Add(caseId);
        }

        totalPatientsInRun = plannedPatientCases.Count;

        if (plannedPatientCases.Count == 0)
            GeneratePlannedPatientCases();
    }

    public void ContinueAfterLoadedSave()
    {
        SetEnergyPanelVisible(true);

        if (diagnosisSubmittedForCurrentPatient)
        {
            SetNotebookBlocked(true);
            patientTransitionInProgress = false;
            diagnosisFlowInProgress = false;

            waitingForPatientResultContinue = true;
            ShowPatientResultPanel();

            return;
        }

        if (remainingHearts <= 0)
        {
            ShowGameOverPanel(false);
            return;
        }

        StartGameplayFlow();
    }

    private List<PatientAnalysisEntry> ClonePatientAnalysisHistory()
    {
        List<PatientAnalysisEntry> clone = new List<PatientAnalysisEntry>();

        foreach (PatientAnalysisEntry entry in patientAnalysisHistory)
        {
            if (entry == null)
                continue;

            clone.Add(new PatientAnalysisEntry
            {
                dayNumber = entry.dayNumber,
                patientNumberInDay = entry.patientNumberInDay,
                globalPatientNumber = entry.globalPatientNumber,
                isCorrect = entry.isCorrect,
                correctDiagnosisName = entry.correctDiagnosisName,
                playerDiagnosisName = entry.playerDiagnosisName
            });
        }

        return clone;
    }

    public void AddCorrectAnswer()
    {
        if (gameEnded) return;

        totalCorrect++;
        dayCorrect++;

        Debug.Log("Doğru eklendi. Toplam doğru: " + totalCorrect);
    }

    public void AddWrongAnswer()
    {
        if (gameEnded) return;

        totalWrong++;
        dayWrong++;
        remainingHearts--;

        if (remainingHearts < 0)
            remainingHearts = 0;

        UpdateHeartsUI();

        Debug.Log("Yanlış eklendi. Toplam yanlış: " + totalWrong);

        if (remainingHearts <= 0)
            pendingLoseGameOver = true;
    }

    public void ShowDayTransitionPanel(string message)
    {
        SetNotebookBlocked(true);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(true);

        if (dayTransitionText != null)
            dayTransitionText.text = message;
    }

    public void CloseDayTransitionPanel()
    {
        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        SetNotebookBlocked(true);
    }

    private void ShowEndOfDayPanel()
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "GÜN " + currentDay + " TAMAMLANDI";

        if (resultText != null)
        {
            resultText.text =
                currentDay + ". Gün Değerlendirmesi\n\n" +
                "Bu Gün Doğru: " + dayCorrect + "\n" +
                "Bu Gün Yanlış: " + dayWrong + "\n\n" +
                "Toplam Doğru: " + totalCorrect + "\n" +
                "Toplam Yanlış: " + totalWrong + "\n\n" +
                "Hasta: " + currentPatientNumber + " / " + totalPatientsInRun;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "Devam Et";
        }
    }

    private void CloseEndOfDayPanel()
    {
        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        SetNotebookBlocked(true);
    }

    private void ShowGameOverPanel(bool isVictory)
    {
        gameEnded = true;
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;
        SetNotebookBlocked(true);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.EnterUIMode();
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllMusic();

            if (isVictory)
                AudioManager.Instance.PlayWin();
            else
                AudioManager.Instance.PlayLose();
        }

        SetEnergyPanelVisible(false);

        if (gameOverTitleText != null)
        {
            if (isVictory)
            {
                gameOverTitleText.text = "OYUN TAMAMLANDI";
                gameOverTitleText.color = gameWinTitleColor;
            }
            else
            {
                gameOverTitleText.text = "OYUN BİTTİ";
                gameOverTitleText.color = gameLoseTitleColor;
            }
        }

        if (gameOverResultText != null)
            gameOverResultText.text = BuildDetailedGameOverText(isVictory);

        if (gameOverContinueButton != null)
        {
            gameOverContinueButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = gameOverContinueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "Ana Menüye Dön";
        }
    }

    private string BuildDetailedGameOverText(bool isVictory)
    {
        StringBuilder sb = new StringBuilder();

        if (isVictory)
            sb.AppendLine("5 günü başarıyla tamamladın.");
        else
            sb.AppendLine("3 yanlış teşhis hakkını doldurdun.");

        sb.AppendLine();
        sb.AppendLine("Toplam Doğru: " + totalCorrect);
        sb.AppendLine("Toplam Yanlış: " + totalWrong);
        sb.AppendLine();

        string doctorName = "Belirsiz";

        if (DoctorProfileManager.Instance != null)
            doctorName = DoctorProfileManager.Instance.GetSelectedDoctorDisplayName();

        sb.AppendLine("Seçilen Doktor: " + doctorName);
        sb.AppendLine();

        if (patientAnalysisHistory.Count == 0)
        {
            sb.AppendLine("Henüz analiz kaydı bulunamadı.");
            return sb.ToString();
        }

        int maxPlayedDay = patientAnalysisHistory.Max(x => x.dayNumber);

        for (int day = 1; day <= maxPlayedDay; day++)
        {
            List<PatientAnalysisEntry> dayEntries = patientAnalysisHistory
                .Where(x => x.dayNumber == day)
                .OrderBy(x => x.patientNumberInDay)
                .ToList();

            if (dayEntries.Count == 0)
                continue;

            int dailyCorrectCount = dayEntries.Count(x => x.isCorrect);
            int dailyWrongCount = dayEntries.Count(x => !x.isCorrect);

            sb.AppendLine(day + ". Gün");

            foreach (PatientAnalysisEntry entry in dayEntries)
            {
                if (entry.isCorrect)
                {
                    sb.AppendLine(entry.patientNumberInDay + ". hasta: Doğru");
                    sb.AppendLine("Senin Teşhisin: " + entry.playerDiagnosisName);
                }
                else
                {
                    sb.AppendLine(entry.patientNumberInDay + ". hasta: Yanlış");
                    sb.AppendLine("Senin Teşhisin: " + entry.playerDiagnosisName);
                    sb.AppendLine("Doğrusu: " + entry.correctDiagnosisName);
                }

                sb.AppendLine();
            }

            sb.AppendLine("Gün " + day + " Doğru: " + dailyCorrectCount);
            sb.AppendLine("Gün " + day + " Yanlış: " + dailyWrongCount);

            if (day < maxPlayedDay)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public SessionHistoryData BuildSessionHistoryData()
    {
        SessionHistoryData session = new SessionHistoryData();

        System.DateTime now = System.DateTime.Now;
        session.completedDateText = now.ToString("dd.MM.yyyy HH:mm");
        session.completedAtTicks = now.Ticks;
        session.relativeTimeText = "";

        if (patientAnalysisHistory == null || patientAnalysisHistory.Count == 0)
            return session;

        int maxPlayedDay = patientAnalysisHistory.Max(x => x.dayNumber);

        for (int day = 1; day <= maxPlayedDay; day++)
        {
            List<PatientAnalysisEntry> dayEntries = patientAnalysisHistory
                .Where(x => x.dayNumber == day)
                .OrderBy(x => x.patientNumberInDay)
                .ToList();

            if (dayEntries.Count == 0)
                continue;

            DayHistoryData dayData = new DayHistoryData();
            dayData.dayNumber = day;

            foreach (PatientAnalysisEntry entry in dayEntries)
            {
                PatientHistoryData patientData = new PatientHistoryData();
                patientData.patientLabel = "Hasta " + entry.patientNumberInDay;
                patientData.playerAnswer = entry.playerDiagnosisName;
                patientData.isCorrect = entry.isCorrect;
                patientData.correctAnswer = entry.correctDiagnosisName;

                dayData.patients.Add(patientData);
            }

            session.days.Add(dayData);
        }

        return session;
    }

    public void OnContinueButtonPressed()
    {
        if (gameEnded)
        {
            ReturnToMainMenuFromEndPanel();
            return;
        }

        if (waitingForDayContinue)
        {
            waitingForDayContinue = false;
            StartCoroutine(PlayDayTransitionAndStartNextDay());
        }
    }

    public void OnPatientResultContinuePressed()
    {
        if (gameEnded)
            return;

        if (!waitingForPatientResultContinue)
            return;

        waitingForPatientResultContinue = false;
        diagnosisSubmittedForCurrentPatient = false;
        currentPatientDiagnosisResolved = false;
        diagnosisFlowInProgress = false;

        HidePatientResultPanel();

        if (pendingLoseGameOver || remainingHearts <= 0)
        {
            pendingLoseGameOver = false;
            ShowGameOverPanel(false);
            return;
        }

        if (currentPatientNumber >= totalPatientsInRun)
        {
            ShowGameOverPanel(true);
            return;
        }

        bool isEndOfDay = (currentPatientNumber % patientsPerDay == 0);

        if (isEndOfDay)
        {
            waitingForDayContinue = true;
            ShowEndOfDayPanel();
        }
        else
        {
            currentPatientNumber++;
            StartCoroutine(StartNextPatientAfterDelay(0.5f));
        }
    }

    private IEnumerator PlayDayTransitionAndStartNextDay()
    {
        CloseEndOfDayPanel();

        currentDay++;
        currentPatientNumber++;

        ShowDayTransitionPanel(currentDay + ". GÜN");

        if (dayTransitionCanvasGroup == null)
        {
            Debug.LogWarning("DayTransitionCanvasGroup bağlı değil.");

            yield return new WaitForSeconds(1.5f);

            CloseDayTransitionPanel();

            dayCorrect = 0;
            dayWrong = 0;

            if (!gameEnded)
                StartGameplayFlow();

            yield break;
        }

        dayTransitionCanvasGroup.alpha = 0f;

        float fadeInDuration = 0.8f;
        float holdDuration = 1.0f;
        float fadeOutDuration = 0.8f;

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            dayTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }

        dayTransitionCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            dayTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }

        dayTransitionCanvasGroup.alpha = 0f;

        CloseDayTransitionPanel();

        dayCorrect = 0;
        dayWrong = 0;

        if (!gameEnded)
            StartGameplayFlow();
    }

    private void ShowPatientResultPanel()
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (patientResultPanel != null)
            patientResultPanel.SetActive(true);

        if (patientResultTitleText != null)
        {
            if (lastDiagnosisWasCorrect)
            {
                patientResultTitleText.text = "Doğru Teşhis";
                patientResultTitleText.color = correctTitleColor;
            }
            else
            {
                patientResultTitleText.text = "Yanlış Teşhis";
                patientResultTitleText.color = wrongTitleColor;
            }
        }

        if (patientResultBodyText != null)
        {
            if (lastDiagnosisWasCorrect)
            {
                patientResultBodyText.text = "Kalpler Korundu";
            }
            else
            {
                patientResultBodyText.text =
                    "Doğrusu: " + lastCorrectDiagnosisName + "\n" +
                    "-1 Kalp Gitti";
            }

            if (!string.IsNullOrEmpty(lastDiagnosisResultText))
                patientResultBodyText.text += "\n\n" + lastDiagnosisResultText;
        }

        if (patientResultContinueButton != null)
        {
            patientResultContinueButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = patientResultContinueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "Devam Et";
        }
    }

    private void HidePatientResultPanel()
    {
        if (patientResultPanel != null)
            patientResultPanel.SetActive(false);

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        SetNotebookBlocked(true);
    }

    public void ReturnToMainMenuFromEndPanel()
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        SetEnergyPanelVisible(false);

        if (mainMenuManager != null)
            mainMenuManager.ReturnToMenu(totalCorrect, totalWrong);
    }

    public void ForceStopAllGameplay()
    {
        Debug.Log("GAME HARD STOP");

        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;
        waitingForDayContinue = false;
        waitingForPatientResultContinue = false;
        pendingLoseGameOver = false;

        if (patientMovement != null)
            patientMovement.StopMovementImmediately();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        SetEnergyPanelVisible(false);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        SetNotebookBlocked(true);
    }

    public bool TryUseEnergyForDoctorBook()
    {
        if (gameEnded)
        {
            Debug.Log("Oyun bittikten sonra enerji kullanılamaz.");
            return false;
        }

        if (doctorBookUnlockedForCurrentPatient)
        {
            Debug.Log("DoctorBook bu hasta için zaten açık.");
            return false;
        }

        if (currentEnergy <= 0)
        {
            Debug.Log("Enerji kalmadı.");
            return false;
        }

        currentEnergy--;
        doctorBookUnlockedForCurrentPatient = true;
        UpdateEnergyUI();

        Debug.Log("Enerji kullanıldı. DoctorBook bu hasta için açıldı. Kalan enerji: " + currentEnergy);
        return true;
    }

    public bool IsDoctorBookUnlockedForCurrentPatient()
    {
        return doctorBookUnlockedForCurrentPatient;
    }

    public void ResetDoctorBookForNewPatient()
    {
        doctorBookUnlockedForCurrentPatient = false;
        UpdateEnergyUI();
    }

    public bool CanUseEnergyForDoctorBook()
    {
        if (gameEnded)
            return false;

        if (doctorBookUnlockedForCurrentPatient)
            return false;

        return currentEnergy > 0;
    }

    private void SetEnergyPanelVisible(bool visible)
    {
        if (energyPanel != null)
            energyPanel.SetActive(visible);

        if (visible)
            UpdateEnergyUI();
    }

    private void UpdateEnergyUI()
    {
        if (energyUIManager == null)
            energyUIManager = FindFirstObjectByType<EnergyUIManager>();

        if (energyUIManager != null)
            energyUIManager.UpdateEnergyUI();
    }

    private void UpdateHeartsUI()
    {
        if (hearts == null || hearts.Length == 0)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].SetActive(i < remainingHearts);
        }
    }

    public void RefreshGameUIAfterLoad()
    {
        UpdateHeartsUI();
        UpdateEnergyUI();
    }

    public void OnPatientArrived()
    {
        Debug.Log("Hasta geldi.");

        patientTransitionInProgress = false;
        SetNotebookBlocked(true);
        SetEnergyPanelVisible(true);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(true);
            interactionModeManager.OpenDialogueMode();
        }
        else if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager != null)
            dialogueManager.StartDialogue();
        else
            Debug.LogError("DialogueManager bulunamadı!");
    }

    public void OnPatientExited()
    {
        Debug.Log("Hasta çıktı.");

        diagnosisFlowInProgress = false;

        patientTransitionInProgress = false;
        SetNotebookBlocked(true);

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);
        else if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (!currentPatientDiagnosisResolved)
        {
            Debug.LogWarning("Hasta çıktı ama teşhis çözülmüş görünmüyor. Akış durduruldu.");
            return;
        }

        if (gameEnded)
            return;

        waitingForPatientResultContinue = true;
        ShowPatientResultPanel();
    }

    private IEnumerator StartNextPatientAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameEnded)
            yield break;

        StartGameplayFlow();
    }

    private void RegisterPatientAnalysis(bool isCorrect, string correctDiagnosisName, string playerDiagnosisName)
    {
        int patientNumberInDay = ((currentPatientNumber - 1) % patientsPerDay) + 1;

        PatientAnalysisEntry entry = new PatientAnalysisEntry
        {
            dayNumber = currentDay,
            patientNumberInDay = patientNumberInDay,
            globalPatientNumber = currentPatientNumber,
            isCorrect = isCorrect,
            correctDiagnosisName = string.IsNullOrEmpty(correctDiagnosisName) ? "Bilinmiyor" : correctDiagnosisName,
            playerDiagnosisName = string.IsNullOrEmpty(playerDiagnosisName) ? "Bilinmiyor" : playerDiagnosisName
        };

        patientAnalysisHistory.Add(entry);
    }

    public void SubmitDiagnosis(bool isCorrect)
    {
        lastDiagnosisWasCorrect = isCorrect;
        lastDiagnosisResultText = "";
        lastCorrectDiagnosisName = "Doğru teşhis";

        RegisterPatientAnalysis(isCorrect, lastCorrectDiagnosisName, lastCorrectDiagnosisName);

        if (isCorrect)
            AddCorrectAnswer();
        else
            AddWrongAnswer();

        diagnosisSubmittedForCurrentPatient = true;
        currentPatientDiagnosisResolved = true;
        diagnosisFlowInProgress = true;
        SetNotebookBlocked(true);

        FinishDiagnosisFlow(null);
    }

    public void SubmitDiagnosis(string diagnosisId)
    {
        Debug.Log("Seçilen teşhis ID: " + diagnosisId);

        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        bool isCorrect = false;
        string diagnosisResultText = "";
        string correctDiagnosisName = "";
        string playerDiagnosisName = "";

        if (dialogueManager != null && dialogueManager.currentCase != null)
        {
            DiagnosisChoiceData selectedDiagnosis = dialogueManager.currentCase.diagnosisChoices
                .FirstOrDefault(d => d != null && d.diseaseId == diagnosisId);

            if (selectedDiagnosis != null)
            {
                isCorrect = selectedDiagnosis.isCorrectDiagnosis;
                diagnosisResultText = selectedDiagnosis.resultText;
                playerDiagnosisName = selectedDiagnosis.diagnosisName;
            }

            DiagnosisChoiceData correctDiagnosis = dialogueManager.currentCase.diagnosisChoices
                .FirstOrDefault(d => d != null && d.isCorrectDiagnosis);

            if (correctDiagnosis != null)
                correctDiagnosisName = correctDiagnosis.diagnosisName;
        }
        else
        {
            Debug.LogWarning("DialogueManager veya currentCase bulunamadı. Teşhis doğruluğu kontrol edilemedi.");
        }

        lastDiagnosisWasCorrect = isCorrect;
        lastDiagnosisResultText = diagnosisResultText;
        lastCorrectDiagnosisName = string.IsNullOrEmpty(correctDiagnosisName) ? "Bilinmiyor" : correctDiagnosisName;

        RegisterPatientAnalysis(isCorrect, lastCorrectDiagnosisName, playerDiagnosisName);

        if (isCorrect)
        {
            AddCorrectAnswer();
            Debug.Log("Doğru teşhis yapıldı.");
        }
        else
        {
            AddWrongAnswer();
            Debug.Log("Yanlış teşhis yapıldı.");
        }

        diagnosisSubmittedForCurrentPatient = true;
        currentPatientDiagnosisResolved = true;
        diagnosisFlowInProgress = true;
        SetNotebookBlocked(true);

        FinishDiagnosisFlow(diagnosisResultText);
    }

    private void FinishDiagnosisFlow(string diagnosisResultText)
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = true;

        if (!string.IsNullOrEmpty(diagnosisResultText))
            Debug.Log("Teşhis sonuç metni: " + diagnosisResultText);

        if (patientMovement != null)
            patientMovement.BeginExit();
    }

    public void SetNotebookBlocked(bool blocked)
    {
        blockNotebookInteraction = blocked;
    }

    public bool IsNotebookBlocked()
    {
        return blockNotebookInteraction;
    }

    public bool IsPatientTransitionInProgress()
    {
        return patientTransitionInProgress;
    }

    public bool IsDiagnosisFlowInProgress()
    {
        return diagnosisFlowInProgress;
    }

    public bool CanOpenNotebook()
    {
        if (blockNotebookInteraction)
            return false;

        if (gameEnded)
            return false;

        if (diagnosisFlowInProgress)
            return false;

        if (patientTransitionInProgress)
            return false;

        if (dialoguePanel != null && dialoguePanel.activeSelf)
            return false;

        if (patientResultPanel != null && patientResultPanel.activeSelf)
            return false;

        if (endDayPanel != null && endDayPanel.activeSelf)
            return false;

        if (dayTransitionPanel != null && dayTransitionPanel.activeSelf)
            return false;

        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return false;

        return true;
    }

    public void NotifyFreeLookStateEntered()
    {
        if (gameEnded)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (diagnosisFlowInProgress)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (patientTransitionInProgress)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (patientResultPanel != null && patientResultPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (endDayPanel != null && endDayPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (dayTransitionPanel != null && dayTransitionPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        SetNotebookBlocked(false);
    }
}