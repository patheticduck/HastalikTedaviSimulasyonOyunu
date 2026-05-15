using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class AIComputerInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public GameFlowManager gameFlowManager;
    public AIComputerCameraController aiComputerController;

    [Header("Interaction")]
    public GameObject highlightObject;
    public GameObject interactText;
    public float interactDistance = 3f;

    [Header("Interaction Text")]
    public string lockedText = "AI Asistan Kilitli";
    public string unlockedText = "[E] Yapay Zeka Asistanını Aç";

    private bool isLookingAt = false;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionManager == null)
            interactionManager = FindFirstObjectByType<PlayerInteractionModeManager>();

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (aiComputerController == null)
            aiComputerController = FindFirstObjectByType<AIComputerCameraController>();

        HideVisuals();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
            return;

        // Kamera hareket ederken hiçbir interaction çalışmasın
        if (aiComputerController != null &&
            aiComputerController.IsMoving())
            return;

        // Bilgisayar açıksa E ile kapat
        if (aiComputerController != null &&
            aiComputerController.IsComputerModeActive() &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            aiComputerController.CloseComputerMode();
            return;
        }

        CheckLookAt();

        if (isLookingAt)
            RefreshInteractionVisuals();

        if (isLookingAt &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryOpenComputer();
        }
    }

    private void CheckLookAt()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool hitComputer = false;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            AIComputerInteractable computer =
                hit.collider.GetComponentInParent<AIComputerInteractable>();

            if (computer == this)
                hitComputer = true;
        }

        if (hitComputer != isLookingAt)
        {
            isLookingAt = hitComputer;
            RefreshInteractionVisuals();
        }
    }

    private void RefreshInteractionVisuals()
    {
        bool canShow = isLookingAt && CanShowInteractionPrompt();

        if (highlightObject != null)
            highlightObject.SetActive(canShow);

        if (interactText != null)
        {
            interactText.SetActive(canShow);

            if (canShow)
            {
                TextMeshProUGUI tmp =
                    interactText.GetComponent<TextMeshProUGUI>();

                if (tmp != null)
                {
                    tmp.text = IsAIUnlocked()
                        ? unlockedText
                        : lockedText;
                }
            }
        }
    }

    private bool CanShowInteractionPrompt()
    {
        if (interactionManager == null)
            return false;

        if (interactionManager.IsDialogueModeActive())
            return false;

        if (interactionManager.IsInForcedUIMode())
            return false;

        // Hasta masaya gelmeden görünmesin
        if (gameFlowManager != null &&
            !gameFlowManager.CanOpenNotebook())
            return false;

        return true;
    }

    private void TryOpenComputer()
    {
        if (!CanShowInteractionPrompt())
            return;

        // Kilitliyse açma
        if (!IsAIUnlocked())
            return;

        HideVisuals();
        isLookingAt = false;

        if (aiComputerController != null)
            aiComputerController.OpenComputerMode();
    }

    private bool IsAIUnlocked()
    {
        if (gameFlowManager == null)
            return false;

        return gameFlowManager.IsAIAnalysisUnlockedForCurrentPatient();
    }

    private void HideVisuals()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (interactText != null)
            interactText.SetActive(false);
    }
}