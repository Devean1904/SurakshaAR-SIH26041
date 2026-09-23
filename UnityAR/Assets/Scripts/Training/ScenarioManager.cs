using UnityEngine;
using System.Collections;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }

    [Header("Current State")]
    private string _currentModuleId;
    private ScenarioConfig _currentScenario;
    private int _actionScore;
    private int _actionsCompleted;
    private int _totalActions;
    private bool _scenarioActive;
    private float _startTime;

    [Header("Detection Mode")]
    [SerializeField] private bool _waitForSurfaceDetection = true;
    [SerializeField] private float _surfaceDetectionTimeout = 15f;
    private bool _surfaceDetected;
    private bool _waitingForTap;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartScenario(string moduleId, string scenarioId)
    {
        var module = TrainingDataStore.GetModule(moduleId);
        if (module == null)
        {
            Debug.LogError($"[ScenarioManager] Module not found: {moduleId}");
            return;
        }

        var scenario = module.Scenarios.Find(s => s.ScenarioId == scenarioId);
        if (scenario == null && module.Scenarios.Count > 0)
            scenario = module.Scenarios[0];

        if (scenario == null)
        {
            Debug.LogError($"[ScenarioManager] Scenario not found: {scenarioId}");
            return;
        }

        _currentModuleId = moduleId;
        _currentScenario = scenario;
        _actionScore = 100;
        _actionsCompleted = 0;
        _totalActions = scenario.RequiredActions.Count;
        _scenarioActive = true;
        _startTime = Time.time;
        _surfaceDetected = false;
        _waitingForTap = false;

        Debug.Log($"[ScenarioManager] Starting: {scenario.ScenarioName} ({scenario.HazardType})");

        if (_waitForSurfaceDetection)
        {
            StartSurfaceDetectionMode(scenario);
        }
        else
        {
            SpawnHazardVisual(scenario);
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(scenario);

        if (EscalationEngine.Instance != null)
        {
            EscalationEngine.Instance.OnScorePenaltyApplied.AddListener(OnPenaltyApplied);
            EscalationEngine.Instance.OnAutoFail.AddListener(OnAutoFail);
            EscalationEngine.Instance.StartEscalation(scenario);
        }

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string startMsg = LanguageManager.Instance?.Get("voice_training_start") ?? "Training starting";
        VoiceModule.Instance?.Speak(startMsg, lang);
    }

    void StartSurfaceDetectionMode(ScenarioConfig scenario)
    {
        Debug.Log("[ScenarioManager] Waiting for surface detection...");

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string scanMsg = LanguageManager.Instance?.Get("voice_scan_surface") ?? "Point your camera at a flat surface";
        VoiceModule.Instance?.Speak(scanMsg, lang);

        if (SurfaceDetector.Instance != null)
        {
            SurfaceDetector.Instance.OnSurfaceDetected += OnSurfaceDetected;
        }

        StartCoroutine(SurfaceDetectionTimeout());
    }

    void OnSurfaceDetected(UnityEngine.Pose pose, UnityEngine.XR.ARFoundation.ARPlane plane)
    {
        if (_surfaceDetected) return;

        _surfaceDetected = true;
        _waitingForTap = true;

        Debug.Log($"[ScenarioManager] Surface detected: {plane.alignment}");

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string tapMsg = LanguageManager.Instance?.Get("voice_tap_to_place") ?? "Tap to place the hazard";
        VoiceModule.Instance?.Speak(tapMsg, lang);

        if (SurfaceDetector.Instance != null)
        {
            SurfaceDetector.Instance.OnSurfaceDetected -= OnSurfaceDetected;
        }
    }

    IEnumerator SurfaceDetectionTimeout()
    {
        yield return new WaitForSeconds(_surfaceDetectionTimeout);

        if (!_surfaceDetected)
        {
            Debug.Log("[ScenarioManager] Surface detection timeout — using default placement");
            SpawnHazardVisual(_currentScenario);
        }
    }

    void Update()
    {
        if (!_scenarioActive || !_waitingForTap) return;

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _waitingForTap = false;

                if (ARScenarioPlayer.Instance != null)
                {
                    ARScenarioPlayer.Instance.OnTapToPlaceHazard(touch.position);
                }

                string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
                string hazardMsg = LanguageManager.Instance?.Get("voice_hazard_placed") ?? "Hazard placed. Start the scenario!";
                VoiceModule.Instance?.Speak(hazardMsg, lang);
            }
        }
    }

    void SpawnHazardVisual(ScenarioConfig scenario)
    {
        switch (scenario.HazardType)
        {
            case "fire":
                if (FireSimulation.Instance != null)
                    FireSimulation.Instance.StartFire();
                else
                    ARScenarioPlayer.Instance?.SpawnHazard(scenario);
                break;

            case "gas_leak":
            case "gas":
                if (GasLeakSimulation.Instance != null)
                    GasLeakSimulation.Instance.StartGasLeak();
                else
                    ARScenarioPlayer.Instance?.SpawnHazard(scenario);
                break;

            default:
                ARScenarioPlayer.Instance?.SpawnHazard(scenario);
                break;
        }
    }

    public void OnActionCompleted(string actionId)
    {
        if (!_scenarioActive) return;

        var action = _currentScenario.RequiredActions.Find(a => a.ActionId == actionId);
        if (action == null) return;

        _actionsCompleted++;
        _actionScore += action.ScoreValue;
        _actionScore = Mathf.Min(_actionScore, 100);

        Debug.Log($"[ScenarioManager] Action completed: {actionId} (Score: {_actionScore})");

        HandleActionEffect(actionId);

        if (_actionsCompleted >= _totalActions)
        {
            CompleteScenario();
        }
    }

    void HandleActionEffect(string actionId)
    {
        switch (actionId)
        {
            case "use-extinguisher":
                if (FireSimulation.Instance != null)
                    FireSimulation.Instance.ExtinguishFire();
                break;

            case "evacuate":
            case "evacuate-zone":
                if (EscalationEngine.Instance != null)
                    EscalationEngine.Instance.StopEscalation();
                break;

            case "activate-ventilation":
                if (GasLeakSimulation.Instance != null)
                    GasLeakSimulation.Instance.ActivateVentilation();
                break;

            case "detect-gas":
            case "detect-gas-pipe":
            case "detect-gas-floor":
            case "detect-gas-duct":
                if (GasLeakSimulation.Instance != null)
                    GasLeakSimulation.Instance.DetectGas();
                break;

            case "detect-fire-pipe":
            case "detect-fire-wall":
            case "detect-fire-floor":
                if (FireSimulation.Instance != null)
                    FireSimulation.Instance.StartFire();
                break;

            case "raise-alarm":
                string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
                VoiceModule.Instance?.Speak(
                    LanguageManager.Instance?.Get("feedback_alarm_raised") ?? "Alarm raised!", lang);
                break;
        }

        ARScenarioPlayer.Instance?.CompleteAction(actionId);
    }

    void OnPenaltyApplied(int penalty)
    {
        _actionScore -= penalty;
        _actionScore = Mathf.Max(0, _actionScore);

        if (HUDManager.Instance != null)
            HUDManager.Instance.ApplyScorePenalty(penalty);

        Debug.Log($"[ScenarioManager] Penalty applied: -{penalty}% (Score: {_actionScore})");
    }

    void OnAutoFail()
    {
        _scenarioActive = false;
        _actionScore = 0;

        Debug.Log("[ScenarioManager] AUTO-FAIL triggered");

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak(
            LanguageManager.Instance?.Get("escalation_critical") ?? "Scenario failed!", lang);

        StartCoroutine(ShowAssessmentAfterDelay(2f));
    }

    void CompleteScenario()
    {
        _scenarioActive = false;

        if (EscalationEngine.Instance != null)
            EscalationEngine.Instance.StopEscalation();

        float responseTime = Time.time - _startTime;
        int timeBonus = responseTime < 15f ? 10 : responseTime < 30f ? 5 : 0;
        _actionScore = Mathf.Min(100, _actionScore + timeBonus);

        Debug.Log($"[ScenarioManager] Scenario complete! Action Score: {_actionScore}, Time: {responseTime:F1}s");

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak(
            LanguageManager.Instance?.Get("training_complete") ?? "Training complete!", lang);

        StartCoroutine(ShowAssessmentAfterDelay(1.5f));
    }

    IEnumerator ShowAssessmentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (HUDManager.Instance != null)
            HUDManager.Instance.HideAll();

        if (AssessmentEngine.Instance != null)
            AssessmentEngine.Instance.StartAssessment(_currentModuleId, _actionScore);
    }

    public void OnCertificateGenerated()
    {
        int certCount = PlayerPrefs.GetInt("CertCount", 0) + 1;
        PlayerPrefs.SetInt("CertCount", certCount);

        int modules = PlayerPrefs.GetInt("ModulesCompleted", 0) + 1;
        PlayerPrefs.SetInt("ModulesCompleted", modules);
        PlayerPrefs.Save();

        Debug.Log($"[ScenarioManager] Certificate generated. Total certs: {certCount}");
    }

    public string GetCurrentModuleId() => _currentModuleId;
    public ScenarioConfig GetCurrentScenario() => _currentScenario;
    public int GetActionScore() => _actionScore;
    public bool IsScenarioActive() => _scenarioActive;
}