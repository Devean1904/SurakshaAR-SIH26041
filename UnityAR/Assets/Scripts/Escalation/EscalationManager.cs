using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EscalationManager : MonoBehaviour
{
    public static EscalationManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject escalationPanel;
    public TMP_Dropdown severityDropdown;
    public TMP_InputField descriptionInput;
    public Button reportButton;
    public Button resolveButton;
    public Transform escalationListParent;
    public GameObject escalationItemPrefab;

    private List<EscalationEntry> _escalations = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (reportButton != null)
            reportButton.onClick.AddListener(OnReportEscalation);
        if (resolveButton != null)
            resolveButton.onClick.AddListener(OnResolveEscalation);

        if (severityDropdown != null)
        {
            severityDropdown.ClearOptions();
            severityDropdown.AddOptions(new List<string> { "Critical", "High", "Medium", "Low" });
        }
    }

    public void OnReportEscalation()
    {
        if (descriptionInput == null) return;
        var desc = descriptionInput.text;
        if (string.IsNullOrEmpty(desc)) return;

        var severity = severityDropdown != null ? severityDropdown.options[severityDropdown.value].text : "Medium";
        string userId = PlayerPrefs.GetString("UserId", "unknown");

        var entry = new EscalationEntry
        {
            Id = System.Guid.NewGuid().ToString(),
            WorkerId = userId,
            Description = desc,
            Severity = severity,
            Timestamp = System.DateTime.UtcNow.ToString("O"),
            Status = "Active"
        };

        string hash = LocalBlockchain.Instance.AddBlock("escalation", userId, desc);
        entry.BlockchainHash = hash;

        _escalations.Add(entry);

        if (!OfflineManager.Instance.IsOnline)
        {
            OfflineManager.Instance.SaveEscalation(JsonUtility.ToJson(entry));
            Debug.Log("[Escalation] Saved offline - will sync when online");
        }
        else
        {
            StartCoroutine(SyncEscalation(entry));
        }

        descriptionInput.text = "";
        RefreshUI();
    }

    System.Collections.IEnumerator SyncEscalation(EscalationEntry entry)
    {
        var body = JsonUtility.ToJson(entry);
        using var req = new UnityEngine.Networking.UnityWebRequest(
            $"{ServerConfig.Instance.ApiBase}/worker/escalation/report", "POST");
        req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            Debug.Log("[Escalation] Synced to server");
        else
            Debug.Log("[Escalation] Server sync failed - stored offline");
    }

    void OnResolveEscalation()
    {
        if (_escalations.Count == 0) return;

        var activeEscalation = _escalations.Find(e => e.Status == "Active");
        if (activeEscalation == null) return;

        activeEscalation.Status = "Resolved";
        Debug.Log($"[Escalation] Resolved escalation: {activeEscalation.Id}");

        if (OfflineManager.Instance.IsOnline)
        {
            StartCoroutine(ResolveOnServer(activeEscalation.Id));
        }

        RefreshUI();
    }

    System.Collections.IEnumerator ResolveOnServer(string escalationId)
    {
        var body = JsonUtility.ToJson(new ResolvePayload { ReportId = escalationId });
        using var req = new UnityEngine.Networking.UnityWebRequest(
            $"{ServerConfig.Instance.ApiBase}/admin/escalation/resolve", "POST");
        req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            Debug.Log("[Escalation] Resolved on server");
        else
            Debug.LogWarning($"[Escalation] Failed to resolve on server: {req.error}");
    }

    [System.Serializable]
    class ResolvePayload
    {
        public string ReportId;
    }

    void RefreshUI()
    {
        if (escalationListParent == null || escalationItemPrefab == null) return;
        foreach (Transform child in escalationListParent)
            Destroy(child.gameObject);

        foreach (var e in _escalations)
        {
            var item = Instantiate(escalationItemPrefab, escalationListParent);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                var statusIcon = e.Status == "Active" ? "!" : "V";
                text.text = $"[{statusIcon}] {e.Severity} - {e.Description}";
            }
        }
    }
}

[System.Serializable]
public class EscalationEntry
{
    public string Id;
    public string WorkerId;
    public string Description;
    public string Severity;
    public float[] Location;
    public string Timestamp;
    public string Status;
    public string BlockchainHash;
}
