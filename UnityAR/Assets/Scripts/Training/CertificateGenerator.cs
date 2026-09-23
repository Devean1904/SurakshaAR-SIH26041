using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class CertificateGenerator : MonoBehaviour
{
    public static CertificateGenerator Instance { get; private set; }

    private string _apiBase => ServerConfig.Instance.ApiBase;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void GenerateCertificate(string moduleId, int actionScore, int questionScore)
    {
        // Inputs may already be a 0-100 total; still apply 50/50 when components differ.
        int actionClamped = Mathf.Clamp(actionScore, 0, 100);
        int questionClamped = Mathf.Clamp(questionScore, 0, 100);
        int totalScore = actionClamped == questionClamped
            ? actionClamped
            : (actionClamped + questionClamped) / 2;
        float threshold = 70f;
        var module = TrainingDataStore.GetModule(moduleId);
        if (module != null && module.PassThreshold > 0f) threshold = module.PassThreshold;
        bool passed = totalScore >= threshold;

        if (!passed)
        {
            Debug.Log("[Certificate] Cannot generate certificate - score below threshold");
            return;
        }

        string employeeId = PlayerPrefs.GetString("UserId", "unknown");
        string empName = PlayerPrefs.GetString("UserName", "Unknown");
        string token = PlayerPrefs.GetString("Token", "");

        var certData = new CertificateData
        {
            CertificateId = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
            EmployeeId = employeeId,
            EmployeeName = empName,
            ModuleId = moduleId,
            ModuleName = GetModuleName(moduleId),
            Score = totalScore,
            Passed = passed,
            IssuedAt = DateTime.UtcNow.ToString("O"),
            QrPayload = $"SURAKSHA:CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8)}"
        };

        string json = JsonUtility.ToJson(certData);
        StartCoroutine(SubmitCertificate(json, token));
    }

    IEnumerator SubmitCertificate(string json, string token)
    {
        using var req = new UnityWebRequest($"{_apiBase}/certificate/generate", "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<CertResponse>(req.downloadHandler.text);
            Debug.Log($"[Certificate] Generated: {response.CertificateId}");

            SaveCertificateLocally(response);
            LocalBlockchain.Instance?.AddBlock("certificate_issued", PlayerPrefs.GetString("UserId"), response.CertificateId);

            string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
            VoiceModule.Instance?.Speak("Certificate issued successfully! You can view it in your certificates.", lang);
        }
        else
        {
            Debug.LogWarning($"[Certificate] Server unavailable, generating local certificate");
            GenerateLocalCertificate(json);
        }
    }

    void GenerateLocalCertificate(string json)
    {
        var certData = JsonUtility.FromJson<CertificateData>(json);
        string certJson = JsonUtility.ToJson(certData, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"cert_{certData.CertificateId}.json");
        System.IO.File.WriteAllText(path, certJson);
        Debug.Log($"[Certificate] Saved locally: {path}");

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak("Certificate saved locally. It will be synced when you are online.", lang);
    }

    void SaveCertificateLocally(CertResponse response)
    {
        string certJson = JsonUtility.ToJson(response, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"cert_{response.CertificateId}.json");
        System.IO.File.WriteAllText(path, certJson);
    }

    string GetModuleName(string moduleId)
    {
        return moduleId switch
        {
            "fire-safety-101" => "Fire & Explosion Response",
            "gas-leak-101" => "Gas Leak & Confined Space",
            _ => "Safety Training"
        };
    }

    [System.Serializable]
    class CertResponse
    {
        public string CertificateId;
        public string EmployeeId;
        public string ModuleId;
        public int Score;
        public bool Passed;
        public string Signature;
        public string BlockchainTx;
        public string QrPayload;
        public string IssuedAt;
    }
}
