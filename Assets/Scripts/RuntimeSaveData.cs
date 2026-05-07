using System.Collections.Generic;

[System.Serializable]
public class RuntimeSaveData
{
    public int currentDay;
    public int currentPatientNumber;
    public int totalCorrect;
    public int totalWrong;
    public int remainingHearts;

    public int maxEnergy;
    public int currentEnergy;
    public bool doctorBookUnlockedForCurrentPatient;

    public int dayCorrect;
    public int dayWrong;

    public bool diagnosisSubmittedForCurrentPatient;
    public bool patientResultPanelPendingAfterLoad;

    public bool lastDiagnosisWasCorrect;
    public string lastDiagnosisResultText;
    public string lastCorrectDiagnosisName;

    public bool pendingLoseGameOver;

    public bool hasSave;

    // Yeni sistem:
    // Oyun başında rastgele oluşturulan 15 hastalık sırası burada saklanır.
    // Böylece Save/Load sonrası hastalar yeniden random seçilmez.
    public List<string> plannedPatientCaseIds = new List<string>();

    public List<PatientAnalysisEntry> patientAnalysisHistory = new List<PatientAnalysisEntry>();
}