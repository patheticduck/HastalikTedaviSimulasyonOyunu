using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNodeData
{
    [Header("Node Kimliği")]
    public string nodeId;

    [Header("Node Ayarları")]
    public bool isHubNode; // Ana menü gibi davranır

    [TextArea]
    public string nodeIntroText; // Bu node'a girince üstte gösterilecek ek metin

    [TextArea]
    public string nodeAssistantText; // Bu node'a özel asistan metni

    [Header("Seçenekler")]
    public List<DialogueChoiceData> choices = new List<DialogueChoiceData>();
}