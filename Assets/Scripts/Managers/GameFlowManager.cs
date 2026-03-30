using UnityEngine;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;

    [Header("Test Case")]
    public PatientCaseData firstCase;

    [Header("Game State")]
    public int currentDay = 1;
    public int currentPatientIndex = 0;
    public int wrongDiagnosisCount = 0;
    public int maxWrongDiagnosis = 5;

    [Header("Optional UI")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI patientText;
    public TextMeshProUGUI wrongCountText;
    public GameObject endPanel;
    public TextMeshProUGUI endPanelText;

    private PatientCaseData currentCase;
    private bool caseFinished = false;

    void Start()
    {
        Debug.Log("GameFlowManager Start çalıştı");
        StartGame();
    }

    public void StartGame()
    {
        Debug.Log("StartGame çalıştı");

        currentDay = 1;
        currentPatientIndex = 1;
        wrongDiagnosisCount = 0;
        caseFinished = false;

        UpdateHUD();
        LoadCase(firstCase);
    }

    public void LoadCase(PatientCaseData newCase)
    {
        Debug.Log("LoadCase çağrıldı");

        if (newCase == null)
        {
            Debug.LogError("GameFlowManager: Yüklenecek vaka yok.");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("GameFlowManager: dialogueManager bağlı değil.");
            return;
        }

        currentCase = newCase;
        dialogueManager.currentCase = currentCase;

        Debug.Log("DialogueManager currentCase atandı: " + currentCase.patientName);

        dialogueManager.StartDialogue();

        caseFinished = false;

        Debug.Log("Yeni vaka yüklendi: " + currentCase.patientName);
    }

    public void SubmitDiagnosis(string selectedDiagnosis)
    {
        Debug.Log("SubmitDiagnosis çağrıldı: " + selectedDiagnosis);

        if (currentCase == null)
        {
            Debug.LogError("SubmitDiagnosis: currentCase null.");
            return;
        }

        if (caseFinished)
        {
            Debug.LogWarning("SubmitDiagnosis: vaka zaten bitmiş.");
            return;
        }

        bool isCorrect = selectedDiagnosis == currentCase.correctDiagnosis;

        if (isCorrect)
        {
            Debug.Log("Doğru teşhis: " + selectedDiagnosis);
        }
        else
        {
            wrongDiagnosisCount++;
            Debug.Log("Yanlış teşhis: " + selectedDiagnosis);
        }

        UpdateHUD();
        EndCurrentCase(isCorrect, selectedDiagnosis);
    }

    void EndCurrentCase(bool isCorrect, string selectedDiagnosis)
    {
        caseFinished = true;
        Debug.Log("EndCurrentCase çalıştı");

        if (wrongDiagnosisCount >= maxWrongDiagnosis)
        {
            GameOver();
            return;
        }

        EndDay(isCorrect, selectedDiagnosis);
    }

    void EndDay(bool isCorrect, string selectedDiagnosis)
    {
        Debug.Log("EndDay çalıştı");

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endPanelText != null)
        {
            if (isCorrect)
            {
                endPanelText.text =
                    "Gün Sonu\n\n" +
                    "Hasta: " + currentCase.patientName + "\n" +
                    "Teşhis: " + selectedDiagnosis + "\n\n" +
                    "Doğru teşhis koydunuz.";
            }
            else
            {
                endPanelText.text =
                    "Gün Sonu\n\n" +
                    "Hasta: " + currentCase.patientName + "\n" +
                    "Teşhis: " + selectedDiagnosis + "\n" +
                    "Doğru Tanı: " + currentCase.correctDiagnosis + "\n\n" +
                    "Yanlış teşhis koydunuz.\n" +
                    "Kalan hata hakkı: " + (maxWrongDiagnosis - wrongDiagnosisCount);
            }
        }

        Debug.Log("Gün bitti.");
    }

    public void GameOver()
    {
        Debug.Log("GameOver çalıştı");

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endPanelText != null)
        {
            endPanelText.text =
                "OYUN BİTTİ\n\n" +
                "Toplam yanlış teşhis hakkınızı doldurdunuz.";
        }

        Debug.Log("Game Over");
    }

    void UpdateHUD()
    {
        if (dayText != null)
            dayText.text = "Gün: " + currentDay + "/7";

        if (patientText != null)
            patientText.text = "Hasta: " + currentPatientIndex;

        if (wrongCountText != null)
            wrongCountText.text = "Yanlış: " + wrongDiagnosisCount + "/" + maxWrongDiagnosis;
    }
}