using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIComputerCameraController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform aiComputerCameraPoint;
    public Transform gameplayCameraPoint;
    public AIAnalysisUIManager aiAnalysisUIManager;
    public PlayerInteractionModeManager interactionModeManager;
    public GameFlowManager gameFlowManager;
    public PauseMenuController pauseMenuController;

    [Header("Settings")]
    public Key toggleKey = Key.Y;
    public float moveDuration = 0.45f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isComputerModeActive = false;
    private bool isMoving = false;

    private bool movingToComputer = false;
    private bool movingToGameplay = false;

    private Coroutine moveRoutine;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (aiAnalysisUIManager == null)
            aiAnalysisUIManager = FindFirstObjectByType<AIAnalysisUIManager>();

        if (interactionModeManager == null)
            interactionModeManager = FindFirstObjectByType<PlayerInteractionModeManager>();

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (pauseMenuController == null)
            pauseMenuController = FindFirstObjectByType<PauseMenuController>();

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.SetExternalCloseAction(CloseComputerMode);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (pauseMenuController != null && pauseMenuController.IsPaused())
            return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (isMoving)
                return;

            if (isComputerModeActive)
                CloseComputerMode();
            else
                OpenComputerMode();
        }
    }

    public void OpenComputerMode()
    {
        if (mainCamera == null || aiComputerCameraPoint == null || aiAnalysisUIManager == null)
            return;

        if (pauseMenuController != null && pauseMenuController.IsPaused())
            return;

        if (interactionModeManager != null && interactionModeManager.IsDialogueModeActive())
            return;

        if (gameFlowManager != null)
        {
            if (!gameFlowManager.IsAIAnalysisUnlockedForCurrentPatient())
                return;

            if (!gameFlowManager.CanOpenAIAnalysisPanel())
                return;
        }

        startPosition = mainCamera.transform.position;
        startRotation = mainCamera.transform.rotation;

        isComputerModeActive = true;
        movingToComputer = true;
        movingToGameplay = false;

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveCameraRoutine(
            aiComputerCameraPoint.position,
            aiComputerCameraPoint.rotation,
            true
        ));
    }

    public void CloseComputerMode()
    {
        if (!isComputerModeActive && !isMoving)
            return;

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.ClosePanelOnly();

        movingToComputer = false;
        movingToGameplay = true;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveCameraRoutine(
            startPosition,
            startRotation,
            false
        ));
    }

    public void ForceCompleteComputerMoveForPause()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isMoving = false;
        movingToComputer = false;
        movingToGameplay = false;
        isComputerModeActive = true;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && aiComputerCameraPoint != null)
        {
            mainCamera.transform.position = aiComputerCameraPoint.position;
            mainCamera.transform.rotation = aiComputerCameraPoint.rotation;
        }

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.OpenPanel();
    }

    public void ForceCompleteGameplayMoveForPause()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isMoving = false;
        movingToComputer = false;
        movingToGameplay = false;
        isComputerModeActive = false;

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.ClosePanelOnly();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && gameplayCameraPoint != null)
        {
            mainCamera.transform.position = gameplayCameraPoint.position;
            mainCamera.transform.rotation = gameplayCameraPoint.rotation;
        }

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();
    }

    public void ForceResetToGameplayCamera(bool exitUIMode = true)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isMoving = false;
        movingToComputer = false;
        movingToGameplay = false;
        isComputerModeActive = false;

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.ClosePanelOnly();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && gameplayCameraPoint != null)
        {
            mainCamera.transform.position = gameplayCameraPoint.position;
            mainCamera.transform.rotation = gameplayCameraPoint.rotation;
        }

        if (exitUIMode && interactionModeManager != null)
            interactionModeManager.ExitUIMode();
    }

    private IEnumerator MoveCameraRoutine(Vector3 targetPosition, Quaternion targetRotation, bool openPanelAfterMove)
    {
        isMoving = true;

        Vector3 fromPosition = mainCamera.transform.position;
        Quaternion fromRotation = mainCamera.transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(fromPosition, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(fromRotation, targetRotation, t);

            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isMoving = false;
        movingToComputer = false;
        movingToGameplay = false;

        if (openPanelAfterMove)
        {
            aiAnalysisUIManager.OpenPanel();
        }
        else
        {
            isComputerModeActive = false;

            if (interactionModeManager != null)
                interactionModeManager.ExitUIMode();
        }
    }

    public bool IsComputerModeActive()
    {
        return isComputerModeActive;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsMovingToComputer()
    {
        return movingToComputer;
    }

    public bool IsMovingToGameplay()
    {
        return movingToGameplay;
    }
}