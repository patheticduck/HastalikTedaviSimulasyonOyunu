using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionModeManager : MonoBehaviour
{
    [Header("References")]
    public MouseLookLimited mouseLook;
    public DialogueManager dialogueManager;
    public GameObject dialoguePanel;

    [Header("Optional Focus")]
    public Transform doctorLookRoot;
    public Transform dialogueFocusPoint;
    public bool snapToDialogueFocusOnOpen = false;

    [Header("Input")]
    public Key toggleDialogueKey = Key.Tab;

    private bool dialogueModeActive = false;
    private bool dialogueAvailable = false;
    private bool forceUIMode = false;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (forceUIMode)
            return;

        if (Keyboard.current[toggleDialogueKey].wasPressedThisFrame)
        {
            if (!dialogueAvailable)
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
    }

    public void CloseDialogueMode()
    {
        dialogueModeActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (!forceUIMode && mouseLook != null)
            mouseLook.SetLookEnabled(true);
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
}