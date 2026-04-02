using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;
    public GameObject patientObject;
    public PatientMovement patientMovement;

    [Header("Day Cases")]
    public List<PatientCaseData> dayCases = new List<PatientCaseData>();

    [Header("Settings")]
    public bool autoStartDayOnPlay = true;

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
        Debug.Log("GameFlowManager hazır");

        if (endPanel != null)
            endPanel.SetActive(false);

        UpdateHUD();

        if (autoStartDayOnPlay)
        {
            StartDay();
        }
    }

    public void StartDay()
    {
        Debug.Log("StartDay çalıştı");

        if (dayCases == null || dayCases.Count == 0)
        {
            Debug.LogError("GameFlowManager: dayCases listesi boş.");
            return;
        }

        currentPatientIndex = 0;
        caseFinished = false;
        currentCase = null;

        if (endPanel != null)
            endPanel.SetActive(false);

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        UpdateHUD();
        SendNextPatient();
    }

    void SendNextPatient()
    {
        if (currentPatientIndex < 0 || currentPatientIndex >= dayCases.Count)
        {
            EndDay();
            return;
        }

        Debug.Log("Sıradaki hasta gönderiliyor. Index: " + currentPatientIndex);

        if (patientMovement != null)
        {
            patientMovement.BeginWalk();
        }
        else if (patientObject != null)
        {
            patientObject.SetActive(true);
            Debug.LogWarning("patientMovement bağlı değil. Hasta sadece aktif edildi.");
        }
        else
        {
            Debug.LogError("GameFlowManager: ne patientMovement ne patientObject bağlı.");
        }
    }

    public void OnPatientArrived()
    {
        Debug.Log("GameFlowManager: hasta masaya ulaştı.");
        StartCurrentPatientCase();
    }

    void StartCurrentPatientCase()
    {
        if (currentPatientIndex < 0 || currentPatientIndex >= dayCases.Count)
        {
            Debug.LogError("StartCurrentPatientCase: geçersiz currentPatientIndex.");
            return;
        }

        PatientCaseData nextCase = dayCases[currentPatientIndex];
        LoadCase(nextCase);
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

        if (currentCase == null && dialogueManager != null)
        {
            currentCase = dialogueManager.currentCase;
            Debug.LogWarning("GameFlowManager currentCase boştu, DialogueManager'dan alındı.");
        }

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

        ShowCaseResult(isCorrect, selectedDiagnosis);
    }

    void ShowCaseResult(bool isCorrect, string selectedDiagnosis)
    {
        Debug.Log("ShowCaseResult çalıştı");

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endPanelText != null)
        {
            if (isCorrect)
            {
                endPanelText.text =
                    "Hasta Sonucu\n\n" +
                    "Hasta: " + currentCase.patientName + "\n" +
                    "Teşhis: " + selectedDiagnosis + "\n\n" +
                    "Doğru teşhis koydunuz.";
            }
            else
            {
                endPanelText.text =
                    "Hasta Sonucu\n\n" +
                    "Hasta: " + currentCase.patientName + "\n" +
                    "Teşhis: " + selectedDiagnosis + "\n" +
                    "Doğru Tanı: " + currentCase.correctDiagnosis + "\n\n" +
                    "Yanlış teşhis koydunuz.\n" +
                    "Kalan hata hakkı: " + (maxWrongDiagnosis - wrongDiagnosisCount);
            }
        }

        Debug.Log("Hasta sonucu paneli gösterildi.");
    }

    public void ContinueToNextPatient()
    {
        Debug.Log("ContinueToNextPatient çalıştı");

        if (endPanel != null)
            endPanel.SetActive(false);

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        if (patientMovement != null)
        {
            patientMovement.HidePatient();
        }
        else if (patientObject != null)
        {
            patientObject.SetActive(false);
        }

        currentCase = null;
        caseFinished = false;
        currentPatientIndex++;

        UpdateHUD();

        if (currentPatientIndex >= dayCases.Count)
        {
            EndDay();
            return;
        }

        SendNextPatient();
    }

    void EndDay()
    {
        Debug.Log("EndDay çalıştı");

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        if (patientMovement != null)
        {
            patientMovement.HidePatient();
        }
        else if (patientObject != null)
        {
            patientObject.SetActive(false);
        }

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endPanelText != null)
        {
            endPanelText.text =
                "Gün Tamamlandı\n\n" +
                "Bugünkü tüm hastalar bitti.\n" +
                "Toplam yanlış teşhis: " + wrongDiagnosisCount + "/" + maxWrongDiagnosis;
        }

        UpdateHUD();
    }

    public void GameOver()
    {
        Debug.Log("GameOver çalıştı");

        if (dialogueManager != null)
            dialogueManager.CloseDialoguePanel();

        if (patientMovement != null)
        {
            patientMovement.HidePatient();
        }
        else if (patientObject != null)
        {
            patientObject.SetActive(false);
        }

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
        {
            int totalPatients = Mathf.Max(dayCases.Count, 1);
            int shownIndex = Mathf.Clamp(currentPatientIndex + 1, 1, totalPatients);
            patientText.text = "Hasta: " + shownIndex + "/" + totalPatients;
        }

        if (wrongCountText != null)
            wrongCountText.text = "Yanlış: " + wrongDiagnosisCount + "/" + maxWrongDiagnosis;
    }
}