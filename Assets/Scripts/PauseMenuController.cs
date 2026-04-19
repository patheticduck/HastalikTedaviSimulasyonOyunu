using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuPanel;
    public GameObject crosshair;
    public TextMeshProUGUI resultText;

    [Header("References")]
    public MouseLookLimited mouseLook;
    public GameFlowManager gameFlowManager;
    public PlayerInteractionModeManager interactionModeManager;

    private bool isPaused = false;
    private bool wasDialogueModeActiveBeforePause = false;

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

        if (pausePressed)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        RefreshPauseInfo();

        wasDialogueModeActiveBeforePause =
            interactionModeManager != null &&
            interactionModeManager.IsDialogueModeActive();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        if (wasDialogueModeActiveBeforePause)
        {
            if (mouseLook != null)
                mouseLook.SetLookEnabled(false);

            if (crosshair != null)
                crosshair.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (mouseLook != null)
                mouseLook.SetLookEnabled(true);

            if (crosshair != null)
                crosshair.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
}