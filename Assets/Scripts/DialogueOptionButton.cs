using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueOptionButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;

    public QuestionOption optionData;  

    private DialogueManager dialogueManager;

    public void Setup(string optionText, string responseText, DialogueManager manager)
    {
        dialogueManager = manager;

        if (buttonText != null)
            buttonText.text = optionText;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClickOption);
    }

    void OnClickOption()
    {
        if (dialogueManager != null && optionData != null)
        {
            dialogueManager.SelectOptionData(optionData);
        }
    }
}