using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPatientCase", menuName = "Game/Patient Case")]
public class PatientCaseData : ScriptableObject
{
    [Header("Case Identity")]
    public string caseId;
    public string trueDiseaseId;

    [Header("Patient Visual")]
    public int patientVisualIndex;

    [Header("Start")]
    public string rootNodeId;

    [TextArea]
    public string startText;

    [TextArea]
    public string startAssistantText;

    [Header("Dialogue")]
    public List<DialogueNodeData> allNodes = new List<DialogueNodeData>();

    [Header("Diagnosis")]
    public List<DiagnosisChoiceData> diagnosisChoices = new List<DiagnosisChoiceData>();

    [Header("Assistant Hints")]
    public List<AssistantHintRuleData> assistantHints = new List<AssistantHintRuleData>();

    [Header("Random Variation")]
    public CaseVariationProfile variationProfile;
}