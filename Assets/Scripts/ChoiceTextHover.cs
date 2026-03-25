using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ChoiceTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI label;

    [SerializeField] private Color normalColor = new Color32(212, 122, 50, 255);
    [SerializeField] private Color hoverColor = new Color32(148, 78, 28, 255);

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