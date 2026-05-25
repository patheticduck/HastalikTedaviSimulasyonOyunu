using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoctorCardHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Info")]
    public GameObject hoverInfoPanel;

    [Header("Scale Target")]
    public RectTransform scaleTarget;

    [Header("Animation")]
    public float hoverScale = 1.04f;
    public float animationDuration = 0.15f;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (scaleTarget == null)
            scaleTarget = GetComponent<RectTransform>();

        originalScale = scaleTarget.localScale;
        ResetHoverState();
    }

    private void OnEnable()
    {
        ResetHoverState();
    }

    private void OnDisable()
    {
        ResetHoverState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverInfoPanel != null)
            hoverInfoPanel.SetActive(true);

        StartScaleAnimation(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHoverState();
    }

    private void ResetHoverState()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (hoverInfoPanel != null)
            hoverInfoPanel.SetActive(false);

        if (scaleTarget != null)
            scaleTarget.localScale = originalScale;
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (scaleTarget == null)
            return;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = scaleTarget.localScale;
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / animationDuration);

            scaleTarget.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        scaleTarget.localScale = targetScale;
    }
}