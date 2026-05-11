using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ChoiceTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI label;

    private readonly Color normalColor = new Color32(176, 216, 242, 255);
    private readonly Color hoverColor = new Color32(70, 130, 180, 255);

    private void Reset()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();

        ApplyNormalColor();
    }

    private void OnEnable()
    {
        ApplyNormalColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (label != null)
            label.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormalColor();
    }

    private void OnDisable()
    {
        ApplyNormalColor();
    }

    private void ApplyNormalColor()
    {
        if (label != null)
            label.color = normalColor;
    }
}