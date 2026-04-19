using UnityEngine;

public class RuntimeSaveManager : MonoBehaviour
{
    public static RuntimeSaveManager Instance;

    public RuntimeSaveData currentSave;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentSave = new RuntimeSaveData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(GameFlowManager gameFlow)
    {
        if (gameFlow == null)
        {
            Debug.LogWarning("RuntimeSaveManager -> GameFlowManager null, kayıt alınamadı.");
            return;
        }

        currentSave = gameFlow.BuildRuntimeSaveData();
        currentSave.hasSave = true;

        Debug.Log("Oyun kaydedildi.");
    }

    public void ClearSave()
    {
        currentSave = new RuntimeSaveData();
    }
}