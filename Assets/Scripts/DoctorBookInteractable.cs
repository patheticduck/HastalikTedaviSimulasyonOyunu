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

    [Header("Interaction Text")]
    public string unlockedInteractText = "[E] Doktor Kitabını Aç";
    public string lockedInteractText = "Doktor Kitabı kilitli - Enerji kullan";

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
        if (IsPauseActive())
            return;

        if (Keyboard.current == null)
            return;

        CheckLookAt();

        if (isLookingAt && Keyboard.current.eKey.wasPressedThisFrame)
            TryToggleBook();

        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseBook();
    }

    private bool IsPauseActive()
    {
        if (PauseMenuController.Instance == null)
            return false;

        if (PauseMenuController.Instance.IsPaused())
            return true;

        if (PauseMenuController.Instance.pauseMenuPanel != null &&
            PauseMenuController.Instance.pauseMenuPanel.activeInHierarchy)
            return true;

        return false;
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
                {
                    if (IsDoctorBookUnlocked())
                        tmp.text = unlockedInteractText;
                    else
                        tmp.text = lockedInteractText;
                }
            }
        }
    }

    private bool CanOpenBookNow()
    {
        if (IsPauseActive())
            return false;

        if (interactionManager == null)
            return false;

        if (interactionManager.IsDialogueModeActive())
            return false;

        if (gameFlowManager != null && !gameFlowManager.CanOpenNotebook())
            return false;

        return true;
    }

    private bool IsDoctorBookUnlocked()
    {
        if (gameFlowManager == null)
            return false;

        return gameFlowManager.IsDoctorBookUnlockedForCurrentPatient();
    }

    private void TryToggleBook()
    {
        if (IsPauseActive())
            return;

        if (isAnimating)
            return;

        if (isOpen)
        {
            CloseBook();
            return;
        }

        if (!CanOpenBookNow())
            return;

        if (!IsDoctorBookUnlocked())
        {
            Debug.Log("DoctorBook kilitli. Açmak için enerji kullanmalısın.");
            RefreshInteractionVisuals();
            return;
        }

        StartCoroutine(OpenBookRoutine());
    }

    private IEnumerator OpenBookRoutine()
    {
        isAnimating = true;
        isOpen = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBookOpenClose();

        RefreshInteractionVisuals();

        if (interactionManager != null)
            interactionManager.EnterUIMode();

        yield return null;
        yield return new WaitForEndOfFrame();

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
            if (IsPauseActive())
            {
                isAnimating = false;
                yield break;
            }

            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        yield return StartCoroutine(SafeOpenBookPanelRoutine());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isAnimating = false;
        RefreshInteractionVisuals();
    }

    private IEnumerator SafeOpenBookPanelRoutine()
    {
        if (doctorBookUIManager == null)
            yield break;

        doctorBookUIManager.OpenBook();

        yield return null;
        yield return new WaitForEndOfFrame();

        if (!doctorBookUIManager.IsBookPanelOpen())
            doctorBookUIManager.OpenBook();

        yield return null;

        if (!doctorBookUIManager.IsBookPanelOpen())
            doctorBookUIManager.OpenBook();
    }

    private void CloseBook()
    {
        if (IsPauseActive())
            return;

        if (!isOpen || isAnimating)
            return;

        StartCoroutine(CloseBookRoutine());
    }

    public void CloseBookFromButton()
    {
        if (IsPauseActive())
            return;

        CloseBook();
    }

    public void ForceCloseBookImmediate()
    {
        StopAllCoroutines();

        isOpen = false;
        isAnimating = false;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (doctorBookUIManager != null)
            doctorBookUIManager.CloseBook();

        RefreshInteractionVisuals();
    }

    public bool IsBookAnimating()
    {
        return isAnimating;
    }

    private IEnumerator CloseBookRoutine()
    {
        isAnimating = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBookOpenClose();

        if (doctorBookUIManager != null)
            doctorBookUIManager.CloseBook();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            if (IsPauseActive())
            {
                isAnimating = false;
                yield break;
            }

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