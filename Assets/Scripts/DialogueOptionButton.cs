using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DialogueOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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

        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();
    }

    private void Reset()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    public void SetupChoice(DialogueChoiceData choice, DialogueManager manager)
    {
        Debug.Log("SetupChoice çalıştı: " + (choice != null ? choice.choiceText : "NULL"));

        choiceData = choice;
        diagnosisData = null;
        dialogueManager = manager;

        EnsureReferences();

        if (buttonText != null && choice != null)
        {
            buttonText.text = choice.choiceText;
            ApplyNormalColor();
            buttonText.raycastTarget = false;
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
            button.interactable = true;
        }
        else
        {
            Debug.LogError("SetupChoice: Button component bulunamadı.");
        }
    }

    public void SetupDiagnosisChoice(DiagnosisChoiceData diagnosis, DialogueManager manager)
    {
        Debug.Log("SetupDiagnosisChoice çalıştı: " + (diagnosis != null ? diagnosis.diagnosisName : "NULL"));

        diagnosisData = diagnosis;
        choiceData = null;
        dialogueManager = manager;

        EnsureReferences();

        if (buttonText != null && diagnosis != null)
        {
            buttonText.text = diagnosis.diagnosisName;
            ApplyNormalColor();
            buttonText.raycastTarget = false;
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
            button.interactable = true;
        }
        else
        {
            Debug.LogError("SetupDiagnosisChoice: Button component bulunamadı.");
        }
    }

    private void OnClickChoice()
    {
        Debug.Log("Choice butonuna tıklandı: " + (choiceData != null ? choiceData.choiceText : "NULL"));

        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();

        if (button != null)
            button.interactable = false;

        if (dialogueManager != null && choiceData != null)
        {
            dialogueManager.SelectChoice(choiceData);
        }
        else
        {
            Debug.LogError("Choice click: dialogueManager veya choiceData null");
        }
    }

    private void OnClickDiagnosisChoice()
    {
        Debug.Log("Diagnosis butonuna tıklandı: " + (diagnosisData != null ? diagnosisData.diagnosisName : "NULL"));

        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();

        if (button != null)
            button.interactable = false;

        if (dialogueManager != null && diagnosisData != null)
        {
            dialogueManager.SelectDiagnosis(diagnosisData);
        }
        else
        {
            Debug.LogError("Diagnosis click: dialogueManager veya diagnosisData null");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("IPointerClick alındı: " + gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHoverColor();

        if (choiceData != null &&
            choiceData.choiceType == ChoiceType.Test &&
            choiceData.useTestChance &&
            TestChanceTooltipUI.Instance != null)
        {
            TestChanceTooltipUI.Instance.Show(choiceData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormalColor();

        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();
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