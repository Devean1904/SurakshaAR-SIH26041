using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Collections.Generic;

public class QRCheckpointScanner : MonoBehaviour
{
    public static QRCheckpointScanner Instance { get; private set; }

    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Checkpoint Data")]
    public TextAsset checkpointDatabase;

    [Header("UI")]
    public GameObject scanPanel;
    public TMPro.TextMeshProUGUI scanStatusText;
    public UnityEngine.UI.Button scanButton;

    private Dictionary<string, ScenarioConfig> _checkpoints = new();
    private bool _isScanning = false;
    private string _lastScannedQR = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LoadCheckpoints();
        if (scanButton != null)
            scanButton.onClick.AddListener(ToggleScanning);
    }

    void LoadCheckpoints()
    {
        _checkpoints.Clear();

        _checkpoints["QR-FIRE-001"] = TrainingDataStore.GetScenario("fire-safety-101", "fire-scenario-001")
            ?? CreateFireCheckpoint();
        _checkpoints["QR-GAS-001"] = TrainingDataStore.GetScenario("gas-leak-101", "gas-scenario-001")
            ?? CreateGasCheckpoint();

        Debug.Log($"[QRScanner] Loaded {_checkpoints.Count} checkpoints");
    }

    ScenarioConfig CreateFireCheckpoint()
    {
        var module = TrainingDataStore.GetModule("fire-safety-101");
        if (module != null && module.Scenarios.Count > 0)
            return module.Scenarios[0];
        return null;
    }

    ScenarioConfig CreateGasCheckpoint()
    {
        var module = TrainingDataStore.GetModule("gas-leak-101");
        if (module != null && module.Scenarios.Count > 0)
            return module.Scenarios[0];
        return null;
    }

    public void ToggleScanning()
    {
        if (_isScanning)
            StopScanning();
        else
            StartScanning();
    }

    public void StartScanning()
    {
        _isScanning = true;
        if (scanPanel != null) scanPanel.SetActive(true);
        if (scanStatusText != null) scanStatusText.text = LanguageManager.Instance?.Get("qr_scan_prompt") ?? "Scan QR Code";

        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("qr_scan_prompt") ?? "Scan the QR code at the checkpoint", lang);

        Debug.Log("[QRScanner] Scanning started");
    }

    public void StopScanning()
    {
        _isScanning = false;

        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        Debug.Log("[QRScanner] Scanning stopped");
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            ProcessTrackedImage(trackedImage.referenceImage.name);
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                ProcessTrackedImage(trackedImage.referenceImage.name);
            }
        }
    }

    void ProcessTrackedImage(string imageName)
    {
        if (_lastScannedQR == imageName) return;
        _lastScannedQR = imageName;

        string checkpointId = ExtractCheckpointId(imageName);
        if (string.IsNullOrEmpty(checkpointId)) return;

        if (_checkpoints.ContainsKey(checkpointId))
        {
            Debug.Log($"[QRScanner] Checkpoint found: {checkpointId}");
            OnCheckpointFound(checkpointId, _checkpoints[checkpointId]);
        }
        else
        {
            Debug.Log($"[QRScanner] Unknown checkpoint: {checkpointId}");
            OnInvalidCheckpoint(checkpointId);
        }
    }

    string ExtractCheckpointId(string imageName)
    {
        if (imageName.StartsWith("QR-"))
            return imageName;

        if (imageName.Contains("fire") || imageName.Contains("Fire"))
            return "QR-FIRE-001";
        if (imageName.Contains("gas") || imageName.Contains("Gas"))
            return "QR-GAS-001";

        return imageName;
    }

    void OnCheckpointFound(string checkpointId, ScenarioConfig scenario)
    {
        StopScanning();

        if (scanStatusText != null)
            scanStatusText.text = LanguageManager.Instance?.Get("qr_checkpoint_found") ?? "Checkpoint found!";

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak(
            LanguageManager.Instance?.Get("qr_loading_scenario") ?? "Loading scenario...",
            lang
        );

        StartCoroutine(LoadScenarioAfterDelay(scenario));
    }

    IEnumerator LoadScenarioAfterDelay(ScenarioConfig scenario)
    {
        yield return new WaitForSeconds(1f);

        if (ARScenarioPlayer.Instance != null)
        {
            ARScenarioPlayer.Instance.SpawnHazard(scenario);
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowHUD(scenario);
        }

        if (EscalationEngine.Instance != null)
        {
            EscalationEngine.Instance.StartEscalation(scenario);
        }

        Debug.Log($"[QRScanner] Scenario loaded: {scenario.ScenarioName}");
    }

    void OnInvalidCheckpoint(string checkpointId)
    {
        if (scanStatusText != null)
            scanStatusText.text = LanguageManager.Instance?.Get("qr_invalid_code") ?? "Invalid QR code!";

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak(
            LanguageManager.Instance?.Get("qr_invalid_code") ?? "Invalid QR code",
            lang
        );

        StartCoroutine(ResetScanAfterDelay(2f));
    }

    IEnumerator ResetScanAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _lastScannedQR = "";
        if (scanStatusText != null)
            scanStatusText.text = LanguageManager.Instance?.Get("qr_scan_prompt") ?? "Scan QR Code";
    }

    public void ManualCheckpointSelect(string checkpointId)
    {
        if (_checkpoints.ContainsKey(checkpointId))
        {
            OnCheckpointFound(checkpointId, _checkpoints[checkpointId]);
        }
    }

    void OnDestroy()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
}