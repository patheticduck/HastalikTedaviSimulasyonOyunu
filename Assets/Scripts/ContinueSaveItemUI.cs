using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinueSaveItemUI : MonoBehaviour
{
    [Header("UI")]
    public Button headerButton;
    public TextMeshProUGUI headerText;

    private int saveIndex = -1;
    private RuntimeSaveData saveData;
    private Action<int, RuntimeSaveData> onClicked;
    private Action<int> onDeleteClicked;

    private GameObject runtimeDeleteButtonObject;

    private void Awake()
    {
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
    }

    public void Setup(
        RuntimeSaveData data,
        int index,
        Action<int, RuntimeSaveData> clickCallback,
        Action<int> deleteCallback)
    {
        AutoAssignReferences();

        saveData = data;
        saveIndex = index;
        onClicked = clickCallback;
        onDeleteClicked = deleteCallback;

        if (headerText != null)
            headerText.text = BuildHeaderText(saveData);

        if (headerButton != null)
        {
            headerButton.onClick.RemoveAllListeners();
            headerButton.onClick.AddListener(OnHeaderClicked);
        }

        CreateDeleteButtonIfNeeded();
        SetDeleteMode(false);
    }

    private void OnHeaderClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        onClicked?.Invoke(saveIndex, saveData);
    }

    private void CreateDeleteButtonIfNeeded()
    {
        if (headerButton == null)
            return;

        if (runtimeDeleteButtonObject != null)
            return;

        GameObject deleteObj = new GameObject("DeleteButton");
        runtimeDeleteButtonObject = deleteObj;

        deleteObj.transform.SetParent(headerButton.transform, false);

        RectTransform rect = deleteObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-28f, 0f);
        rect.sizeDelta = new Vector2(32f, 32f);

        Image image = deleteObj.AddComponent<Image>();
        image.color = new Color32(180, 45, 45, 255);

        Button deleteButton = deleteObj.AddComponent<Button>();
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayClick();

            onDeleteClicked?.Invoke(saveIndex);
        });

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(deleteObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI deleteButtonText = textObj.AddComponent<TextMeshProUGUI>();
        deleteButtonText.text = "-";
        deleteButtonText.fontSize = 26;
        deleteButtonText.fontStyle = FontStyles.Bold;
        deleteButtonText.color = Color.white;
        deleteButtonText.alignment = TextAlignmentOptions.Center;
        deleteButtonText.raycastTarget = false;

        deleteObj.SetActive(false);
    }

    public void SetDeleteMode(bool active)
    {
        if (runtimeDeleteButtonObject != null)
            runtimeDeleteButtonObject.SetActive(active);
    }

    private string BuildHeaderText(RuntimeSaveData data)
    {
        if (data == null)
            return "Boş Kayıt";

        string dateText = data.savedDateText;

        if (string.IsNullOrEmpty(dateText))
            dateText = "Tarihsiz kayıt";

        return dateText +
               " • Gün " + data.currentDay +
               " • Hasta " + data.currentPatientNumber +
               " • Doğru " + data.totalCorrect +
               " • Yanlış " + data.totalWrong;
    }
}