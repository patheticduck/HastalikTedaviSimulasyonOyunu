using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionModeManager : MonoBehaviour
{
    [Header("References")]
    public MouseLookLimited mouseLook;
    public DialogueManager dialogueManager;
    public GameObject dialoguePanel;
    public GameFlowManager gameFlowManager;
    public EnergyUIManager energyUIManager;

    [Header("Optional Focus")]
    public Transform doctorLookRoot;
    public Transform dialogueFocusPoint;
    public bool snapToDialogueFocusOnOpen = false;

    [Header("Input")]
    public Key toggleDialogueKey = Key.Tab;

    private bool dialogueModeActive = false;
    private bool dialogueAvailable = false;
    private bool forceUIMode = false;

    private void Start()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (energyUIManager == null)
            energyUIManager = FindFirstObjectByType<EnergyUIManager>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        PauseMenuController pause = FindFirstObjectByType<PauseMenuController>();
        if (pause != null && pause.IsPaused())
            return;

        // Enerji onay paneli açıksa Tab ile dialogue aç/kapat yapılmasın.
        // Oyuncu önce Evet/Hayır seçsin.
        if (energyUIManager != null && energyUIManager.IsConfirmPanelOpen())
            return;

        if (forceUIMode)
            return;

        if (Keyboard.current[toggleDialogueKey].wasPressedThisFrame)
        {
            if (!dialogueAvailable)
                return;

            // Tanı seçildikten sonra son konuşma akarken
            // oyuncu dialog panelini manuel kapatamasın.
            if (gameFlowManager != null && gameFlowManager.IsDiagnosisFlowInProgress())
                return;

            ToggleDialogueMode();
        }
    }

    public void EnterUIMode()
    {
        forceUIMode = true;

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);
    }

    public void ExitUIMode()
    {
        forceUIMode = false;

        if (!dialogueModeActive && mouseLook != null)
            mouseLook.SetLookEnabled(true);

        if (!dialogueModeActive && gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();
    }

    public void SetDialogueAvailable(bool available)
    {
        dialogueAvailable = available;

        if (!available)
        {
            dialogueModeActive = false;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (!forceUIMode && mouseLook != null)
                mouseLook.SetLookEnabled(true);

            // Dialogue tamamen kapandıysa, güvenli free-look durumunu yeniden değerlendir
            if (!forceUIMode && gameFlowManager != null)
                gameFlowManager.NotifyFreeLookStateEntered();
        }
    }

    public void OpenDialogueMode()
    {
        if (!dialogueAvailable)
            dialogueAvailable = true;

        dialogueModeActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        if (snapToDialogueFocusOnOpen && doctorLookRoot != null && dialogueFocusPoint != null)
        {
            doctorLookRoot.position = dialogueFocusPoint.position;
            doctorLookRoot.rotation = dialogueFocusPoint.rotation;
        }

        // Dialogue açıkken notebook hiçbir şekilde açılmamalı
        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);
    }

    public void CloseDialogueMode()
    {
        dialogueModeActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (!forceUIMode && mouseLook != null)
            mouseLook.SetLookEnabled(true);

        // Dialogue kapanınca direkt aç demiyoruz,
        // GameFlowManager güvenli mi diye kendi karar versin
        if (!forceUIMode && gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();
    }

    public void ToggleDialogueMode()
    {
        if (dialogueModeActive)
            CloseDialogueMode();
        else
            OpenDialogueMode();
    }

    public bool IsDialogueModeActive()
    {
        return dialogueModeActive;
    }

    public bool IsDialogueAvailable()
    {
        return dialogueAvailable;
    }

    public bool IsInForcedUIMode()
    {
        return forceUIMode;
    }
}