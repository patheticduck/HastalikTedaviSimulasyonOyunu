using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RunHistoryManager : MonoBehaviour
{
    public static RunHistoryManager Instance;

    [Header("History Limit")]
    [SerializeField] private int maxHistoryCount = 5;

    [Header("Persistent Save")]
    [SerializeField] private bool usePersistentSave = true;

    private readonly List<byte[]> sessionHistoryBytes = new List<byte[]>();
    private readonly List<SessionHistoryData> cachedSessionHistoryList = new List<SessionHistoryData>();

    public IReadOnlyList<SessionHistoryData> SessionHistoryList => cachedSessionHistoryList;

    private string FilePath => Path.Combine(Application.persistentDataPath, "run_history.bytes");

    [Serializable]
    private class SessionHistoryJsonWrapper
    {
        public List<string> sessionsBase64 = new List<string>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log(Application.persistentDataPath);

            if (usePersistentSave)
                LoadFromFile();
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

        byte[] sessionBytes = SerializeSessionToBytes(session);

        sessionHistoryBytes.Insert(0, sessionBytes);
        cachedSessionHistoryList.Insert(0, session);

        TrimHistoryLimit();
        SaveToFile();

        Debug.Log("History kaydı byte[] olarak RAM'e eklendi.");
    }

    public void RemoveSessionAt(int index)
    {
        if (index < 0 || index >= sessionHistoryBytes.Count)
            return;

        sessionHistoryBytes.RemoveAt(index);

        if (index < cachedSessionHistoryList.Count)
            cachedSessionHistoryList.RemoveAt(index);

        SaveToFile();

        Debug.Log("Seçili history kaydı RAM'den ve dosyadan silindi.");
    }

    public bool HasHistory()
    {
        return cachedSessionHistoryList.Count > 0;
    }

    public void ClearHistory()
    {
        sessionHistoryBytes.Clear();
        cachedSessionHistoryList.Clear();

        DeleteHistoryFile();

        Debug.Log("Tüm history RAM'den ve dosyadan temizlendi.");
    }

    public void ClearAllHistory()
    {
        ClearHistory();
    }

    private byte[] SerializeSessionToBytes(SessionHistoryData session)
    {
        string json = JsonUtility.ToJson(session, true);
        return Encoding.UTF8.GetBytes(json);
    }

    private SessionHistoryData DeserializeBytesToSession(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        string json = Encoding.UTF8.GetString(bytes);
        return JsonUtility.FromJson<SessionHistoryData>(json);
    }

    private void TrimHistoryLimit()
    {
        while (sessionHistoryBytes.Count > maxHistoryCount)
            sessionHistoryBytes.RemoveAt(sessionHistoryBytes.Count - 1);

        while (cachedSessionHistoryList.Count > maxHistoryCount)
            cachedSessionHistoryList.RemoveAt(cachedSessionHistoryList.Count - 1);
    }

    private void SaveToFile()
    {
        if (!usePersistentSave)
            return;

        try
        {
            SessionHistoryJsonWrapper wrapper = new SessionHistoryJsonWrapper();

            foreach (byte[] bytes in sessionHistoryBytes)
            {
                if (bytes == null || bytes.Length == 0)
                    continue;

                wrapper.sessionsBase64.Add(Convert.ToBase64String(bytes));
            }

            string wrapperJson = JsonUtility.ToJson(wrapper, true);
            byte[] fileBytes = Encoding.UTF8.GetBytes(wrapperJson);

            File.WriteAllBytes(FilePath, fileBytes);

            Debug.Log("History dosyaya kaydedildi: " + FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("History dosyaya kaydedilemedi: " + e.Message);
        }
    }

    private void LoadFromFile()
    {
        if (!File.Exists(FilePath))
            return;

        try
        {
            byte[] fileBytes = File.ReadAllBytes(FilePath);
            string wrapperJson = Encoding.UTF8.GetString(fileBytes);

            SessionHistoryJsonWrapper wrapper =
                JsonUtility.FromJson<SessionHistoryJsonWrapper>(wrapperJson);

            sessionHistoryBytes.Clear();
            cachedSessionHistoryList.Clear();

            if (wrapper == null || wrapper.sessionsBase64 == null)
                return;

            foreach (string base64 in wrapper.sessionsBase64)
            {
                if (string.IsNullOrEmpty(base64))
                    continue;

                byte[] sessionBytes = Convert.FromBase64String(base64);
                SessionHistoryData session = DeserializeBytesToSession(sessionBytes);

                if (session == null)
                    continue;

                sessionHistoryBytes.Add(sessionBytes);
                cachedSessionHistoryList.Add(session);
            }

            TrimHistoryLimit();

            Debug.Log("History dosyadan RAM'e yüklendi. Kayıt sayısı: " + cachedSessionHistoryList.Count);
        }
        catch (Exception e)
        {
            Debug.LogWarning("History dosyadan okunamadı: " + e.Message);
            sessionHistoryBytes.Clear();
            cachedSessionHistoryList.Clear();
        }
    }

    private void DeleteHistoryFile()
    {
        if (!usePersistentSave)
            return;

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("History dosyası silinemedi: " + e.Message);
        }
    }
}