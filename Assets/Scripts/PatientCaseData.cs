using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPatientCase", menuName = "Diagnosis/Patient Case")]
public class PatientCaseData : ScriptableObject
{
    [Header("Hasta Bilgileri")]
    public string patientName;
    public string correctDiagnosis;

    [Header("Başlangıç")]
    [TextArea]
    public string startText;

    [TextArea]
    public string startAssistantText;

    [Header("Akış")]
    public string rootNodeId;

    public List<DialogueNodeData> allNodes = new List<DialogueNodeData>();

    [Header("Teşhis Seçenekleri")]
    public List<DiagnosisChoiceData> diagnosisChoices = new List<DiagnosisChoiceData>();
}