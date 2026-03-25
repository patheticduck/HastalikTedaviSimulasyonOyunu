using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueChoiceData
{
    [Header("Buton Metni")]
    public string choiceText;

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
    public bool reusable = true;     // Aynı seçenek tekrar seçilebilsin mi
    public bool hideAfterUse = false; // Bir kez seçildikten sonra gizlensin mi

    [Header("Tag Sistemi")]
    public List<string> gainedTags = new List<string>();
    public List<string> requiredTags = new List<string>();
    public List<string> blockedTags = new List<string>();
}