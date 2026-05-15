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

    public Button doctorBookButton;
    public Button aiAnalysisButton;
    public Button noButton;

    [Header("Texts")]
    public TextMeshProUGUI confirmText;

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

        if (doctorBookButton != null)
        {
            doctorBookButton.onClick.RemoveListener(OnDoctorBookButton);
            doctorBookButton.onClick.AddListener(OnDoctorBookButton);
        }

        if (aiAnalysisButton != null)
        {
            aiAnalysisButton.onClick.RemoveListener(OnAIAnalysisButton);
            aiAnalysisButton.onClick.AddListener(OnAIAnalysisButton);
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

        if (!gameFlowManager.CanOpenHintPanel())
            return;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
            confirmPanel.transform.SetAsLastSibling();
        }

        UpdateHintButtons();

        if (confirmText != null)
        {
            confirmText.text =
                "1 enerji harcayarak ipucu kullanmak istediğinize emin misiniz?";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnDoctorBookButton()
    {
        if (gameFlowManager != null)
            gameFlowManager.TryUseEnergyForDoctorBook();

        CloseConfirmPanel();
        UpdateEnergyUI();
    }

    public void OnAIAnalysisButton()
    {
        if (gameFlowManager != null)
            gameFlowManager.TryUseEnergyForAIAnalysis();

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

        int currentEnergy = gameFlowManager != null
            ? gameFlowManager.currentEnergy
            : 0;

        if (energyIcons != null)
        {
            for (int i = 0; i < energyIcons.Length; i++)
            {
                if (energyIcons[i] != null)
                    energyIcons[i].enabled = i < currentEnergy;
            }
        }

        if (useEnergyButton != null && gameFlowManager != null)
            useEnergyButton.interactable =
                gameFlowManager.CanOpenHintPanel();

        if (energyInfoText != null && gameFlowManager != null)
            energyInfoText.text =
                currentEnergy + " / " + gameFlowManager.maxEnergy;

        UpdateHintButtons();
    }

    private void UpdateHintButtons()
    {
        if (gameFlowManager == null)
            return;

        bool hasEnergy = gameFlowManager.currentEnergy > 0;

        if (doctorBookButton != null)
        {
            doctorBookButton.interactable =
                hasEnergy &&
                !gameFlowManager.IsDoctorBookUnlockedForCurrentPatient();
        }

        if (aiAnalysisButton != null)
        {
            aiAnalysisButton.interactable =
                hasEnergy &&
                !gameFlowManager.IsAIAnalysisUnlockedForCurrentPatient();
        }
    }
}