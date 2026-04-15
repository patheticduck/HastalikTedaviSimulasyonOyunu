using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotebookInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public NotebookUIManager notebookUIManager;

    [Header("Animation")]
    public float moveSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, -0.05f, 0.6f);

    [Header("Open Rotation Offset")]
    public Vector3 openRotationOffset = new Vector3(75f, 0f, 0f);

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isOpen = false;
    private bool isAnimating = false;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionManager == null)
            interactionManager = FindFirstObjectByType<PlayerInteractionModeManager>();

        if (notebookUIManager == null)
            notebookUIManager = FindFirstObjectByType<NotebookUIManager>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (notebookUIManager != null && notebookUIManager.notebookPanel != null)
            notebookUIManager.notebookPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryToggleNotebook();
        }

        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StartCoroutine(CloseNotebook());
        }
    }

    private void TryToggleNotebook()
    {
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

        if (interactionManager.IsDialogueModeActive())
            return;

        if (isOpen)
            StartCoroutine(CloseNotebook());
        else
            StartCoroutine(OpenNotebook());
    }

    private IEnumerator OpenNotebook()
    {
        isAnimating = true;
        isOpen = true;

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
    }

    private IEnumerator CloseNotebook()
    {
        isAnimating = true;
        isOpen = false;

        if (notebookUIManager != null)
            notebookUIManager.CloseNotebookUI();

        float t = 0f;
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(currentPos, originalPosition, t);
            transform.rotation = Quaternion.Slerp(currentRot, originalRotation, t);
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        interactionManager.ExitUIMode();

        isAnimating = false;
    }
}