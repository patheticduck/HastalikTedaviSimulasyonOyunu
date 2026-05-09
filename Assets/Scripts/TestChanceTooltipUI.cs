using UnityEngine;
using TMPro;

public class TestChanceTooltipUI : MonoBehaviour
{
    public static TestChanceTooltipUI Instance { get; private set; }

    [Header("UI")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private void Awake()
    {
        Instance = this;

        if (tooltipPanel != null)
        {
            CanvasGroup cg = tooltipPanel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = tooltipPanel.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        Hide();
    }

    public void Show(DialogueChoiceData choice)
    {
        if (choice == null || tooltipPanel == null || tooltipText == null)
            return;

        int finalChance = CalculateFinalChance(choice);

        tooltipText.text =
            choice.choiceText +
            "\n\nBaşarı Şansı: %" + finalChance +
            "\n\nUzmanlık Bonusu: " + GetBonusText(choice);

        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private int CalculateFinalChance(DialogueChoiceData choice)
    {
        int chance = choice.baseSuccessChance;

        DoctorProfileManager profile = DoctorProfileManager.Instance;

        if (profile != null)
        {
            DoctorSpecialization selected = profile.GetSelectedSpecialization();

            if (selected == choice.bonusSpecialization)
                chance += choice.specializationBonus;
            else if (selected == DoctorSpecialization.Generalist)
                chance += choice.generalistBonus;
        }

        return Mathf.Clamp(chance, 0, 100);
    }

    private string GetBonusText(DialogueChoiceData choice)
    {
        DoctorProfileManager profile = DoctorProfileManager.Instance;

        if (profile == null)
            return "Yok";

        DoctorSpecialization selected = profile.GetSelectedSpecialization();

        if (selected == choice.bonusSpecialization)
            return "+" + choice.specializationBonus + " (" + selected + ")";

        if (selected == DoctorSpecialization.Generalist)
            return "+" + choice.generalistBonus + " (Generalist)";

        return "Yok";
    }
}