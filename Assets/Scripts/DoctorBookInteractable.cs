using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DoctorBookInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public DoctorBookUIManager doctorBookUIManager;
    public GameFlowManager gameFlowManager;

    [Header("Interaction")]
    public GameObject highlightObject;
    public GameObject interactText;
    public float interactDistance = 3f;

    [Header("Animation")]
    public float moveSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, -0.05f, 0.6f);

    [Header("Open Rotation Offset")]
    public Vector3 openRotationOffset = new Vector3(-75f, -180f, 180f);

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isOpen = false;
    private bool isAnimating = false;
    private bool isLookingAt = false;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionManager == null)
            interactionManager = FindFirstObjectByType<PlayerInteractionModeManager>();

        if (doctorBookUIManager == null)
            doctorBookUIManager = FindFirstObjectByType<DoctorBookUIManager>();

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (interactText != null)
            interactText.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        CheckLookAt();

        if (isLookingAt && Keyboard.current.eKey.wasPressedThisFrame)
            TryToggleBook();

        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseBook();
    }

    private void CheckLookAt()
    {
        if (playerCamera == null || isAnimating || isOpen)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool hitBook = false;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider != null &&
                hit.collider.GetComponentInParent<DoctorBookInteractable>() == this)
            {
                hitBook = true;
            }
        }

        if (hitBook != isLookingAt)
        {
            isLookingAt = hitBook;
            RefreshInteractionVisuals();
        }
    }

    private void RefreshInteractionVisuals()
    {
        bool canShow = isLookingAt && !isOpen && !isAnimating && CanOpenBookNow();

        if (highlightObject != null)
            highlightObject.SetActive(canShow);

        if (interactText != null)
        {
            interactText.SetActive(canShow);

            if (canShow)
            {
                TextMeshProUGUI tmp = interactText.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = "[E] Doktor Kitabını Aç";
            }
        }
    }

    private bool CanOpenBookNow()
    {
        if (interactionManager == null)
            return false;

        if (interactionManager.IsDialogueModeActive())
            return false;

        if (gameFlowManager != null && !gameFlowManager.CanOpenNotebook())
            return false;

        return true;
    }

    private void TryToggleBook()
    {
        if (isAnimating)
            return;

        if (isOpen)
        {
            CloseBook();
            return;
        }

        if (!CanOpenBookNow())
            return;

        StartCoroutine(OpenBookRoutine());
    }

    private IEnumerator OpenBookRoutine()
    {
        isAnimating = true;
        isOpen = true;

        RefreshInteractionVisuals();

        if (interactionManager != null)
            interactionManager.EnterUIMode();

        Vector3 targetPos =
            playerCamera.transform.position +
            playerCamera.transform.forward * cameraOffset.z +
            playerCamera.transform.up * cameraOffset.y +
            playerCamera.transform.right * cameraOffset.x;

        Quaternion baseLookRot = Quaternion.LookRotation(playerCamera.transform.forward, playerCamera.transform.up);
        Quaternion targetRot = baseLookRot * Quaternion.Euler(openRotationOffset);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (doctorBookUIManager != null)
            doctorBookUIManager.OpenBook();

        isAnimating = false;
        RefreshInteractionVisuals();
    }

    private void CloseBook()
    {
        if (!isOpen || isAnimating)
            return;

        StartCoroutine(CloseBookRoutine());
    }

    public void CloseBookFromButton()
    {
        CloseBook();
    }

    private IEnumerator CloseBookRoutine()
    {
        isAnimating = true;

        if (doctorBookUIManager != null)
            doctorBookUIManager.CloseBook();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        isOpen = false;
        isAnimating = false;

        if (interactionManager != null)
            interactionManager.ExitUIMode();

        if (gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();

        RefreshInteractionVisuals();
    }
}