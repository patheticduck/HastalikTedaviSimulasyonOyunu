using System.Collections.Generic;
using UnityEngine;

public class RunHistoryManager : MonoBehaviour
{
    public static RunHistoryManager Instance;

    [Header("History Limit")]
    [SerializeField] private int maxHistoryCount = 5;

    private List<SessionHistoryData> sessionHistoryList = new List<SessionHistoryData>();

    public IReadOnlyList<SessionHistoryData> SessionHistoryList => sessionHistoryList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddSession(SessionHistoryData session)
    {
        if (session == null)
            return;

        sessionHistoryList.Insert(0, session);

        while (sessionHistoryList.Count > maxHistoryCount)
        {
            sessionHistoryList.RemoveAt(sessionHistoryList.Count - 1);
        }
    }

    public void RemoveSessionAt(int index)
    {
        if (index < 0 || index >= sessionHistoryList.Count)
            return;

        sessionHistoryList.RemoveAt(index);
    }

    public bool HasHistory()
    {
        return sessionHistoryList.Count > 0;
    }

    public void ClearHistory()
    {
        sessionHistoryList.Clear();
    }
}