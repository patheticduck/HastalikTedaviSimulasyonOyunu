using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class HistoryItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Button headerButton;
    public TextMeshProUGUI headerText;
    public GameObject detailPanel;
    public TextMeshProUGUI detailText;
    public LayoutElement detailLayoutElement;

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

    private void Awake()
    {
        rootRect = GetComponent<RectTransform>();
        AutoAssignReferences();
    }

    private void OnEnable()
    {
        AutoAssignReferences();

        if (headerButton != null)
        {
            headerButton.onClick.RemoveAllListeners();
            headerButton.onClick.AddListener(OnHeaderButtonClicked);
            Debug.Log("[HistoryItemUI] Listener bağlandı: " + gameObject.name);
        }
        else
        {
            Debug.LogError("[HistoryItemUI] HeaderButton NULL: " + gameObject.name);
        }

        isOpen = startOpen;

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshAllLayouts();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[HistoryItemUI] Root click aldı: " + gameObject.name);
    }

    private void OnHeaderButtonClicked()
    {
        Debug.Log("[HistoryItemUI] Header button click aldı: " + gameObject.name);
        ToggleDetail();
    }

    private void AutoAssignReferences()
    {
        if (headerButton == null)
        {
            Transform buttonTransform = transform.Find("HeaderButton");
            if (buttonTransform != null)
                headerButton = buttonTransform.GetComponent<Button>();
        }

        if (headerText == null && headerButton != null)
            headerText = headerButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (detailPanel == null)
        {
            Transform t = transform.Find("DetailPanel");
            if (t != null)
                detailPanel = t.gameObject;
        }

        if (detailText == null && detailPanel != null)
            detailText = detailPanel.GetComponentInChildren<TextMeshProUGUI>(true);

        if (detailLayoutElement == null && detailPanel != null)
            detailLayoutElement = detailPanel.GetComponent<LayoutElement>();
    }

    public void ToggleDetail()
    {
        isOpen = !isOpen;

        Debug.Log("[HistoryItemUI] ToggleDetail çalıştı. isOpen = " + isOpen);

        if (detailPanel != null)
            detailPanel.SetActive(isOpen);

        RefreshDetailHeight();
        RefreshAllLayouts();
    }

    public void Setup(SessionHistoryData sessionData)
    {
        if (sessionData == null)
            return;

        AutoAssignReferences();

        if (headerText != null)
        {
            string relative = BuildRelativeTimeText(sessionData);
            headerText.text = sessionData.completedDateText + " • " + relative;
        }

        if (detailText != null)
        {
            detailText.text = BuildDetailText(sessionData);
        }

        RefreshDetailHeight();
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
            return;
        }

        if (!isOpen)
        {
            detailLayoutElement.preferredHeight = 0f;
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
    }

    private void RefreshAllLayouts()
    {
        Canvas.ForceUpdateCanvases();

        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);

        if (parentRect != null && parentRect.parent is RectTransform grandParentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(grandParentRect);
    }
}