using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public Transform contentRoot;
    public GameObject dialogueLinePrefab;
    public GameObject optionButtonPrefab;
    public ScrollRect scrollRect;

    [Header("Flow")]
    public GameFlowManager gameFlowManager;

    [Header("Case")]
    public PatientCaseData currentCase;

    private GameObject currentOptionsGroup;
    private GameObject lastDoctorHeader;

    private HashSet<string> discoveredTags = new HashSet<string>();
    private HashSet<string> usedHiddenChoices = new HashSet<string>();

    private string currentNodeId;
    private string rootNodeId;

    public void StartDialogue()
    {
        Debug.Log("DialogueManager StartDialogue çalıştı");

        if (currentCase == null)
        {
            Debug.LogError("DialogueManager: Current Case atanmadı.");
            return;
        }

        if (string.IsNullOrEmpty(currentCase.rootNodeId))
        {
            Debug.LogError("DialogueManager: rootNodeId boş.");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueManager: dialoguePanel bağlı değil.");
            return;
        }

        if (contentRoot == null)
        {
            Debug.LogError("DialogueManager: contentRoot bağlı değil.");
            return;
        }

        dialoguePanel.SetActive(true);

        discoveredTags.Clear();
        usedHiddenChoices.Clear();

        rootNodeId = currentCase.rootNodeId;
        currentNodeId = rootNodeId;

        AddDialogueLine(FormatLine("HASTA", currentCase.startText));

        if (!string.IsNullOrEmpty(currentCase.startAssistantText))
        {
            AddDialogueLine(FormatLine("ASİSTAN", currentCase.startAssistantText));
        }

        ShowNode(rootNodeId, false);
    }

    public void SelectChoice(DialogueChoiceData choice)
    {
        Debug.Log("SelectChoice çalıştı");

        if (choice == null)
        {
            Debug.LogError("SelectChoice: choice null.");
            return;
        }

        RemoveDoctorHeader();
        RemoveCurrentOptions();

        if (!string.IsNullOrEmpty(choice.doctorLine))
            AddDialogueLine(FormatLine("DOKTOR", choice.doctorLine));

        if (!string.IsNullOrEmpty(choice.patientReply))
            AddDialogueLine(FormatLine("HASTA", choice.patientReply));

        if (!string.IsNullOrEmpty(choice.assistantReply))
            AddDialogueLine(FormatLine("ASİSTAN", choice.assistantReply));

        if (choice.gainedTags != null)
        {
            foreach (string tag in choice.gainedTags)
            {
                if (!string.IsNullOrEmpty(tag))
                    discoveredTags.Add(tag);
            }
        }

        if (choice.hideAfterUse)
        {
            string key = BuildChoiceKey(currentNodeId, choice.choiceText);
            usedHiddenChoices.Add(key);
        }

        if (choice.opensDiagnosisPanel)
        {
            OpenDiagnosisPanel();
            return;
        }

        if (choice.returnToHub)
        {
            ShowNode(rootNodeId, false);
            return;
        }

        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            ShowNode(choice.nextNodeId, true);
            return;
        }

        ShowNode(rootNodeId, false);
    }

    public void SelectDiagnosis(DiagnosisChoiceData diagnosis)
    {
        if (diagnosis == null)
        {
            Debug.LogError("SelectDiagnosis: diagnosis null.");
            return;
        }

        Debug.Log("SelectDiagnosis çalıştı: " + diagnosis.diagnosisName);

        RemoveDoctorHeader();
        RemoveCurrentOptions();

        AddDialogueLine(FormatLine("DOKTOR", "Sizin hastalığınızın " + diagnosis.diagnosisName + " olduğunu düşünüyorum."));

        if (gameFlowManager != null)
        {
            gameFlowManager.SubmitDiagnosis(diagnosis.diagnosisName);
        }
        else
        {
            Debug.LogError("DialogueManager: gameFlowManager bağlı değil.");
            AddDialogueLine(FormatLine("SİSTEM", "GameFlowManager bağlı değil."));
        }

        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottom());
    }

    void OpenDiagnosisPanel()
    {
        Debug.Log("OpenDiagnosisPanel çalıştı");

        if (currentCase == null)
        {
            Debug.LogError("OpenDiagnosisPanel: currentCase null.");
            return;
        }

        if (currentCase.diagnosisChoices == null || currentCase.diagnosisChoices.Count == 0)
        {
            AddDialogueLine(FormatLine("SİSTEM", "Teşhis seçenekleri tanımlanmamış."));
            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToBottom());
            return;
        }

        RemoveDoctorHeader();

        lastDoctorHeader = AddDialogueLineReturn(FormatLine("DOKTOR", ""), false);

        CreateOptionsGroup();

        foreach (DiagnosisChoiceData diagnosis in currentCase.diagnosisChoices)
        {
            GameObject optionObj = Instantiate(optionButtonPrefab, currentOptionsGroup.transform);
            DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();

            if (optionScript != null)
            {
                optionScript.SetupDiagnosisChoice(diagnosis, this);
            }
            else
            {
                Debug.LogError("OpenDiagnosisPanel: optionButtonPrefab içinde DialogueOptionButton yok.");
            }
        }

        StartCoroutine(ScrollToBottom());
    }

    void ShowNode(string nodeId, bool showNodeIntro)
    {
        Debug.Log("ShowNode çalıştı: " + nodeId);

        DialogueNodeData node = GetNodeById(nodeId);

        if (node == null)
        {
            Debug.LogError("Node bulunamadı: " + nodeId);
            return;
        }

        currentNodeId = nodeId;

        if (showNodeIntro)
        {
            if (!string.IsNullOrEmpty(node.nodeIntroText))
                AddDialogueLine(FormatLine("SİSTEM", node.nodeIntroText));

            if (!string.IsNullOrEmpty(node.nodeAssistantText))
                AddDialogueLine(FormatLine("ASİSTAN", node.nodeAssistantText));
        }

        ShowChoices(node);
    }

    void ShowChoices(DialogueNodeData node)
    {
        Debug.Log("ShowChoices çalıştı");

        RemoveDoctorHeader();

        lastDoctorHeader = AddDialogueLineReturn(FormatLine("DOKTOR", ""), false);

        CreateOptionsGroup();

        foreach (DialogueChoiceData choice in GetAvailableChoices(node))
        {
            GameObject optionObj = Instantiate(optionButtonPrefab, currentOptionsGroup.transform);
            DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();

            if (optionScript != null)
            {
                optionScript.SetupChoice(choice, this);
            }
            else
            {
                Debug.LogError("ShowChoices: optionButtonPrefab içinde DialogueOptionButton yok.");
            }
        }

        StartCoroutine(ScrollToBottom());
    }

    List<DialogueChoiceData> GetAvailableChoices(DialogueNodeData node)
    {
        List<DialogueChoiceData> result = new List<DialogueChoiceData>();

        if (node == null || node.choices == null)
            return result;

        foreach (DialogueChoiceData choice in node.choices)
        {
            if (choice == null)
                continue;

            if (!choice.reusable && choice.hideAfterUse)
            {
                string key = BuildChoiceKey(node.nodeId, choice.choiceText);
                if (usedHiddenChoices.Contains(key))
                    continue;
            }

            if (!AreRequiredTagsMet(choice.requiredTags))
                continue;

            if (HasAnyBlockedTag(choice.blockedTags))
                continue;

            result.Add(choice);
        }

        return result;
    }

    bool AreRequiredTagsMet(List<string> requiredTags)
    {
        if (requiredTags == null || requiredTags.Count == 0)
            return true;

        foreach (string tag in requiredTags)
        {
            if (!string.IsNullOrEmpty(tag) && !discoveredTags.Contains(tag))
                return false;
        }

        return true;
    }

    bool HasAnyBlockedTag(List<string> blockedTags)
    {
        if (blockedTags == null || blockedTags.Count == 0)
            return false;

        foreach (string tag in blockedTags)
        {
            if (!string.IsNullOrEmpty(tag) && discoveredTags.Contains(tag))
                return true;
        }

        return false;
    }

    DialogueNodeData GetNodeById(string nodeId)
    {
        if (currentCase == null || currentCase.allNodes == null)
            return null;

        return currentCase.allNodes.FirstOrDefault(n => n != null && n.nodeId == nodeId);
    }

    string BuildChoiceKey(string nodeId, string choiceText)
    {
        return nodeId + "::" + choiceText;
    }

    void RemoveDoctorHeader()
    {
        if (lastDoctorHeader != null)
        {
            Destroy(lastDoctorHeader);
            lastDoctorHeader = null;
        }
    }

    string FormatLine(string speaker, string text)
    {
        switch (speaker)
        {
            case "HASTA":
                return $"<size=26><color=#FFFFFF><b>HASTA —</b></color> {text}</size>";

            case "DOKTOR":
                if (string.IsNullOrWhiteSpace(text))
                    return $"<size=25><color=#FF8C42><b>DOKTOR —</b></color></size>";
                return $"<size=25><color=#FF8C42><b>DOKTOR —</b></color> {text}</size>";

            case "ASİSTAN":
                return $"<size=22><color=#CFCFCF><i>ASİSTAN — {text}</i></color></size>";

            case "SİSTEM":
                return $"<size=21><color=#AAAAAA><i>SİSTEM — {text}</i></color></size>";

            default:
                return text;
        }
    }

    void AddDialogueLine(string text, bool autoScroll = true)
    {
        if (dialogueLinePrefab == null)
        {
            Debug.LogError("AddDialogueLine: dialogueLinePrefab bağlı değil.");
            return;
        }

        GameObject lineObj = Instantiate(dialogueLinePrefab, contentRoot);
        TextMeshProUGUI lineText = lineObj.GetComponentInChildren<TextMeshProUGUI>();

        if (lineText != null)
            lineText.text = text;
        else
            Debug.LogError("AddDialogueLine: prefab içinde TextMeshProUGUI bulunamadı.");

        if (autoScroll)
            StartCoroutine(ScrollToBottom());
    }

    GameObject AddDialogueLineReturn(string text, bool autoScroll = true)
    {
        if (dialogueLinePrefab == null)
        {
            Debug.LogError("AddDialogueLineReturn: dialogueLinePrefab bağlı değil.");
            return null;
        }

        GameObject lineObj = Instantiate(dialogueLinePrefab, contentRoot);
        TextMeshProUGUI lineText = lineObj.GetComponentInChildren<TextMeshProUGUI>();

        if (lineText != null)
            lineText.text = text;
        else
            Debug.LogError("AddDialogueLineReturn: prefab içinde TextMeshProUGUI bulunamadı.");

        if (autoScroll)
            StartCoroutine(ScrollToBottom());

        return lineObj;
    }

    void CreateOptionsGroup()
    {
        RemoveCurrentOptions();

        currentOptionsGroup = new GameObject("OptionsGroup");
        currentOptionsGroup.transform.SetParent(contentRoot, false);

        VerticalLayoutGroup layout = currentOptionsGroup.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.padding.top = 0;
        layout.padding.bottom = 0;
        layout.padding.left = 0;
        layout.padding.right = 0;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = currentOptionsGroup.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
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
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}