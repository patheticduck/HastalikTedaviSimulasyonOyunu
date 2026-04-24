using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoctorBookUIManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject doctorBookPanel;

    [Header("Left Page")]
    public TextMeshProUGUI diseaseTitleText;
    public TextMeshProUGUI pageContentText;

    [Header("Right Page")]
    public Image diseaseImage;
    public TextMeshProUGUI extraInfoText;
    public TextMeshProUGUI pageNumberText;

    [Header("Pages")]
    public List<DiseaseBookPageData> pages;

    [Header("Highlight")]
    public string highlightColor = "#FFF176AA";

    private int currentPageIndex = 0;
    private DialogueManager dialogueManager;

    private void Start()
    {
        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);
    }

    public void OpenBook()
    {
        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(true);

        dialogueManager = FindFirstObjectByType<DialogueManager>();

        currentPageIndex = 0;
        ShowCurrentPage();
    }

    public void CloseBook()
    {
        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);
    }

    public void NextPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        currentPageIndex++;

        if (currentPageIndex >= pages.Count)
            currentPageIndex = 0;

        ShowCurrentPage();
    }

    public void PrevPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        currentPageIndex--;

        if (currentPageIndex < 0)
            currentPageIndex = pages.Count - 1;

        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        PrevPage();
    }

    private void ShowCurrentPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        DiseaseBookPageData page = pages[currentPageIndex];

        if (diseaseTitleText != null)
            diseaseTitleText.text = page.diseaseName;

        if (pageContentText != null)
        {
            string content =
                "<b>GENEL BİLGİ</b>\n" + page.generalInfo + "\n\n" +
                "<b>SEMPTOMLAR</b>\n" + page.symptomsInfo + "\n\n" +
                "<b>TANI İPUÇLARI</b>\n" + page.diagnosisTips;

            pageContentText.text = ApplyEvidenceHighlights(content, page);
        }

        if (extraInfoText != null)
            extraInfoText.text = "";

        if (diseaseImage != null)
        {
            diseaseImage.sprite = page.diseaseImage;
            diseaseImage.gameObject.SetActive(page.diseaseImage != null);
        }

        if (pageNumberText != null)
            pageNumberText.text = (currentPageIndex + 1) + " / " + pages.Count;
    }

    private string ApplyEvidenceHighlights(string text, DiseaseBookPageData page)
    {
        if (dialogueManager == null)
            return text;

        HashSet<string> evidenceTexts = new HashSet<string>();

        foreach (string item in dialogueManager.GetNotebookSymptoms())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetNotebookNotes())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetNotebookTests())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetDiscoveredSymptoms())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetPerformedTests())
            evidenceTexts.Add(item);

        Debug.Log("BOOK EvidenceTexts: " + string.Join(" | ", evidenceTexts));
        Debug.Log("BOOK Keywords: " + string.Join(" | ", page.highlightKeywords));

        if (evidenceTexts.Count == 0)
            return text;

        string result = text;

        if (page.highlightKeywords == null)
            return result;

        foreach (string keyword in page.highlightKeywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            if (IsKeywordDiscovered(keyword, evidenceTexts))
                result = HighlightWord(result, keyword);
        }

        return result;
    }

    private bool IsKeywordDiscovered(string keyword, HashSet<string> evidenceTexts)
    {
        string normalizedKeyword = NormalizeText(keyword);

        foreach (string evidence in evidenceTexts)
        {
            string normalizedEvidence = NormalizeText(evidence);

            if (normalizedEvidence.Contains(normalizedKeyword))
                return true;

            if (normalizedKeyword.Contains(normalizedEvidence))
                return true;
        }

        return false;
    }

    private string HighlightWord(string text, string word)
    {
        return text.Replace(
            word,
            $"<mark={highlightColor}>{word}</mark>",
            System.StringComparison.OrdinalIgnoreCase
        );
    }

    private string NormalizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        return input
            .ToLower()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ü", "u")
            .Replace("ç", "c");
    }
}