using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueNotificationUI : MonoBehaviour
{
    [Header("UI")]
    public Button notificationButton;
    public GameObject notificationInfoPanel;
    public Image notificationButtonImage;
    public TextMeshProUGUI notificationText;

    [Header("Linked Panels")]
    public GameObject dialoguePanel;

    [Header("Text")]
    [TextArea]
    public string message =
        "Yeni sorular ve testler kullanılabilir!\nTanı koymak istiyorsanız diyalog panelini kaydırınız.";

    [Header("Blink")]
    public float blinkSpeed = 0.45f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 1f;

    [Header("Pulse")]
    public float minScale = 1f;
    public float maxScale = 1.18f;

    private Coroutine blinkRoutine;
    private bool isBlinking = false;

    private void Awake()
    {
        if (notificationInfoPanel != null)
            notificationInfoPanel.SetActive(false);

        if (notificationText != null)
            notificationText.text = message;

        if (notificationButton != null)
        {
            notificationButton.onClick.RemoveAllListeners();
            notificationButton.onClick.AddListener(ToggleNotificationPanel);
        }

        SetButtonAlpha(maxAlpha);

        if (notificationButton != null)
            notificationButton.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (!IsDialogueContextActive())
        {
            if (notificationInfoPanel != null &&
                notificationInfoPanel.activeSelf)
            {
                notificationInfoPanel.SetActive(false);
            }

            if (isBlinking)
                StopNotification();
        }
    }

    public void NotifyNewOptions()
    {
        Debug.Log("DialogueNotificationUI -> NotifyNewOptions çağrıldı.");

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("DialogueNotificationUI -> Obje aktif değil, bildirim başlatılmadı.");
            return;
        }

        if (!IsDialogueContextActive())
            return;

        if (isBlinking)
        {
            Debug.Log("DialogueNotificationUI -> Zaten blink çalışıyor.");
            return;
        }

        isBlinking = true;

        Debug.Log("DialogueNotificationUI -> Blink başladı, bildirim sesi çalacak.");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayNotification();
        else
            Debug.LogWarning("DialogueNotificationUI -> AudioManager NULL!");

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    public void StopNotification()
    {
        isBlinking = false;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        SetButtonAlpha(maxAlpha);

        if (notificationButton != null)
            notificationButton.transform.localScale = Vector3.one;
    }

    public void ResetNotificationState()
    {
        StopNotification();

        if (notificationInfoPanel != null)
            notificationInfoPanel.SetActive(false);
    }

    private void ToggleNotificationPanel()
    {
        if (notificationInfoPanel == null)
            return;

        if (!IsDialogueContextActive())
        {
            notificationInfoPanel.SetActive(false);
            return;
        }

        bool nextState = !notificationInfoPanel.activeSelf;
        notificationInfoPanel.SetActive(nextState);

        StopNotification();
    }

    private bool IsDialogueContextActive()
    {
        return dialoguePanel != null &&
               dialoguePanel.activeInHierarchy;
    }

    private IEnumerator BlinkRoutine()
    {
        while (isBlinking)
        {
            float t = Mathf.PingPong(Time.unscaledTime / blinkSpeed, 1f);

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            float scale = Mathf.Lerp(minScale, maxScale, t);

            SetButtonAlpha(alpha);

            if (notificationButton != null)
                notificationButton.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        SetButtonAlpha(maxAlpha);

        if (notificationButton != null)
            notificationButton.transform.localScale = Vector3.one;
    }

    private void SetButtonAlpha(float alpha)
    {
        if (notificationButtonImage == null)
            return;

        Color c = notificationButtonImage.color;
        c.a = alpha;
        notificationButtonImage.color = c;
    }
}