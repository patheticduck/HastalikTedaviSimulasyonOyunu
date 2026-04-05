using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DiagnosisChoiceData
{
    [Header("Kimlik")]
    public string diseaseId;

    [Header("UI")]
    public string diagnosisName;

    [TextArea]
    public string resultText;

    public bool isCorrectDiagnosis;

    [Header("Opsiyonel Koşullar")]
    public List<string> recommendedTags = new List<string>();
}