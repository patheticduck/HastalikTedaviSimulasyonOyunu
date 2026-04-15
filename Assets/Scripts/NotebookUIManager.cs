using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class NotebookUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject notebookPanel;
    public TextMeshProUGUI symptomsText;
    public TextMeshProUGUI tagsText;
    public TextMeshProUGUI testsText;

    private DialogueManager currentDialogueManager;

    private void Start()
    {
        if (notebookPanel != null)
            notebookPanel.SetActive(false);
    }

    public void SetDialogueManager(DialogueManager manager)
    {
        currentDialogueManager = manager;
    }

    public void OpenNotebookUI()
    {
        RefreshNotebook();

        if (notebookPanel != null)
            notebookPanel.SetActive(true);
    }

    public void CloseNotebookUI()
    {
        if (notebookPanel != null)
            notebookPanel.SetActive(false);
    }

    public void RefreshNotebook()
    {
        if (currentDialogueManager == null)
        {
            Debug.LogWarning("NotebookUIManager -> currentDialogueManager atanmadı.");
            return;
        }

        HashSet<string> symptoms = currentDialogueManager.GetDiscoveredSymptoms();
        HashSet<string> tags = currentDialogueManager.GetDiscoveredTags();
        HashSet<string> tests = currentDialogueManager.GetPerformedTests();

        Debug.Log(
            $"Notebook Refresh -> Symptoms: {symptoms.Count}, Tags: {tags.Count}, Tests: {tests.Count}"
        );

        if (symptomsText != null)
            symptomsText.text = BuildSectionText(symptoms, "Henüz belirti kaydı yok.");

        if (tagsText != null)
            tagsText.text = BuildSectionText(tags, "Henüz ipucu kaydı yok.");

        if (testsText != null)
            testsText.text = BuildSectionText(tests, "Henüz test yapılmadı.");
    }

    private string BuildSectionText(HashSet<string> items, string emptyMessage)
    {
        if (items == null || items.Count == 0)
            return emptyMessage;

        StringBuilder sb = new StringBuilder();

        foreach (string item in items)
        {
            sb.AppendLine("• " + FormatLabel(item));
        }

        return sb.ToString().TrimEnd();
    }

    private string FormatLabel(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "-";

        string formatted = raw.Replace("_", " ");

        if (formatted.Length == 1)
            return formatted.ToUpper();

        return char.ToUpper(formatted[0]) + formatted.Substring(1);
    }
}