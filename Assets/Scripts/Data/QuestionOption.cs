using UnityEngine;

[System.Serializable]
public class QuestionOption
{
    public string questionText;     
    public string patientResponse;   
    public string infoTag;           

    public bool triggersAssistant;
    [TextArea]
    public string assistantText;

    public bool endsDialogue;        
}