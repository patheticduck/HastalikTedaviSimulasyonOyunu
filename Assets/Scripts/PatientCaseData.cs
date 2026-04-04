using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPatientCase", menuName = "Game/Patient Case")]
public class PatientCaseData : ScriptableObject
{
    public string rootNodeId;

    [TextArea]
    public string startText;

    [TextArea]
    public string startAssistantText;

    public List<DialogueNodeData> allNodes;

    public List<DiagnosisChoiceData> diagnosisChoices;
}