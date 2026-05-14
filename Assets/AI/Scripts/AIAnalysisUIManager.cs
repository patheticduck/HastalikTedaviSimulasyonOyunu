using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIAnalysisUIManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject aiPanel;
    public TMP_Text resultText;
    public Button closeButton;

    [Header("References")]
    public DiagnosisAI diagnosisAI;
    public DialogueManager dialogueManager;

    private Action externalCloseAction;

    private void Start()
    {
        if (diagnosisAI == null)
            diagnosisAI = FindFirstObjectByType<DiagnosisAI>();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (aiPanel != null)
            aiPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void SetExternalCloseAction(Action closeAction)
    {
        externalCloseAction = closeAction;
    }

    public void OpenPanel()
    {
        if (aiPanel != null)
            aiPanel.SetActive(true);

        AnalyzeCurrentPatient();
    }

    public void ClosePanel()
    {
        if (externalCloseAction != null)
        {
            externalCloseAction.Invoke();
            return;
        }

        ClosePanelOnly();
    }

    public void ClosePanelOnly()
    {
        if (aiPanel != null)
            aiPanel.SetActive(false);
    }

    public void AnalyzeCurrentPatient()
    {
        if (diagnosisAI == null)
            diagnosisAI = FindFirstObjectByType<DiagnosisAI>();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (diagnosisAI == null || dialogueManager == null)
        {
            if (resultText != null)
                resultText.text = "AI bağlantısı eksik.";

            Debug.LogWarning("AI bağlantısı eksik.");
            return;
        }

        Debug.Log("AIAnalysis uses DialogueManager Instance: " + dialogueManager.GetInstanceID());

        List<string> features = dialogueManager.GetAIFeatures();

        Debug.Log("AI Current Case: " +
            (dialogueManager.currentCase != null ? dialogueManager.currentCase.trueDiseaseId : "NULL"));

        Debug.Log("AI Feature Count: " + features.Count);

        foreach (string f in features)
            Debug.Log("AI Feature: " + f);

        if (features.Count == 0)
        {
            if (resultText != null)
            {
                resultText.text =
                    "AI Analizi için henüz yeterli veri yok.\n\n" +
                    "Önce hastaya birkaç soru sorun veya test yapın.";
            }

            return;
        }

        List<DiagnosisPrediction> results = diagnosisAI.Predict(features);

        if (results == null || results.Count == 0)
        {
            if (resultText != null)
            {
                resultText.text =
                    "AI tahmin üretemedi.\n\n" +
                    "Model JSON, feature eşleşmesi veya DiagnosisAI bağlantısı kontrol edilmeli.";
            }

            Debug.LogWarning("AI tahmin üretemedi. Results boş döndü.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Olası Tanılar:\n");

        for (int i = 0; i < Mathf.Min(5, results.Count); i++)
        {
            var r = results[i];
            sb.AppendLine($"{i + 1}. {r.disease}  →  %{r.probability * 100f:F1}");
        }

        sb.AppendLine("\nAI Yorumu:");
        sb.AppendLine("Bu analiz, oyuncunun şu ana kadar topladığı semptom ve test bulgularına göre hesaplanmıştır.");

        if (resultText != null)
            resultText.text = sb.ToString();
    }
}