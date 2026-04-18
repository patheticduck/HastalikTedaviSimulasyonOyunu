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

    [Header("Game Data")]
    public int currentDay = 1;
    public int maxHearts = 5;
    public int remainingHearts = 5;

    public int totalCorrect = 0;
    public int totalWrong = 0;

    [Header("Patient Flow")]
    public int totalPatientsInRun = 21;
    public int patientsPerDay = 3;
    public int currentPatientNumber = 1;

    private bool gameEnded = false;
    private bool waitingForDayContinue = false;
    private bool waitingForPatientResultContinue = false;

    // 5. yanlışta hasta önce çıksın, panel sonra gelsin
    private bool pendingLoseGameOver = false;

    // Günlük sayaçlar
    private int dayCorrect = 0;
    private int dayWrong = 0;

    // Son hastanın sonucu
    private bool lastDiagnosisWasCorrect = false;
    private string lastDiagnosisResultText = "";
    private string lastCorrectDiagnosisName = "";

    // Analiz kayıtları
    private readonly List<PatientAnalysisEntry> patientAnalysisHistory = new List<PatientAnalysisEntry>();

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (heartsPanel != null) heartsPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);

        if (dayTransitionCanvasGroup != null)
            dayTransitionCanvasGroup.alpha = 0f;

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);

        UpdateHeartsUI();
    }

    // -------------------------------------------------
    // OYUN BAŞLATMA / RESET
    // -------------------------------------------------

    public void ResetGame()
    {
        currentDay = 1;
        currentPatientNumber = 1;

        remainingHearts = maxHearts;
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
        gameEnded = false;

        patientAnalysisHistory.Clear();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (heartsPanel != null) heartsPanel.SetActive(true);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);

        if (dayTransitionCanvasGroup != null)
            dayTransitionCanvasGroup.alpha = 0f;

        UpdateHeartsUI();

        if (patientMovement != null)
            patientMovement.HidePatient();

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        Debug.Log("Yeni oyun başlatıldı.");
    }

    public void StartGameplayFlow()
    {
        if (gameEnded) return;

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

        if (patientMovement != null)
        {
            patientMovement.BeginWalk();
        }
        else
        {
            Debug.LogWarning("PatientMovement bağlı değil!");
        }
    }

    // -------------------------------------------------
    // DOĞRU / YANLIŞ
    // -------------------------------------------------

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
        {
            pendingLoseGameOver = true;
        }
    }

    // -------------------------------------------------
    // GÜN / GEÇİŞ
    // -------------------------------------------------

    public void ShowDayTransitionPanel(string message)
    {
        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(true);

        if (dayTransitionText != null)
            dayTransitionText.text = message;
    }

    public void CloseDayTransitionPanel()
    {
        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);
    }

    private void ShowEndOfDayPanel()
    {
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
    }

    // -------------------------------------------------
    // GAME OVER / FINAL PANEL
    // -------------------------------------------------

    private void ShowGameOverPanel(bool isVictory)
    {
        gameEnded = true;

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
        {
            gameOverResultText.text = BuildDetailedGameOverText(isVictory);
        }

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
        {
            sb.AppendLine("7 günü başarıyla tamamladın.");
        }
        else
        {
            sb.AppendLine("5 yanlış teşhis hakkını doldurdun.");
        }

        sb.AppendLine();
        sb.AppendLine("Toplam Doğru: " + totalCorrect);
        sb.AppendLine("Toplam Yanlış: " + totalWrong);
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

    // -------------------------------------------------
    // HISTORY DATA ÜRETİMİ
    // -------------------------------------------------

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

    // -------------------------------------------------
    // BUTONLAR
    // -------------------------------------------------

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
        HidePatientResultPanel();

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

    // -------------------------------------------------
    // HASTA SONUÇ PANELİ
    // -------------------------------------------------

    private void ShowPatientResultPanel()
    {
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
            {
                patientResultBodyText.text += "\n\n" + lastDiagnosisResultText;
            }
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
    }

    // -------------------------------------------------
    // MENÜYE DÖNÜŞ
    // -------------------------------------------------

    public void ReturnToMainMenuFromEndPanel()
    {
        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (mainMenuManager != null)
            mainMenuManager.ReturnToMenu(totalCorrect, totalWrong);
    }

    // -------------------------------------------------
    // HEART UI
    // -------------------------------------------------

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

    // -------------------------------------------------
    // HASTA AKIŞI
    // -------------------------------------------------

    public void OnPatientArrived()
    {
        Debug.Log("Hasta geldi.");

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
        {
            dialogueManager.StartDialogue();
        }
        else
        {
            Debug.LogError("DialogueManager bulunamadı!");
        }
    }

    public void OnPatientExited()
    {
        Debug.Log("Hasta çıktı.");

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);
        else if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (pendingLoseGameOver)
        {
            pendingLoseGameOver = false;
            ShowGameOverPanel(false);
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

    // -------------------------------------------------
    // TEŞHİS KAYDI
    // -------------------------------------------------

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

    // -------------------------------------------------
    // TEŞHİS GÖNDERİMİ
    // -------------------------------------------------

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
            {
                correctDiagnosisName = correctDiagnosis.diagnosisName;
            }
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

        FinishDiagnosisFlow(diagnosisResultText);
    }

    private void FinishDiagnosisFlow(string diagnosisResultText)
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        if (!string.IsNullOrEmpty(diagnosisResultText))
            Debug.Log("Teşhis sonuç metni: " + diagnosisResultText);

        if (patientMovement != null)
            patientMovement.BeginExit();
    }
}