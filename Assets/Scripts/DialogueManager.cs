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

    private Transform questionsContainer;
    private Transform testsContainer;
    private Transform diagnosisContainer;

    private GameObject lastConversationLine;
    private GameObject currentTurnFocusLine;

    private HashSet<string> discoveredTags = new HashSet<string>();
    private HashSet<string> usedHiddenChoices = new HashSet<string>();

    private HashSet<string> discoveredSymptoms = new HashSet<string>();
    private HashSet<string> performedTests = new HashSet<string>();
    private HashSet<string> shownAssistantHints = new HashSet<string>();

    private Dictionary<string, int> diseaseScores = new Dictionary<string, int>();

    private int askedQuestionCount = 0;
    private int performedTestCount = 0;
    private int totalStepCount = 0;

    private string currentNodeId;
    private string rootNodeId;

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void StartDialogue()
    {
        Debug.Log("DialogueManager -> StartDialogue çalıştı");

        if (currentCase == null)
        {
            Debug.LogError("DialogueManager: currentCase atanmadı.");
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

        if (dialogueLinePrefab == null)
        {
            Debug.LogError("DialogueManager: dialogueLinePrefab bağlı değil.");
            return;
        }

        if (optionButtonPrefab == null)
        {
            Debug.LogError("DialogueManager: optionButtonPrefab bağlı değil.");
            return;
        }

        ClearDialogueContent();

        dialoguePanel.SetActive(true);

        discoveredTags.Clear();
        usedHiddenChoices.Clear();
        discoveredSymptoms.Clear();
        performedTests.Clear();
        shownAssistantHints.Clear();
        diseaseScores.Clear();

        askedQuestionCount = 0;
        performedTestCount = 0;
        totalStepCount = 0;

        rootNodeId = currentCase.rootNodeId;
        currentNodeId = rootNodeId;

        currentTurnFocusLine = AddDialogueLineReturn(FormatLine("HASTA", currentCase.startText), false);

        if (!string.IsNullOrEmpty(currentCase.startAssistantText))
        {
            AddDialogueLine(FormatLine("ASİSTAN", currentCase.startAssistantText), false);
        }

        ShowNode(rootNodeId, false);

        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToConversationFocus());
    }

    public void CloseDialoguePanel()
    {
        ClearDialogueContent();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void SelectChoice(DialogueChoiceData choice)
    {
        Debug.Log("DialogueManager -> SelectChoice çalıştı");

        if (choice == null)
        {
            Debug.LogError("SelectChoice: choice null.");
            return;
        }

        totalStepCount++;

        if (choice.choiceType == ChoiceType.Question)
            askedQuestionCount++;

        if (choice.choiceType == ChoiceType.Test)
            performedTestCount++;

        RemoveCurrentOptions();

        currentTurnFocusLine = null;

        if (!string.IsNullOrEmpty(choice.doctorLine))
            currentTurnFocusLine = AddDialogueLineReturn(FormatLine("DOKTOR", choice.doctorLine), false);

        if (!string.IsNullOrEmpty(choice.patientReply))
        {
            GameObject patientLine = AddDialogueLineReturn(FormatLine("HASTA", choice.patientReply), false);

            if (currentTurnFocusLine == null)
                currentTurnFocusLine = patientLine;
        }

        if (!string.IsNullOrEmpty(choice.assistantReply))
            AddDialogueLine(FormatLine("ASİSTAN", choice.assistantReply), false);

        if (choice.gainedTags != null)
        {
            foreach (string tag in choice.gainedTags)
            {
                if (!string.IsNullOrEmpty(tag))
                    discoveredTags.Add(tag);
            }
        }

        if (choice.discoveredSymptoms != null)
        {
            foreach (string symptom in choice.discoveredSymptoms)
            {
                if (!string.IsNullOrEmpty(symptom))
                    discoveredSymptoms.Add(symptom);
            }
        }

        if (choice.performedTests != null)
        {
            foreach (string test in choice.performedTests)
            {
                if (!string.IsNullOrEmpty(test))
                    performedTests.Add(test);
            }
        }

        ApplyDiseaseEffects(choice.diseaseEffects);

        if (choice.hideAfterUse)
        {
            string key = BuildChoiceKey(currentNodeId, choice.choiceText);
            usedHiddenChoices.Add(key);
        }

        TryShowAssistantHints();

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

        Debug.Log("DialogueManager -> SelectDiagnosis çalıştı: " + diagnosis.diagnosisName);

        RemoveCurrentOptions();

        currentTurnFocusLine = AddDialogueLineReturn(
            FormatLine("DOKTOR", "Sizin hastalığınızın " + diagnosis.diagnosisName + " olduğunu düşünüyorum."),
            false
        );

        DebugTopDiseaseScores();

        if (gameFlowManager != null)
        {
            gameFlowManager.SubmitDiagnosis(diagnosis.diseaseId);
        }
        else
        {
            Debug.LogError("DialogueManager: gameFlowManager bağlı değil.");
            AddDialogueLine(FormatLine("SİSTEM", "GameFlowManager bağlı değil."), false);
        }

        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToConversationFocus());
    }

    private void OpenDiagnosisPanel()
    {
        Debug.Log("DialogueManager -> OpenDiagnosisPanel çalıştı");

        if (currentCase == null)
        {
            Debug.LogError("OpenDiagnosisPanel: currentCase null.");
            return;
        }

        if (currentCase.diagnosisChoices == null || currentCase.diagnosisChoices.Count == 0)
        {
            AddDialogueLine(FormatLine("SİSTEM", "Teşhis seçenekleri tanımlanmamış."));
            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToConversationFocus());
            return;
        }

        if (askedQuestionCount < 3)
        {
            AddDialogueLine(FormatLine("SİSTEM", "Henüz yeterli anamnez toplanmamış olabilir."), false);
        }

        if (performedTestCount == 0)
        {
            AddDialogueLine(FormatLine("SİSTEM", "En az bir muayene veya test tanıyı netleştirebilir."), false);
        }

        DebugTopDiseaseScores();

        RemoveCurrentOptions();
        CreateOptionsGroup();

        CreateSectionHeaderAndContainer("Tanı", out diagnosisContainer);

        foreach (DiagnosisChoiceData diagnosis in currentCase.diagnosisChoices)
        {
            if (diagnosis == null)
                continue;

            GameObject optionObj = Instantiate(optionButtonPrefab, diagnosisContainer);
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

        StartCoroutine(ScrollToConversationFocus());
    }

    private void ShowNode(string nodeId, bool showNodeIntro)
    {
        Debug.Log("DialogueManager -> ShowNode çalıştı: " + nodeId);

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
                AddDialogueLine(FormatLine("SİSTEM", node.nodeIntroText), false);

            if (!string.IsNullOrEmpty(node.nodeAssistantText))
                AddDialogueLine(FormatLine("ASİSTAN", node.nodeAssistantText), false);
        }

        ShowChoices(node);
    }

    private void ShowChoices(DialogueNodeData node)
    {
        Debug.Log("DialogueManager -> ShowChoices çalıştı");

        List<DialogueChoiceData> availableChoices = GetAvailableChoices(node);

        RemoveCurrentOptions();

        if (availableChoices.Count == 0)
        {
            AddDialogueLine(FormatLine("SİSTEM", "Şu anda kullanılabilir seçenek yok."));
            StartCoroutine(ScrollToConversationFocus());
            return;
        }

        List<DialogueChoiceData> questionChoices = availableChoices
            .Where(c => c.choiceType == ChoiceType.Question || c.choiceType == ChoiceType.General)
            .ToList();

        List<DialogueChoiceData> testChoices = availableChoices
            .Where(c => c.choiceType == ChoiceType.Test)
            .ToList();

        List<DialogueChoiceData> diagnosisChoices = availableChoices
            .Where(c => c.choiceType == ChoiceType.Diagnosis)
            .ToList();

        CreateOptionsGroup();

        if (questionChoices.Count > 0)
        {
            CreateSectionHeaderAndContainer("Sorular", out questionsContainer);

            foreach (DialogueChoiceData choice in questionChoices)
                CreateChoiceButton(choice, questionsContainer);
        }

        if (testChoices.Count > 0)
        {
            CreateSectionHeaderAndContainer("Testler", out testsContainer);

            foreach (DialogueChoiceData choice in testChoices)
                CreateChoiceButton(choice, testsContainer);
        }

        if (diagnosisChoices.Count > 0)
        {
            CreateSectionHeaderAndContainer("Tanı", out diagnosisContainer);

            foreach (DialogueChoiceData choice in diagnosisChoices)
                CreateChoiceButton(choice, diagnosisContainer);
        }

        StartCoroutine(ScrollToConversationFocus());
    }

    private void CreateChoiceButton(DialogueChoiceData choice, Transform parent)
    {
        GameObject optionObj = Instantiate(optionButtonPrefab, parent);
        DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();

        if (optionScript != null)
        {
            optionScript.SetupChoice(choice, this);
        }
        else
        {
            Debug.LogError("CreateChoiceButton: optionButtonPrefab içinde DialogueOptionButton yok.");
        }
    }

    private List<DialogueChoiceData> GetAvailableChoices(DialogueNodeData node)
    {
        List<DialogueChoiceData> result = new List<DialogueChoiceData>();

        if (node == null || node.choices == null)
            return result;

        foreach (DialogueChoiceData choice in node.choices)
        {
            if (choice == null)
                continue;

            if (choice.hideAfterUse)
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

    private void ApplyDiseaseEffects(List<DiseaseScoreEffect> effects)
    {
        if (effects == null || effects.Count == 0)
            return;

        foreach (DiseaseScoreEffect effect in effects)
        {
            if (effect == null || string.IsNullOrEmpty(effect.diseaseId))
                continue;

            if (!diseaseScores.ContainsKey(effect.diseaseId))
                diseaseScores[effect.diseaseId] = 0;

            diseaseScores[effect.diseaseId] += effect.score;

            Debug.Log("Disease Score Updated -> " + effect.diseaseId + ": " + diseaseScores[effect.diseaseId]);
        }
    }

    private void TryShowAssistantHints()
    {
        if (currentCase == null || currentCase.assistantHints == null || currentCase.assistantHints.Count == 0)
            return;

        foreach (AssistantHintRuleData hint in currentCase.assistantHints)
        {
            if (hint == null)
                continue;

            if (hint.showOnlyOnce && shownAssistantHints.Contains(hint.hintId))
                continue;

            if (askedQuestionCount < hint.minAskedQuestions)
                continue;

            if (!AreRequiredTagsMet(hint.requiredTags))
                continue;

            if (HasAnyBlockedTag(hint.blockedTags))
                continue;

            AddDialogueLine(FormatLine("ASİSTAN", hint.hintText), false);
            shownAssistantHints.Add(hint.hintId);

            if (hint.showOnlyOnce)
                break;
        }
    }

    private bool AreRequiredTagsMet(List<string> requiredTags)
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

    private bool HasAnyBlockedTag(List<string> blockedTags)
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

    private DialogueNodeData GetNodeById(string nodeId)
    {
        if (currentCase == null || currentCase.allNodes == null)
            return null;

        return currentCase.allNodes.FirstOrDefault(n => n != null && n.nodeId == nodeId);
    }

    private string BuildChoiceKey(string nodeId, string choiceText)
    {
        return nodeId + "::" + choiceText;
    }

    private void DebugTopDiseaseScores()
    {
        if (diseaseScores == null || diseaseScores.Count == 0)
        {
            Debug.Log("Henüz disease score oluşmadı.");
            return;
        }

        var sortedScores = diseaseScores
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        string debugText = "Top Disease Scores -> ";
        for (int i = 0; i < sortedScores.Count; i++)
        {
            debugText += sortedScores[i].Key + ": " + sortedScores[i].Value;
            if (i < sortedScores.Count - 1)
                debugText += " | ";
        }

        Debug.Log(debugText);
    }

    public int GetDiseaseScore(string diseaseId)
    {
        if (string.IsNullOrEmpty(diseaseId))
            return 0;

        if (diseaseScores.ContainsKey(diseaseId))
            return diseaseScores[diseaseId];

        return 0;
    }

    public Dictionary<string, int> GetAllDiseaseScores()
    {
        return new Dictionary<string, int>(diseaseScores);
    }

    public HashSet<string> GetDiscoveredTags()
    {
        return new HashSet<string>(discoveredTags);
    }

    public HashSet<string> GetDiscoveredSymptoms()
    {
        return new HashSet<string>(discoveredSymptoms);
    }

    public HashSet<string> GetPerformedTests()
    {
        return new HashSet<string>(performedTests);
    }

    public int GetAskedQuestionCount()
    {
        return askedQuestionCount;
    }

    public int GetPerformedTestCount()
    {
        return performedTestCount;
    }

    public int GetTotalStepCount()
    {
        return totalStepCount;
    }

    private string FormatLine(string speaker, string text)
    {
        switch (speaker)
        {
            case "HASTA":
                return $"<size=26><color=#FFFFFF><b>HASTA —</b></color> {text}</size>";

            case "DOKTOR":
                return $"<size=25><color=#FF8C42><b>DOKTOR —</b></color> {text}</size>";

            case "ASİSTAN":
                return $"<size=22><color=#CFCFCF><i>ASİSTAN — {text}</i></color></size>";

            case "SİSTEM":
                return $"<size=21><color=#AAAAAA><i>SİSTEM — {text}</i></color></size>";

            default:
                return text;
        }
    }

    private void AddDialogueLine(string text, bool autoScroll = true)
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

        lastConversationLine = lineObj;

        if (autoScroll)
            StartCoroutine(ScrollToConversationFocus());
    }

    private GameObject AddDialogueLineReturn(string text, bool autoScroll = true)
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

        lastConversationLine = lineObj;

        if (autoScroll)
            StartCoroutine(ScrollToConversationFocus());

        return lineObj;
    }

    private void CreateOptionsGroup()
    {
        RemoveCurrentOptions();

        currentOptionsGroup = new GameObject("OptionsGroup");
        currentOptionsGroup.transform.SetParent(contentRoot, false);

        RectTransform rect = currentOptionsGroup.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        VerticalLayoutGroup layout = currentOptionsGroup.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.padding.top = 6;
        layout.padding.bottom = 6;
        layout.padding.left = 0;
        layout.padding.right = 0;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = currentOptionsGroup.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        questionsContainer = null;
        testsContainer = null;
        diagnosisContainer = null;
    }

    private void CreateSectionHeaderAndContainer(string title, out Transform sectionContainer)
    {
        CreateSectionHeader(title);
        CreateDivider();

        GameObject containerObj = new GameObject(title + "Container");
        containerObj.transform.SetParent(currentOptionsGroup.transform, false);

        RectTransform rect = containerObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.padding = new RectOffset(0, 0, 0, 4);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = containerObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        sectionContainer = containerObj.transform;
    }

    private void CreateSectionHeader(string title)
    {
        GameObject headerObj = new GameObject(title + "Header");
        headerObj.transform.SetParent(currentOptionsGroup.transform, false);

        RectTransform rect = headerObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        LayoutElement layoutElement = headerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 26f;
        layoutElement.preferredHeight = 26f;

        TextMeshProUGUI text = headerObj.AddComponent<TextMeshProUGUI>();
        text.text = title.ToUpper();
        text.fontSize = 20;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color32(220, 220, 220, 255);
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = false;
    }

    private void CreateDivider()
    {
        GameObject dividerObj = new GameObject("Divider");
        dividerObj.transform.SetParent(currentOptionsGroup.transform, false);

        RectTransform rect = dividerObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        LayoutElement layoutElement = dividerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 2f;
        layoutElement.preferredHeight = 2f;

        Image image = dividerObj.AddComponent<Image>();
        image.color = new Color32(90, 90, 90, 255);
    }

    private void RemoveCurrentOptions()
    {
        if (currentOptionsGroup != null)
        {
            Destroy(currentOptionsGroup);
            currentOptionsGroup = null;
        }

        questionsContainer = null;
        testsContainer = null;
        diagnosisContainer = null;
    }

    private void ClearDialogueContent()
    {
        RemoveCurrentOptions();
        lastConversationLine = null;
        currentTurnFocusLine = null;

        if (contentRoot != null)
        {
            foreach (Transform child in contentRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private IEnumerator ScrollToConversationFocus()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        if (scrollRect == null)
            yield break;

        RectTransform viewport = scrollRect.viewport != null
            ? scrollRect.viewport
            : scrollRect.GetComponent<RectTransform>();

        if (viewport == null || contentRect == null)
            yield break;

        GameObject focusObj = currentTurnFocusLine != null ? currentTurnFocusLine : lastConversationLine;
        if (focusObj == null)
            yield break;

        RectTransform focusRect = focusObj.GetComponent<RectTransform>();
        if (focusRect == null)
            yield break;

        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 0f;
            yield break;
        }

        float focusTop = Mathf.Abs(focusRect.anchoredPosition.y);

        float topPadding = 50f;

        float scrollY = Mathf.Clamp(focusTop - topPadding, 0f, contentHeight - viewportHeight);
        float normalized = 1f - (scrollY / (contentHeight - viewportHeight));

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = normalized;
    }
}