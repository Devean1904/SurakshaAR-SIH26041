using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class OfflineManager : MonoBehaviour
{
    public static OfflineManager Instance { get; private set; }

    private string OfflineDataPath => Path.Combine(Application.persistentDataPath, "offline_data.json");
    private string ModulesPath => Path.Combine(Application.persistentDataPath, "offline_modules.json");
    private string CertsPath => Path.Combine(Application.persistentDataPath, "offline_certs.json");

    public bool IsOnline { get; private set; } = true;
    public bool IsOfflineMode { get; private set; } = false;

    private OfflineSaveData _cachedData = new();
    private List<string> _cachedModules = new();
    private List<string> _cachedCertificates = new();
    private string baseUrl => ServerConfig.Instance.ApiBase;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CheckConnectivity();
        InvokeRepeating(nameof(CheckConnectivity), 5f, 10f);
        LoadAllCachedData();
    }

    void CheckConnectivity()
    {
        IsOnline = Application.internetReachability != NetworkReachability.NotReachable;
        if (IsOnline && _cachedData.PendingEscalations.Count > 0)
        {
            StartCoroutine(SyncToServer());
        }
    }

    void LoadAllCachedData()
    {
        if (File.Exists(ModulesPath))
            _cachedModules = new List<string> { File.ReadAllText(ModulesPath) };

        if (File.Exists(CertsPath))
            _cachedCertificates = new List<string> { File.ReadAllText(CertsPath) };

        if (File.Exists(OfflineDataPath))
        {
            var json = File.ReadAllText(OfflineDataPath);
            _cachedData = JsonUtility.FromJson<OfflineSaveData>(json);
            Debug.Log($"[Offline] Loaded {_cachedData.Scenarios.Count} scenarios, {_cachedData.Progress.Count} progress entries");
        }
    }

    public void LoadOfflineSession()
    {
        IsOfflineMode = true;
        LoadAllCachedData();
    }

    public void CacheForOffline(OfflineSaveData data)
    {
        _cachedData = data;
        SaveToFile(OfflineDataPath, _cachedData);
        Debug.Log("[Offline] Data cached for offline use");
    }

    public void CacheModuleConfig(string moduleJson)
    {
        if (!_cachedModules.Contains(moduleJson))
            _cachedModules.Add(moduleJson);
        File.WriteAllText(ModulesPath, JsonUtility.ToJson(new ModuleCacheWrapper { Modules = _cachedModules }, true));
        Debug.Log("[Offline] Module config cached");
    }

    public void CacheCertificate(string certJson)
    {
        if (!_cachedCertificates.Contains(certJson))
            _cachedCertificates.Add(certJson);
        File.WriteAllText(CertsPath, JsonUtility.ToJson(new CertCacheWrapper { Certificates = _cachedCertificates }, true));
        Debug.Log("[Offline] Certificate cached");
    }

    public void SaveProgress(string scenarioId, string status)
    {
        var entry = new OfflineProgress
        {
            ScenarioId = scenarioId,
            Status = status,
            Timestamp = System.DateTime.UtcNow.ToString("O")
        };
        _cachedData.Progress.Add(entry);
        SaveToFile(OfflineDataPath, _cachedData);
    }

    public void SaveTrainingResult(string moduleId, int score, bool passed)
    {
        var result = new OfflineTrainingResult
        {
            ModuleId = moduleId,
            Score = score,
            Passed = passed,
            Timestamp = System.DateTime.UtcNow.ToString("O")
        };
        _cachedData.TrainingResults.Add(result);
        SaveToFile(OfflineDataPath, _cachedData);
    }

    public void SaveEscalation(string reportJson)
    {
        _cachedData.PendingEscalations.Add(reportJson);
        SaveToFile(OfflineDataPath, _cachedData);
    }

    public bool HasOfflineModule(string moduleId)
    {
        return _cachedModules.Exists(m => m.Contains(moduleId));
    }

    public List<string> GetCachedCertificates() => _cachedCertificates;

    public bool TrySyncToServer()
    {
        if (!IsOnline || _cachedData.PendingEscalations.Count == 0)
            return false;
        StartCoroutine(SyncToServer());
        return true;
    }

    IEnumerator SyncToServer()
    {
        if (!IsOnline) yield break;

        Debug.Log($"[Sync] Uploading {_cachedData.Progress.Count} progress, {_cachedData.TrainingResults.Count} results, {_cachedData.PendingEscalations.Count} escalations");

        foreach (var result in _cachedData.TrainingResults)
        {
            var body = JsonUtility.ToJson(result);
            using var req = new UnityWebRequest($"{baseUrl}/training/complete", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log($"[Sync] Training result synced: {result.ModuleId}");
            else
                Debug.LogWarning($"[Sync] Failed to sync result: {req.error}");
        }

        foreach (var escalationJson in _cachedData.PendingEscalations)
        {
            using var req = new UnityWebRequest($"{baseUrl}/worker/online/sync", "POST");
            var body = JsonUtility.ToJson(new SyncPayload
            {
                CompletedScenarios = new List<ScenarioSync>(),
                Escalations = new List<EscalationReport> { JsonUtility.FromJson<EscalationReport>(escalationJson) }
            });
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log("[Sync] Escalation synced");
            else
                Debug.LogWarning($"[Sync] Failed to sync escalation: {req.error}");
        }

        _cachedData.Progress.Clear();
        _cachedData.TrainingResults.Clear();
        _cachedData.PendingEscalations.Clear();
        _cachedData.LastSyncTime = System.DateTime.UtcNow.ToString("O");
        IsOfflineMode = false;

        SaveToFile(OfflineDataPath, _cachedData);
        Debug.Log("[Sync] All data synced successfully");
    }

    void SaveToFile(string path, object data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public OfflineSaveData GetCachedData() => _cachedData;
}

[System.Serializable]
public class OfflineSaveData
{
    public List<OfflineScenarioData> Scenarios = new();
    public List<OfflineProgress> Progress = new();
    public List<OfflineTrainingResult> TrainingResults = new();
    public List<string> PendingEscalations = new();
    public string LastSyncTime = "";
    public string UserId = "";
}

[System.Serializable]
public class OfflineScenarioData
{
    public string Id;
    public string SiteName;
    public string ScenarioType;
    public float[] Position;
    public string Description;
    public string RiskLevel;
}

[System.Serializable]
public class OfflineProgress
{
    public string ScenarioId;
    public string Status;
    public string Timestamp;
}

[System.Serializable]
public class OfflineTrainingResult
{
    public string ModuleId;
    public int Score;
    public bool Passed;
    public string Timestamp;
}

[System.Serializable]
public class SyncPayload
{
    public List<ScenarioSync> CompletedScenarios = new();
    public List<EscalationReport> Escalations = new();
}

[System.Serializable]
public class ScenarioSync
{
    public string Id;
}

[System.Serializable]
public class EscalationReport
{
    public string Id;
    public string WorkerId;
    public string Description;
    public string Severity;
}

[System.Serializable]
public class ModuleCacheWrapper
{
    public List<string> Modules = new();
}

[System.Serializable]
public class CertCacheWrapper
{
    public List<string> Certificates = new();
}