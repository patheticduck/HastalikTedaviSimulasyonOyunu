using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotebookInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public NotebookUIManager notebookUIManager;
    public GameFlowManager gameFlowManager;

    [Header("Interaction")]
    public GameObject highlightObject;
    public GameObject interactText;
    public float interactDistance = 3f;

    [Header("Animation")]
    public float moveSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, -0.05f, 0.6f);

    [Header("Open Rotation Offset")]
    public Vector3 openRotationOffset = new Vector3(75f, 0f, 0f);

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

        if (notebookUIManager == null)
            notebookUIManager = FindFirstObjectByType<NotebookUIManager>();

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (notebookUIManager != null && notebookUIManager.notebookPanel != null)
            notebookUIManager.notebookPanel.SetActive(false);

        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (interactText != null)
            interactText.SetActive(false);
    }

    private void Update()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
            return;

        if (Keyboard.current == null)
            return;

        CheckLookAt();

        if (isLookingAt && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryToggleNotebook();
        }

        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StartCoroutine(CloseNotebook());
        }
    }

    private void CheckLookAt()
    {
        if (playerCamera == null || isAnimating)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        bool hitNotebook = false;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider != null && hit.collider.GetComponentInParent<NotebookInteractable>() == this)
            {
                hitNotebook = true;
            }
        }

        if (hitNotebook != isLookingAt)
        {
            isLookingAt = hitNotebook;
            RefreshInteractionVisuals();
        }
    }

    private void RefreshInteractionVisuals()
    {
        bool canShowPrompt = isLookingAt && !isOpen && !isAnimating && CanShowInteractionPrompt();

        if (highlightObject != null)
            highlightObject.SetActive(canShowPrompt);

        if (interactText != null)
            interactText.SetActive(canShowPrompt);
    }

    private bool CanShowInteractionPrompt()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
            return false;

        if (interactionManager == null)
            return false;

        if (interactionManager.IsDialogueModeActive())
            return false;

        if (gameFlowManager != null && !gameFlowManager.CanOpenNotebook())
            return false;

        return true;
    }

    private void TryToggleNotebook()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
            return;

        if (isAnimating)
            return;

        if (playerCamera == null)
        {
            Debug.LogError($"NotebookInteractable -> playerCamera atanmadı: {gameObject.name}", this);
            return;
        }

        if (interactionManager == null)
        {
            Debug.LogError($"NotebookInteractable -> interactionManager atanmadı: {gameObject.name}", this);
            return;
        }

        if (isOpen)
        {
            StartCoroutine(CloseNotebook());
            return;
        }

        if (!CanOpenNotebookNow())
            return;

        StartCoroutine(OpenNotebook());
    }

    private bool CanOpenNotebookNow()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
            return false;

        if (interactionManager == null)
            return false;

        if (interactionManager.IsDialogueModeActive())
            return false;

        if (gameFlowManager != null && !gameFlowManager.CanOpenNotebook())
            return false;

        return true;
    }

    private IEnumerator OpenNotebook()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
            yield break;

        if (!CanOpenNotebookNow())
            yield break;

        isAnimating = true;
        isOpen = true;

        RefreshInteractionVisuals();

        interactionManager.EnterUIMode();

        Vector3 targetPos =
            playerCamera.transform.position +
            playerCamera.transform.forward * cameraOffset.z +
            playerCamera.transform.up * cameraOffset.y +
            playerCamera.transform.right * cameraOffset.x;

        Quaternion baseLookRot = Quaternion.LookRotation(playerCamera.transform.forward, playerCamera.transform.up);
        Quaternion targetRot = baseLookRot * Quaternion.Euler(openRotationOffset);

        float t = 0f;

        while (t < 1f)
        {
            if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
                yield break;

            if (playerCamera == null)
                break;

            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(originalPosition, targetPos, t);
            transform.rotation = Quaternion.Slerp(originalRotation, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (notebookUIManager != null)
            notebookUIManager.OpenNotebookUI();

        isAnimating = false;
        RefreshInteractionVisuals();
    }

    private IEnumerator CloseNotebook()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
            yield break;

        isAnimating = true;
        isOpen = false;

        if (notebookUIManager != null)
            notebookUIManager.CloseNotebookUI();

        float t = 0f;
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        while (t < 1f)
        {
            if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
                yield break;

            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(currentPos, originalPosition, t);
            transform.rotation = Quaternion.Slerp(currentRot, originalRotation, t);
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        interactionManager.ExitUIMode();

        if (gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();

        isAnimating = false;
        RefreshInteractionVisuals();
    }

    public bool IsNotebookOpen()
    {
        return isOpen;
    }

    public bool IsNotebookAnimating()
    {
        return isAnimating;
    }

    public void ForceCloseNotebookImmediate()
    {
        StopAllCoroutines();

        isOpen = false;
        isAnimating = false;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (notebookUIManager != null)
            notebookUIManager.CloseNotebookUI();

        RefreshInteractionVisuals();
    }
}