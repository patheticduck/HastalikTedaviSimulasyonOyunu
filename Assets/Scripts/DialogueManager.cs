using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;

    public Transform contentRoot;
    public GameObject dialogueLinePrefab;
    public GameObject optionButtonPrefab;
    public ScrollRect scrollRect;

    public PatientCaseData currentCase;

    private GameObject currentOptionsGroup;

    public void StartDialogue()
    {
        dialoguePanel.SetActive(true);

        AddDialogueLine("HASTA — " + currentCase.startText);

        if (!string.IsNullOrEmpty(currentCase.startAssistantText))
        {
            AddDialogueLine("ASİSTAN — " + currentCase.startAssistantText);
        }

        ShowOptions_FromData(currentCase.initialOptions);
    }

    public void SelectOptionData(QuestionOption option)
    {
        RemoveCurrentOptions();

        AddDialogueLine("DOKTOR — " + option.questionText);
        AddDialogueLine("HASTA — " + option.patientResponse);

        if (option.triggersAssistant && !string.IsNullOrEmpty(option.assistantText))
        {
            AddDialogueLine("ASİSTAN — " + option.assistantText);
        }

        if (option.endsDialogue)
        {
            AddDialogueLine("SİSTEM — Teşhis ekranı açılacak.");
            return;
        }

        ShowOptions_FromData(currentCase.initialOptions);
    }

    void ShowOptions_FromData(List<QuestionOption> options)
    {
        CreateOptionsGroup();

        foreach (var option in options)
        {
            GameObject optionObj = Instantiate(optionButtonPrefab, currentOptionsGroup.transform);
            DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();

            if (optionScript != null)
            {
                optionScript.Setup(option.questionText, option.patientResponse, this);
                optionScript.optionData = option;
            }
        }
    }

    void AddDialogueLine(string text)
    {
        GameObject lineObj = Instantiate(dialogueLinePrefab, contentRoot);
        TextMeshProUGUI lineText = lineObj.GetComponent<TextMeshProUGUI>();

        if (lineText != null)
            lineText.text = text;

        StartCoroutine(ScrollToBottom());
    }

    void CreateOptionsGroup()
    {
        RemoveCurrentOptions();

        currentOptionsGroup = new GameObject("OptionsGroup");
        currentOptionsGroup.transform.SetParent(contentRoot, false);

        VerticalLayoutGroup layout = currentOptionsGroup.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = currentOptionsGroup.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        StartCoroutine(ScrollToBottom());
    }

    void RemoveCurrentOptions()
    {
        if (currentOptionsGroup != null)
        {
            Destroy(currentOptionsGroup);
            currentOptionsGroup = null;
        }
    }

    IEnumerator ScrollToBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}