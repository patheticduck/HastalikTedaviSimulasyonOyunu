using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyUIManager : MonoBehaviour
{
    [Header("References")]
    public GameFlowManager gameFlowManager;
    public GameObject confirmPanel;

    [Header("Buttons")]
    public Button useEnergyButton;
    public Button yesButton;
    public Button noButton;

    [Header("Energy Icons")]
    public Image[] energyIcons;

    [Header("Optional Text")]
    public TextMeshProUGUI energyInfoText;

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (useEnergyButton != null)
        {
            useEnergyButton.onClick.RemoveListener(OnClickUseEnergy);
            useEnergyButton.onClick.AddListener(OnClickUseEnergy);
        }

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(OnConfirmYes);
            yesButton.onClick.AddListener(OnConfirmYes);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(OnConfirmNo);
            noButton.onClick.AddListener(OnConfirmNo);
        }
    }

    private void Start()
    {
        UpdateEnergyUI();
    }

    private void OnEnable()
    {
        UpdateEnergyUI();
    }

    public void OnClickUseEnergy()
    {
        if (gameFlowManager == null)
            return;

        UpdateEnergyUI();

        if (!gameFlowManager.CanUseEnergyForDoctorBook())
            return;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
            confirmPanel.transform.SetAsLastSibling();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnConfirmYes()
    {
        if (gameFlowManager != null)
            gameFlowManager.TryUseEnergyForDoctorBook();

        CloseConfirmPanel();
        UpdateEnergyUI();
    }

    public void OnConfirmNo()
    {
        CloseConfirmPanel();
        UpdateEnergyUI();
    }

    private void CloseConfirmPanel()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public bool IsConfirmPanelOpen()
    {
        return confirmPanel != null && confirmPanel.activeInHierarchy;
    }

    public void UpdateEnergyUI()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        int currentEnergy = gameFlowManager != null ? gameFlowManager.currentEnergy : 0;

        if (energyIcons != null)
        {
            for (int i = 0; i < energyIcons.Length; i++)
            {
                if (energyIcons[i] != null)
                    energyIcons[i].enabled = i < currentEnergy;
            }
        }

        if (useEnergyButton != null && gameFlowManager != null)
            useEnergyButton.interactable = gameFlowManager.CanUseEnergyForDoctorBook();

        if (energyInfoText != null && gameFlowManager != null)
            energyInfoText.text = currentEnergy + " / " + gameFlowManager.maxEnergy;
    }
}