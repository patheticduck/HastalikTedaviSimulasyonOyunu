using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueChoiceData
{
    [Header("Buton Metni")]
    public string choiceText;

    [Header("Tür")]
    public ChoiceType choiceType = ChoiceType.Question;

    [Header("Konuşma Akışı")]
    [TextArea]
    public string doctorLine;

    [TextArea]
    public string patientReply;

    [TextArea]
    public string assistantReply;

    [Header("Geçiş")]
    public string nextNodeId;
    public bool returnToHub;
    public bool opensDiagnosisPanel;

    [Header("Davranış")]
    public bool reusable = true;
    public bool hideAfterUse = false;

    [Header("Tag Sistemi")]
    public List<string> gainedTags = new List<string>();
    public List<string> requiredTags = new List<string>();
    public List<string> blockedTags = new List<string>();

    [Header("Belirti / Test Kayıtları")]
    public List<string> discoveredSymptoms = new List<string>();
    public List<string> performedTests = new List<string>();

    [Header("Notebook Kayıtları (Türkçe)")]
    public List<string> notebookSymptoms = new List<string>();
    public List<string> notebookTests = new List<string>();
    public List<string> notebookNotes = new List<string>();

    [Header("Disease Scoring")]
    public List<DiseaseScoreEffect> diseaseEffects = new List<DiseaseScoreEffect>();
}