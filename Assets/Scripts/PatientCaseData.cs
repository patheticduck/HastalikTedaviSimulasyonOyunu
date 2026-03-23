using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPatientCase", menuName = "Diagnosis/Patient Case")]
public class PatientCaseData : ScriptableObject
{
    public string patientName;
    public string correctDiagnosis;

    [TextArea]
    public string startText;

    [TextArea]
    public string startAssistantText;

    public List<QuestionOption> initialOptions;
}