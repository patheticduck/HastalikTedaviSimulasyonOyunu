using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HistoryItemUI : MonoBehaviour,
    IScrollHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI")]
    public Button headerButton;
    public TextMeshProUGUI headerText;
    public GameObject detailPanel;
    public TextMeshProUGUI detailText;
    public LayoutElement detailLayoutElement;

    [Header("History Scroll Root")]
    public RectTransform historyContentRect;

    [Header("Mode")]
    public bool useDynamicHeight = true;

    [Header("Fixed Height")]
    public float fixedDetailHeight = 270f;

    [Header("Dynamic Settings")]
    public float topBottomPadding = 32f;
    public float minDetailHeight = 80f;
    public float maxDetailHeight = 5000f;

    [Header("Start State")]
    public bool startOpen = false;

    private bool isOpen = false;
    private ScrollRect parentScrollRect;
    private LayoutElement rootLayoutElement;
    private RectTransform rootRect;
    private RectTransform detailPanelRect;

    private const float HeaderHeight = 60f;
    private const float ClosedExtraPadding = 4f;
    private const float OpenExtraPadding = 8f;

    private void Awake()
    {
        rootRect = GetComponent<RectTransform>();

        rootLayoutElement = GetComponent<LayoutElement>();
        if (rootLayoutElement == null)
            rootLayoutElement = gameObject.AddComponent<LayoutElement>();

        if (parentScrollRect == null)
            parentScrollRect = GetComponentInParent<ScrollRect>();

        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        if (headerButton == null)
        {
            Transform t = transform.Find("HeaderButton");
            if (t != null)
                headerButton = t.GetComponent<Button>();
        }

        if (headerText == null)
        {
            Transform t = transform.Find("HeaderButton/Text (TMP)");
            if (t != null)
                headerText = t.GetComponent<TextMeshProUGUI>();
        }

        if (detailPanel == null)
        {
            Transform t = transform.Find("DetailPanel");
            if (t != null)
                detailPanel = t.gameObject;
        }

        if (detailText == null)
        {
            Transform t = transform.Find("DetailPanel/DetailText");
            if (t != null)
                detailText = t.GetComponent<TextMeshProUGUI>();
        }

        if (detailPanel != null)
        {
            detailPanelRect = detailPanel.GetComponent<RectTransform>();

            if (detailLayoutElement == null)
                detailLayoutElement = detailPanel.GetComponent<LayoutElement>();

            if (detailLayoutElement == null)
                detailLayoutElement = detailPanel.AddComponent<LayoutElement>();
        }
    }

    public void SetHistoryContentRect(RectTransform contentRect)
    {
        historyContentRect = contentRect;
    }

    public void Setup(SessionHistoryData sessionData)
    {
        if (sessionData == null)
            return;

        AutoAssignReferences();

        if (headerButton != null)
        {
            headerButton.gameObject.SetActive(true);
            headerButton.interactable = true;
            headerButton.onClick.RemoveAllListeners();
            headerButton.onClick.AddListener(ToggleDetail);

            LayoutElement headerLayout = headerButton.GetComponent<LayoutElement>();
            if (headerLayout == null)
                headerLayout = headerButton.gameObject.AddComponent<LayoutElement>();

            headerLayout.minHeight = HeaderHeight;
            headerLayout.preferredHeight = HeaderHeight;
            headerLayout.flexibleHeight = 0f;
        }

        if (headerText != null)
        {
            headerText.gameObject.SetActive(true);
            headerText.text = sessionData.completedDateText + " • " + BuildRelativeTimeText(sessionData);
        }

        if (detailText != null)
        {
            detailText.gameObject.SetActive(true);
            detailText.text = BuildDetailText(sessionData);
        }

        isOpen = startOpen;

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshLayout();
    }

    public void ToggleDetail()
    {
        isOpen = !isOpen;

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshLayout();
    }

    private void RefreshLayout()
    {
        float detailHeight = 0f;

        if (isOpen && detailPanel != null && detailText != null && detailLayoutElement != null)
        {
            if (useDynamicHeight)
                detailHeight = CalculateRealDetailHeight();
            else
                detailHeight = fixedDetailHeight;

            // Layout Element'i runtime'da zorla
            detailLayoutElement.minHeight = detailHeight;
            detailLayoutElement.preferredHeight = detailHeight;
            detailLayoutElement.flexibleHeight = 0f;

            // RectTransform'u da runtime'da zorla
            if (detailPanelRect != null)
                detailPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, detailHeight);
        }
        else if (detailLayoutElement != null)
        {
            detailLayoutElement.minHeight = 0f;
            detailLayoutElement.preferredHeight = 0f;
            detailLayoutElement.flexibleHeight = 0f;

            if (detailPanelRect != null)
                detailPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
        }

        if (rootLayoutElement != null)
        {
            if (isOpen)
            {
                float totalHeight = HeaderHeight + detailHeight + OpenExtraPadding;
                rootLayoutElement.minHeight = totalHeight;
                rootLayoutElement.preferredHeight = totalHeight;
            }
            else
            {
                float closedHeight = HeaderHeight + ClosedExtraPadding;
                rootLayoutElement.minHeight = closedHeight;
                rootLayoutElement.preferredHeight = closedHeight;
            }

            rootLayoutElement.flexibleHeight = 0f;
        }

        Canvas.ForceUpdateCanvases();

        if (detailPanelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanelRect);

        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);

        if (historyContentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyContentRect);

        Canvas.ForceUpdateCanvases();
    }

    private float CalculateRealDetailHeight()
    {
        if (detailPanel == null || detailText == null)
            return fixedDetailHeight;

        RectTransform detailTextRect = detailText.GetComponent<RectTransform>();

        bool oldActive = detailPanel.activeSelf;
        detailPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();

        if (detailPanelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanelRect);

        if (detailTextRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(detailTextRect);

        Canvas.ForceUpdateCanvases();
        detailText.ForceMeshUpdate();

        float availableWidth = 600f;

        if (detailTextRect != null && detailTextRect.rect.width > 0f)
            availableWidth = detailTextRect.rect.width;
        else if (detailPanelRect != null && detailPanelRect.rect.width > 0f)
            availableWidth = detailPanelRect.rect.width - 30f;

        Vector2 preferred = detailText.GetPreferredValues(detailText.text, availableWidth, 0f);
        float textHeight = Mathf.Max(detailText.preferredHeight, preferred.y);

        float finalHeight = textHeight + topBottomPadding + 12f;

        if (!oldActive && !isOpen)
            detailPanel.SetActive(false);

        return Mathf.Clamp(finalHeight, minDetailHeight, maxDetailHeight);
    }

    private string BuildRelativeTimeText(SessionHistoryData sessionData)
    {
        if (sessionData.completedAtTicks <= 0)
        {
            if (!string.IsNullOrEmpty(sessionData.relativeTimeText))
                return sessionData.relativeTimeText;

            return "az önce tamamlandı";
        }

        System.DateTime completedAt = new System.DateTime(sessionData.completedAtTicks);
        System.TimeSpan diff = System.DateTime.Now - completedAt;

        if (diff.TotalSeconds < 60)
            return "az önce tamamlandı";

        if (diff.TotalMinutes < 60)
            return Mathf.FloorToInt((float)diff.TotalMinutes) + " dakika önce tamamlandı";

        if (diff.TotalHours < 24)
            return Mathf.FloorToInt((float)diff.TotalHours) + " saat önce tamamlandı";

        return Mathf.FloorToInt((float)diff.TotalDays) + " gün önce tamamlandı";
    }

    private string BuildDetailText(SessionHistoryData sessionData)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var day in sessionData.days)
        {
            sb.AppendLine("Gün " + day.dayNumber);

            for (int i = 0; i < day.patients.Count; i++)
            {
                var patient = day.patients[i];

                sb.Append("- ");
                sb.Append(patient.patientLabel);
                sb.Append(": Verilen cevap: ");
                sb.Append(patient.playerAnswer);

                if (patient.isCorrect)
                {
                    sb.Append(" | Doğru");
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(" | Yanlış");
                    sb.AppendLine();
                    sb.Append("Doğru cevap: ");
                    sb.Append(patient.correctAnswer);
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
        }

        sb.AppendLine("Toplam Doğru: " + sessionData.TotalCorrect());
        sb.AppendLine("Toplam Yanlış: " + sessionData.TotalWrong());

        return sb.ToString();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnScroll(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnEndDrag(eventData);
    }
}