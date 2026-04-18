using System.Collections.Generic;
using UnityEngine;

public class RunHistoryManager : MonoBehaviour
{
    public static RunHistoryManager Instance;

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