using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EscalationEngine : MonoBehaviour
{
    public static EscalationEngine Instance { get; private set; }

    [Header("Events")]
    public UnityEvent<int, EscalationEvent> OnEscalationLevelChanged;
    public UnityEvent<string> OnWarningDisplayed;
    public UnityEvent OnAutoFail;
    public UnityEvent<float> OnTimerUpdated;
    public UnityEvent<int> OnScorePenaltyApplied;

    private ScenarioConfig _currentScenario;
    private float _elapsedTime;
    private int _currentLevel;
    private bool _isActive;
    private bool _isPaused;
    private float _totalScorePenalty;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartEscalation(ScenarioConfig scenario)
    {
        _currentScenario = scenario;
        _elapsedTime = 0f;
        _currentLevel = 0;
        _isActive = true;
        _isPaused = false;
        _totalScorePenalty = 0f;

        if (OnEscalationLevelChanged != null)
        {
            var firstEvent = scenario.EscalationEvents[0];
            OnEscalationLevelChanged.Invoke(0, firstEvent);
        }

        if (OnWarningDisplayed != null)
        {
            var firstEvent = scenario.EscalationEvents[0];
            OnWarningDisplayed.Invoke(firstEvent.WarningText);
        }

        Debug.Log($"[Escalation] Started for scenario: {scenario.ScenarioName}");
    }

    void Update()
    {
        if (!_isActive || _isPaused || _currentScenario == null) return;

        _elapsedTime += Time.deltaTime;
        OnTimerUpdated?.Invoke(_elapsedTime);

        CheckEscalationLevels();
    }

    void CheckEscalationLevels()
    {
        for (int i = _currentScenario.EscalationEvents.Count - 1; i >= 0; i--)
        {
            var escalationEvent = _currentScenario.EscalationEvents[i];
            if (_elapsedTime >= escalationEvent.TimeThreshold && i > _currentLevel)
            {
                TriggerEscalation(i, escalationEvent);
                break;
            }
        }
    }

    void TriggerEscalation(int newLevel, EscalationEvent escalationEvent)
    {
        _currentLevel = newLevel;
        _totalScorePenalty += escalationEvent.ScorePenalty;

        Debug.Log($"[Escalation] Level {newLevel}: {escalationEvent.LevelName} - {escalationEvent.WarningText}");

        OnEscalationLevelChanged?.Invoke(newLevel, escalationEvent);
        OnWarningDisplayed?.Invoke(escalationEvent.WarningText);
        OnScorePenaltyApplied?.Invoke((int)escalationEvent.ScorePenalty);

        ApplyVisualEffects(escalationEvent);
        ApplyAudioEffects(escalationEvent);

        if (escalationEvent.TriggerAutoFail)
        {
            Debug.Log("[Escalation] AUTO-FAIL triggered");
            OnAutoFail?.Invoke();
            StopEscalation();
        }
    }

    void ApplyVisualEffects(EscalationEvent escalationEvent)
    {
        switch (escalationEvent.VisualEffect)
        {
            case "yellow_alert":
                HUDManager.Instance?.SetAlertColor(Color.yellow);
                HUDManager.Instance?.ShowAlert(escalationEvent.WarningText);
                break;
            case "orange_alert_flash":
                HUDManager.Instance?.SetAlertColor(new Color(1f, 0.5f, 0f));
                HUDManager.Instance?.ShowAlert(escalationEvent.WarningText);
                StartCoroutine(FlashAlert(0.5f));
                break;
            case "red_alert_shake":
                HUDManager.Instance?.SetAlertColor(Color.red);
                HUDManager.Instance?.ShowAlert(escalationEvent.WarningText);
                StartCoroutine(ScreenShake(0.3f, 0.1f));
                break;
            case "red_screen_fail":
                HUDManager.Instance?.SetAlertColor(Color.red);
                HUDManager.Instance?.ShowCriticalFail(escalationEvent.WarningText);
                break;
        }

        if (escalationEvent.ShowEvacuationArrows)
        {
            ARScenarioPlayer.Instance?.ShowEvacuationArrows();
        }
    }

    void ApplyAudioEffects(EscalationEvent escalationEvent)
    {
        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";

        switch (escalationEvent.AudioEffect)
        {
            case "alarm":
                VoiceModule.Instance?.Speak(escalationEvent.WarningText, lang);
                break;
            case "urgent_alarm":
                VoiceModule.Instance?.Speak(escalationEvent.WarningText, lang);
                break;
            case "critical_alarm":
                VoiceModule.Instance?.Speak(escalationEvent.WarningText, lang);
                break;
            case "buzzer":
                VoiceModule.Instance?.Speak(escalationEvent.WarningText, lang);
                break;
            default:
                VoiceModule.Instance?.Speak(escalationEvent.WarningText, lang);
                break;
        }
    }

    IEnumerator FlashAlert(float interval)
    {
        float flashTimer = 0f;
        bool visible = true;
        while (_currentLevel >= 2 && _isActive)
        {
            flashTimer += Time.deltaTime;
            if (flashTimer >= interval)
            {
                flashTimer = 0f;
                visible = !visible;
                HUDManager.Instance?.SetAlertVisible(visible);
            }
            yield return null;
        }
        HUDManager.Instance?.SetAlertVisible(true);
    }

    IEnumerator ScreenShake(float duration, float magnitude)
    {
        Transform cam = Camera.main?.transform;
        if (cam == null) yield break;

        Vector3 originalPos = cam.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cam.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.localPosition = originalPos;
    }

    public void StopEscalation()
    {
        _isActive = false;
        Debug.Log($"[Escalation] Stopped. Final level: {_currentLevel}, Total penalty: {_totalScorePenalty}%");
    }

    public void PauseEscalation()
    {
        _isPaused = true;
        Debug.Log("[Escalation] Paused");
    }

    public void ResumeEscalation()
    {
        _isPaused = false;
        Debug.Log("[Escalation] Resumed");
    }

    public void ResetEscalation()
    {
        _elapsedTime = 0f;
        _currentLevel = 0;
        _totalScorePenalty = 0f;
        _isActive = false;
        _isPaused = false;
        Debug.Log("[Escalation] Reset");
    }

    public float GetElapsed => _elapsedTime;
    public int GetCurrentLevel => _currentLevel;
    public float GetTotalPenalty => _totalScorePenalty;
    public bool IsActive => _isActive;
    public bool IsPaused => _isPaused;

    public int CalculatePenalty(int baseScore)
    {
        float penaltyMultiplier = _totalScorePenalty / 100f;
        int penalty = Mathf.RoundToInt(baseScore * penaltyMultiplier);
        return Mathf.Max(0, baseScore - penalty);
    }

    public string GetCurrentLevelName()
    {
        if (_currentScenario == null || _currentLevel >= _currentScenario.EscalationEvents.Count)
            return "Unknown";
        return _currentScenario.EscalationEvents[_currentLevel].LevelName;
    }

    public float GetTimeToNextLevel()
    {
        if (_currentScenario == null) return -1f;

        int nextLevel = _currentLevel + 1;
        if (nextLevel >= _currentScenario.EscalationEvents.Count)
            return -1f;

        float nextThreshold = _currentScenario.EscalationEvents[nextLevel].TimeThreshold;
        return Mathf.Max(0f, nextThreshold - _elapsedTime);
    }
}
