using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class HistoryItemUI : MonoBehaviour,
    IPointerClickHandler,
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
    public float topBottomPadding = 30f;
    public float minDetailHeight = 80f;
    public float maxDetailHeight = 1200f;

    [Header("Start State")]
    public bool startOpen = false;

    private bool isOpen = false;
    private RectTransform rootRect;
    private LayoutElement rootLayoutElement;
    private ScrollRect parentScrollRect;

    private void Awake()
    {
        rootRect = GetComponent<RectTransform>();

        rootLayoutElement = GetComponent<LayoutElement>();
        if (rootLayoutElement == null)
            rootLayoutElement = gameObject.AddComponent<LayoutElement>();

        AutoAssignReferences();

        if (parentScrollRect == null)
            parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    private void OnEnable()
    {
        AutoAssignReferences();

        if (parentScrollRect == null)
            parentScrollRect = GetComponentInParent<ScrollRect>();

        ForceHeaderVisible();
        BindButton();

        isOpen = startOpen;

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshDetailHeight();
        RefreshRootHeight();
        RefreshAllLayouts();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[HistoryItemUI] Root click aldı: " + gameObject.name);
    }

    public void SetHistoryContentRect(RectTransform contentRect)
    {
        historyContentRect = contentRect;
        RefreshAllLayouts();
    }

    private void BindButton()
    {
        if (headerButton != null)
        {
            headerButton.onClick.RemoveListener(OnHeaderButtonClicked);
            headerButton.onClick.AddListener(OnHeaderButtonClicked);
        }
        else
        {
            Debug.LogError("[HistoryItemUI] HeaderButton NULL: " + gameObject.name);
        }
    }

    private void OnHeaderButtonClicked()
    {
        ToggleDetail();
    }

    private void AutoAssignReferences()
    {
        if (headerButton == null)
            headerButton = GetComponentInChildren<Button>(true);

        if (headerText == null && headerButton != null)
            headerText = headerButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (detailPanel == null)
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (Transform t in children)
            {
                if (t.name == "DetailPanel")
                {
                    detailPanel = t.gameObject;
                    break;
                }
            }
        }

        if (detailText == null && detailPanel != null)
            detailText = detailPanel.GetComponentInChildren<TextMeshProUGUI>(true);

        if (detailLayoutElement == null && detailPanel != null)
            detailLayoutElement = detailPanel.GetComponent<LayoutElement>();
    }

    private void ForceHeaderVisible()
    {
        if (headerButton != null)
        {
            headerButton.gameObject.SetActive(true);

            RectTransform headerRect = headerButton.GetComponent<RectTransform>();
            if (headerRect != null && headerRect.sizeDelta.y < 60f)
                headerRect.sizeDelta = new Vector2(headerRect.sizeDelta.x, 60f);

            LayoutElement headerLayout = headerButton.GetComponent<LayoutElement>();
            if (headerLayout == null)
                headerLayout = headerButton.gameObject.AddComponent<LayoutElement>();

            headerLayout.minHeight = 60f;
            headerLayout.preferredHeight = 60f;
            headerLayout.flexibleHeight = 0f;

            Image img = headerButton.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                if (c.a <= 0.05f)
                    img.color = new Color(c.r, c.g, c.b, 1f);
            }
        }

        if (headerText != null)
        {
            headerText.gameObject.SetActive(true);

            Color tc = headerText.color;
            if (tc.a <= 0.05f)
                headerText.color = new Color(tc.r, tc.g, tc.b, 1f);
        }
    }

    public void ToggleDetail()
    {
        isOpen = !isOpen;

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshDetailHeight();
        RefreshRootHeight();
        RefreshAllLayouts();
    }

    public void Setup(SessionHistoryData sessionData)
    {
        if (sessionData == null)
            return;

        AutoAssignReferences();
        ForceHeaderVisible();
        BindButton();

        if (headerText != null)
        {
            string relative = BuildRelativeTimeText(sessionData);
            headerText.text = sessionData.completedDateText + " • " + relative;
        }

        if (detailText != null)
            detailText.text = BuildDetailText(sessionData);

        isOpen = startOpen;

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshDetailHeight();
        RefreshRootHeight();
        RefreshAllLayouts();
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

    public void RefreshDetailHeight()
    {
        if (detailLayoutElement == null)
            return;

        if (!useDynamicHeight)
        {
            detailLayoutElement.preferredHeight = isOpen ? fixedDetailHeight : 0f;
            detailLayoutElement.minHeight = 0f;
            detailLayoutElement.flexibleHeight = 0f;
            return;
        }

        if (!isOpen)
        {
            detailLayoutElement.preferredHeight = 0f;
            detailLayoutElement.minHeight = 0f;
            detailLayoutElement.flexibleHeight = 0f;
            return;
        }

        if (detailText == null || detailPanel == null)
            return;

        Canvas.ForceUpdateCanvases();
        detailText.ForceMeshUpdate();

        float textHeight = detailText.preferredHeight;
        float finalHeight = textHeight + topBottomPadding + 24f;
        finalHeight = Mathf.Clamp(finalHeight, minDetailHeight, maxDetailHeight);

        detailLayoutElement.preferredHeight = finalHeight;
        detailLayoutElement.minHeight = 0f;
        detailLayoutElement.flexibleHeight = 0f;
    }

    private void RefreshRootHeight()
    {
        if (rootLayoutElement == null)
            return;

        float headerHeight = 60f;

        if (headerButton != null)
        {
            RectTransform headerRect = headerButton.GetComponent<RectTransform>();
            if (headerRect != null && headerRect.rect.height > 0f)
                headerHeight = Mathf.Max(60f, headerRect.rect.height);
        }

        float detailHeight = 0f;
        if (isOpen && detailLayoutElement != null)
            detailHeight = detailLayoutElement.preferredHeight;

        float totalHeight = headerHeight + detailHeight + 16f;

        rootLayoutElement.preferredHeight = totalHeight;
        rootLayoutElement.minHeight = headerHeight + 16f;
        rootLayoutElement.flexibleHeight = 0f;
    }

    private void RefreshAllLayouts()
    {
        Canvas.ForceUpdateCanvases();

        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);

        if (historyContentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyContentRect);

        if (parentRect != null && parentRect.parent is RectTransform grandParentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(grandParentRect);

        Canvas.ForceUpdateCanvases();
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