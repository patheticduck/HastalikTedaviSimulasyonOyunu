using System.Collections.Generic;

[System.Serializable]
public class RuntimeSaveData
{
    public int currentDay;
    public int currentPatientNumber;
    public int totalCorrect;
    public int totalWrong;
    public int remainingHearts;

    public int dayCorrect;
    public int dayWrong;

    public bool diagnosisSubmittedForCurrentPatient;
    public bool patientResultPanelPendingAfterLoad;

    public bool lastDiagnosisWasCorrect;
    public string lastDiagnosisResultText;
    public string lastCorrectDiagnosisName;

    public bool pendingLoseGameOver;

    public bool hasSave;

    public List<PatientAnalysisEntry> patientAnalysisHistory = new List<PatientAnalysisEntry>();
}