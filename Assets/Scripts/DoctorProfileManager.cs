using UnityEngine;

public class DoctorProfileManager : MonoBehaviour
{
    public static DoctorProfileManager Instance { get; private set; }

    [Header("Selected Doctor Profile")]
    public DoctorSpecialization selectedSpecialization = DoctorSpecialization.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectSpecialization(DoctorSpecialization specialization)
    {
        selectedSpecialization = specialization;

        Debug.Log("Seçilen doktor uzmanlığı: " + selectedSpecialization);
    }

    public DoctorSpecialization GetSelectedSpecialization()
    {
        return selectedSpecialization;
    }

    public bool HasSpecialization(DoctorSpecialization specialization)
    {
        return selectedSpecialization == specialization;
    }
}