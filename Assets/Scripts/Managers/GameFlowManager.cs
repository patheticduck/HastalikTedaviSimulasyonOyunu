using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;

    [Header("Test Case")]
    public PatientCaseData firstCase;

    [Header("Scene")]
    public GameObject patientObject;
    public PatientMovement patientMovement;

    [Header("Result UI")]
    public GameObject endPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    [Header("Day Transition UI")]
    public GameObject dayTransitionPanel;
    public CanvasGroup dayTransitionCanvasGroup;
    public TextMeshProUGUI dayTransitionText;

    [Header("Lives UI")]
    public Image[] heartImages;
    public Color fullHeartColor = new Color(1f, 0f, 0f, 1f);
    public Color emptyHeartColor = new Color(1f, 0f, 0f, 0.2f);

    [Header("Game State")]
    public int currentDay = 1;
    public int currentPatientInDay = 1;
    public int patientsPerDay = 3;

    public int wrongDiagnosisCount = 0;
    public int maxWrongDiagnosis = 5;

    private int dayCorrect = 0;
    private int dayWrong = 0;

    private int totalCorrect = 0;
    private int totalWrong = 0;

    // Gün bazlı istatistikler
    private int[] dayCorrectStats = new int[7];
    private int[] dayWrongStats = new int[7];

    private bool waitingForContinue = false;
    private bool isDayEnd = false;
    private bool isGameEnd = false;
    private bool pendingCaseResult = false;
    private bool lastDiagnosisCorrect = false;
    private string lastSelectedDiagnosis = "";

    private PatientCaseData currentCase;
    private TextMeshProUGUI continueButtonText;

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinuePressed);
            continueButtonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (endPanel != null)
            endPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        StartGame();
    }

    void StartGame()
    {
        currentDay = 1;
        currentPatientInDay = 1;
        wrongDiagnosisCount = 0;

        totalCorrect = 0;
        totalWrong = 0;

        dayCorrect = 0;
        dayWrong = 0;

        for (int i = 0; i < 7; i++)
        {
            dayCorrectStats[i] = 0;
            dayWrongStats[i] = 0;
        }

        waitingForContinue = false;
        isDayEnd = false;
        isGameEnd = false;
        pendingCaseResult = false;

        if (endPanel != null)
            endPanel.SetActive(false);

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        if (patientMovement != null)
            patientMovement.HidePatient();
        else if (patientObject != null)
            patientObject.SetActive(false);

        SetContinueButtonLabel("Devam Et");
        UpdateHearts();

        StartDay();
    }

    void StartDay()
    {
        dayCorrect = 0;
        dayWrong = 0;

        LoadCase(firstCase);
    }

    void LoadCase(PatientCaseData newCase)
    {
        if (newCase == null)
        {
            Debug.LogError("GameFlowManager: Yüklenecek vaka yok.");
            return;
        }

        currentCase = newCase;
        dialogueManager.currentCase = currentCase;

        if (endPanel != null)
            endPanel.SetActive(false);

        if (patientObject != null)
            patientObject.SetActive(true);

        if (patientMovement != null)
            patientMovement.BeginWalk();
        else
            dialogueManager.StartDialogue();
    }

    public void OnPatientArrived()
    {
        if (dialogueManager != null)
            dialogueManager.StartDialogue();
    }

    public void SubmitDiagnosis(string selectedDiagnosis)
    {
        if (currentCase == null)
            return;

        bool isCorrect = selectedDiagnosis == currentCase.correctDiagnosis;

        lastDiagnosisCorrect = isCorrect;
        lastSelectedDiagnosis = selectedDiagnosis;

        if (isCorrect)
        {
            dayCorrect++;
            totalCorrect++;
        }
        else
        {
            dayWrong++;
            totalWrong++;
            wrongDiagnosisCount++;
            UpdateHearts();
        }

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        pendingCaseResult = true;

        if (patientMovement != null)
            patientMovement.BeginExit();
        else
            OnPatientExited();
    }

    public void OnPatientExited()
    {
        if (!pendingCaseResult)
            return;

        pendingCaseResult = false;

        // O günün anlık istatistiğini kaydet
        if (currentDay >= 1 && currentDay <= 7)
        {
            dayCorrectStats[currentDay - 1] = dayCorrect;
            dayWrongStats[currentDay - 1] = dayWrong;
        }

        if (wrongDiagnosisCount >= maxWrongDiagnosis)
        {
            ShowGameOverSummary();
            return;
        }

        ShowCaseResult(lastDiagnosisCorrect, lastSelectedDiagnosis);
    }

    void ShowCaseResult(bool isCorrect, string selected)
    {
        if (endPanel != null)
            endPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text =
                "HASTA SONUCU\n\n" +
                "Seçtiğin Teşhis: " + selected + "\n" +
                "Doğru Tanı: " + currentCase.correctDiagnosis + "\n\n" +
                (isCorrect ? "DOĞRU TEŞHİS" : "YANLIŞ TEŞHİS") + "\n" +
                "Kalan Hata Hakkı: " + (maxWrongDiagnosis - wrongDiagnosisCount);
        }

        SetContinueButtonLabel("Devam Et");

        waitingForContinue = true;
        isDayEnd = false;
        isGameEnd = false;
    }

    void ShowDaySummary()
    {
        if (currentDay >= 1 && currentDay <= 7)
        {
            dayCorrectStats[currentDay - 1] = dayCorrect;
            dayWrongStats[currentDay - 1] = dayWrong;
        }

        if (endPanel != null)
            endPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text =
                "GÜN " + currentDay + " TAMAMLANDI\n\n" +
                "Bugünkü Doğru: " + dayCorrect + "\n" +
                "Bugünkü Yanlış: " + dayWrong + "\n\n" +
                "Toplam Doğru: " + totalCorrect + "\n" +
                "Toplam Yanlış: " + totalWrong + "\n" +
                "Toplam Hata: " + wrongDiagnosisCount + "/" + maxWrongDiagnosis;
        }

        SetContinueButtonLabel("Devam Et");

        waitingForContinue = true;
        isDayEnd = true;
        isGameEnd = false;
    }

    void ShowFinalSummary()
    {
        if (endPanel != null)
            endPanel.SetActive(true);

        float totalCases = totalCorrect + totalWrong;
        float successRate = totalCases > 0 ? (totalCorrect / totalCases) * 100f : 0f;

        if (resultText != null)
        {
            resultText.text =
                "HAFTA SONU DEĞERLENDİRMESİ\n\n" +
                "Toplam Doğru: " + totalCorrect + "\n" +
                "Toplam Yanlış: " + totalWrong + "\n" +
                "Başarı Oranı: %" + successRate.ToString("F1") + "\n" +
                "Toplam Hata: " + wrongDiagnosisCount + "/" + maxWrongDiagnosis + "\n\n" +
                BuildDayAnalysis(7);
        }

        SetContinueButtonLabel("Yeniden Başlat");

        waitingForContinue = true;
        isDayEnd = false;
        isGameEnd = true;
    }

    void ShowGameOverSummary()
    {
        if (endPanel != null)
            endPanel.SetActive(true);

        float totalCases = totalCorrect + totalWrong;
        float successRate = totalCases > 0 ? (totalCorrect / totalCases) * 100f : 0f;

        if (resultText != null)
        {
            resultText.text =
                "OYUN BİTTİ\n\n" +
                "5 yanlış teşhis hakkını doldurdun.\n\n" +
                "Bakılan Hasta: " + (totalCorrect + totalWrong) + "\n" +
                "Toplam Doğru: " + totalCorrect + "\n" +
                "Toplam Yanlış: " + totalWrong + "\n" +
                "Başarı Oranı: %" + successRate.ToString("F1") + "\n\n" +
                BuildDayAnalysis(currentDay);
        }

        SetContinueButtonLabel("Yeniden Başlat");

        waitingForContinue = true;
        isDayEnd = false;
        isGameEnd = true;
    }

    string BuildDayAnalysis(int lastDayToShow)
    {
        string text = "GÜN BAZLI ANALİZ\n";

        for (int i = 0; i < lastDayToShow; i++)
        {
            text += "\nGün " + (i + 1) +
                    " -> Doğru: " + dayCorrectStats[i] +
                    " | Yanlış: " + dayWrongStats[i];
        }

        return text;
    }

    void OnContinuePressed()
    {
        if (!waitingForContinue)
            return;

        waitingForContinue = false;

        if (endPanel != null)
            endPanel.SetActive(false);

        if (isGameEnd)
        {
            StartGame();
            return;
        }

        if (isDayEnd)
        {
            currentDay++;

            if (currentDay > 7)
            {
                ShowFinalSummary();
                return;
            }

            currentPatientInDay = 1;
            StartCoroutine(DayTransitionRoutine(currentDay));
            return;
        }

        if (currentPatientInDay < patientsPerDay)
        {
            currentPatientInDay++;
            LoadCase(firstCase);
        }
        else
        {
            if (currentDay >= 7)
                ShowFinalSummary();
            else
                ShowDaySummary();
        }
    }

    IEnumerator DayTransitionRoutine(int dayNumber)
    {
        if (dayTransitionPanel == null || dayTransitionCanvasGroup == null || dayTransitionText == null)
        {
            StartDay();
            yield break;
        }

        dayTransitionPanel.SetActive(true);
        dayTransitionText.text = "GÜN " + dayNumber;

        float duration = 0.6f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            dayTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        dayTransitionCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(1.0f);

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            dayTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        dayTransitionCanvasGroup.alpha = 0f;
        dayTransitionPanel.SetActive(false);

        StartDay();
    }

    void UpdateHearts()
    {
        if (heartImages == null || heartImages.Length == 0)
            return;

        int remainingLives = maxWrongDiagnosis - wrongDiagnosisCount;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
                continue;

            if (i < remainingLives)
                heartImages[i].color = fullHeartColor;
            else
                heartImages[i].color = emptyHeartColor;
        }
    }

    void SetContinueButtonLabel(string newText)
    {
        if (continueButtonText != null)
            continueButtonText.text = newText;
    }
}