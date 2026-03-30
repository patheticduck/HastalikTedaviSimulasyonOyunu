using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DialogueOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public TextMeshProUGUI buttonText;

    [Header("Colors")]
    public Color normalColor = new Color32(190, 110, 45, 255);
    public Color hoverColor = new Color32(140, 75, 25, 255);

    [HideInInspector] public DialogueChoiceData choiceData;
    [HideInInspector] public DiagnosisChoiceData diagnosisData;

    private DialogueManager dialogueManager;
    private Button button;

    public void SetupChoice(DialogueChoiceData choice, DialogueManager manager)
    {
        Debug.Log("SetupChoice çalıştı");

        choiceData = choice;
        diagnosisData = null;
        dialogueManager = manager;

        EnsureReferences();

        if (buttonText != null && choice != null)
        {
            buttonText.text = "- " + choice.choiceText;
            ApplyNormalColor();
        }
        else
        {
            Debug.LogError("SetupChoice: buttonText veya choice null.");
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickChoice);
            button.transition = Selectable.Transition.None;
        }
        else
        {
            Debug.LogError("SetupChoice: Button component bulunamadı.");
        }
    }

    public void SetupDiagnosisChoice(DiagnosisChoiceData diagnosis, DialogueManager manager)
    {
        Debug.Log("SetupDiagnosisChoice çalıştı");

        diagnosisData = diagnosis;
        choiceData = null;
        dialogueManager = manager;

        EnsureReferences();

        if (buttonText != null && diagnosis != null)
        {
            buttonText.text = "- " + diagnosis.diagnosisName;
            ApplyNormalColor();
        }
        else
        {
            Debug.LogError("SetupDiagnosisChoice: buttonText veya diagnosis null.");
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickDiagnosisChoice);
            button.transition = Selectable.Transition.None;
        }
        else
        {
            Debug.LogError("SetupDiagnosisChoice: Button component bulunamadı.");
        }
    }

    void OnClickChoice()
    {
        Debug.Log("Choice butonuna tıklandı");

        if (dialogueManager != null && choiceData != null)
            dialogueManager.SelectChoice(choiceData);
        else
            Debug.LogError("Choice click: dialogueManager veya choiceData null");
    }

    void OnClickDiagnosisChoice()
    {
        Debug.Log("Diagnosis butonuna tıklandı");

        if (dialogueManager != null && diagnosisData != null)
            dialogueManager.SelectDiagnosis(diagnosisData);
        else
            Debug.LogError("Diagnosis click: dialogueManager veya diagnosisData null");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHoverColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormalColor();
    }

    private void Awake()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    private void OnEnable()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    private void OnDisable()
    {
        ApplyNormalColor();
    }

    private void Reset()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    private void EnsureReferences()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button == null)
            button = GetComponent<Button>();
    }

    private void ApplyNormalColor()
    {
        if (buttonText != null)
            buttonText.color = normalColor;
    }

    private void ApplyHoverColor()
    {
        if (buttonText != null)
            buttonText.color = hoverColor;
    }
}