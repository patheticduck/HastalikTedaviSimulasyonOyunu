using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("Panels To Close On Main Menu")]
    public GameObject doctorBookPanel;
    public GameObject notebookPanel;

    [Header("UI")]
    public GameObject pauseMenuPanel;
    public GameObject crosshair;
    public TextMeshProUGUI resultText;

    [Header("Blocking Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("References")]
    public MouseLookLimited mouseLook;
    public GameFlowManager gameFlowManager;
    public PlayerInteractionModeManager interactionModeManager;
    public NotebookInteractable notebookInteractable;
    public DoctorBookInteractable doctorBookInteractable;

    private bool isPaused = false;
    private bool wasDialogueModeActiveBeforePause = false;
    private bool wasForcedUIModeBeforePause = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        isPaused = false;

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (interactionModeManager == null)
            interactionModeManager = FindFirstObjectByType<PlayerInteractionModeManager>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool pausePressed = false;

#if UNITY_EDITOR
        pausePressed = Keyboard.current.pKey.wasPressedThisFrame;
#else
        pausePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
#endif

        if (!pausePressed)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
            return;

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            if (CanOpenPause())
                PauseGame();
        }
    }

    private bool CanOpenPause()
    {
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            return false;

        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            return false;

        if (settingsPanel != null && settingsPanel.activeSelf)
            return false;

        if (gameFlowManager != null)
        {
            if (gameFlowManager.patientResultPanel != null && gameFlowManager.patientResultPanel.activeSelf)
                return false;

            if (gameFlowManager.endDayPanel != null && gameFlowManager.endDayPanel.activeSelf)
                return false;

            if (gameFlowManager.gameOverPanel != null && gameFlowManager.gameOverPanel.activeSelf)
                return false;

            if (gameFlowManager.dayTransitionPanel != null && gameFlowManager.dayTransitionPanel.activeSelf)
                return false;
        }

        return true;
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        RefreshPauseInfo();

        bool interruptedBookAnimation = false;

        if (notebookInteractable != null && notebookInteractable.IsNotebookAnimating())
        {
            notebookInteractable.ForceCloseNotebookImmediate();
            interruptedBookAnimation = true;
        }

        if (doctorBookInteractable != null && doctorBookInteractable.IsBookAnimating())
        {
            doctorBookInteractable.ForceCloseBookImmediate();
            interruptedBookAnimation = true;
        }

        if (interruptedBookAnimation && interactionModeManager != null)
        {
            interactionModeManager.ExitUIMode();
        }

        wasDialogueModeActiveBeforePause =
            interactionModeManager != null &&
            interactionModeManager.IsDialogueModeActive();

        wasForcedUIModeBeforePause =
            interactionModeManager != null &&
            interactionModeManager.IsInForcedUIMode();

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            pauseMenuPanel.transform.SetAsLastSibling();
        }

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        if (wasDialogueModeActiveBeforePause || wasForcedUIModeBeforePause)
        {
            if (mouseLook != null)
                mouseLook.SetLookEnabled(false);

            if (crosshair != null)
                crosshair.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (gameFlowManager != null)
                gameFlowManager.SetNotebookBlocked(true);
        }
        else
        {
            if (mouseLook != null)
                mouseLook.SetLookEnabled(true);

            if (crosshair != null)
                crosshair.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (gameFlowManager != null)
                gameFlowManager.NotifyFreeLookStateEntered();
        }
    }

    public void SaveGame()
    {
        if (gameFlowManager == null)
        {
            Debug.LogWarning("PauseMenuController -> GameFlowManager bağlı değil, kayıt alınamadı.");
            return;
        }

        if (RuntimeSaveManager.Instance == null)
        {
            Debug.LogWarning("PauseMenuController -> RuntimeSaveManager bulunamadı, kayıt alınamadı.");
            return;
        }

        RuntimeSaveManager.Instance.SaveGame(gameFlowManager);
        Debug.Log("Pause panelinden oyun kaydedildi.");
    }

    public void ReturnToMainMenu()
    {
        if (gameFlowManager != null)
            gameFlowManager.ForceStopAllGameplay();

        if (notebookInteractable != null)
            notebookInteractable.ForceCloseNotebookImmediate();

        if (doctorBookInteractable != null)
            doctorBookInteractable.ForceCloseBookImmediate();

        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        if (gameFlowManager != null && gameFlowManager.mainMenuManager != null)
        {
            gameFlowManager.mainMenuManager.ShowMainMenu();
        }
        else
        {
            Debug.LogWarning("PauseMenuController -> MainMenuManager bulunamadı, menüye dönülemedi.");
        }
    }

    private void RefreshPauseInfo()
    {
        if (resultText == null || gameFlowManager == null)
            return;

        int patientInDay = ((gameFlowManager.currentPatientNumber - 1) % gameFlowManager.patientsPerDay) + 1;

        resultText.text =
            "Gün: " + gameFlowManager.currentDay + "\n" +
            "Hasta: " + patientInDay + " (" + gameFlowManager.currentPatientNumber + "/" + gameFlowManager.totalPatientsInRun + ")\n" +
            "Doğru: " + gameFlowManager.totalCorrect + "\n" +
            "Yanlış: " + gameFlowManager.totalWrong + "\n" +
            "Kalan Hak: " + gameFlowManager.remainingHearts;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}