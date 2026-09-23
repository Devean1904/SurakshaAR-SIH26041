using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("HUD Panels")]
    public GameObject hudPanel;
    public GameObject alertPanel;
    public GameObject criticalFailPanel;
    public GameObject evacuationPanel;
    public GameObject scorePanel;

    [Header("Hazard Display")]
    public TextMeshProUGUI hazardTypeText;
    public TextMeshProUGUI hazardLevelText;
    public Image hazardLevelBar;
    public Image hazardIndicator;
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = new Color(1f, 0.5f, 0f);
    public Color criticalColor = Color.red;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public Image timerProgressBar;
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.yellow;
    public Color timerCriticalColor = Color.red;

    [Header("Alerts")]
    public TextMeshProUGUI alertText;
    public Image alertBackground;
    public TextMeshProUGUI warningInstructionsText;

    [Header("Actions")]
    public GameObject actionButtonContainer;
    public Button actionButton1;
    public Button actionButton2;
    public Button actionButton3;
    public TextMeshProUGUI actionText1;
    public TextMeshProUGUI actionText2;
    public TextMeshProUGUI actionText3;

    [Header("Critical Fail")]
    public TextMeshProUGUI criticalFailTitle;
    public TextMeshProUGUI criticalFailMessage;
    public TextMeshProUGUI criticalFailScore;
    public Button retryButton;
    public Button viewResultsButton;

    [Header("Evacuation")]
    public TextMeshProUGUI evacuationText;
    public Image evacuationArrow;

    [Header("Score Display")]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI penaltyText;
    public TextMeshProUGUI levelText;

    private float _totalTime;
    private float _currentTime;
    private bool _timerActive;
    private ScenarioConfig _currentScenario;
    private int _currentScore;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (viewResultsButton != null) viewResultsButton.onClick.AddListener(OnViewResultsClicked);
        HideAll();
    }

    public void ShowHUD(ScenarioConfig scenario)
    {
        _currentScenario = scenario;
        _totalTime = scenario.EscalationEvents[scenario.EscalationEvents.Count - 1].TimeThreshold;
        _currentTime = 0f;
        _timerActive = true;
        _currentScore = 100;

        if (hudPanel != null) hudPanel.SetActive(true);
        if (alertPanel != null) alertPanel.SetActive(false);
        if (criticalFailPanel != null) criticalFailPanel.SetActive(false);
        if (evacuationPanel != null) evacuationPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);

        UpdateHazardDisplay(scenario.HazardType, 0f);
        UpdateTimer(0f);
        ShowActionButtons(scenario.RequiredActions);
        UpdateScoreDisplay();

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string hazardName = LanguageManager.Instance?.Get($"scenario_{scenario.HazardType}") ?? scenario.ScenarioName;
        VoiceModule.Instance?.Speak($"{hazardName} detected! Take action immediately!", lang);
    }

    void Update()
    {
        if (!_timerActive) return;
        _currentTime += Time.deltaTime;
        UpdateTimer(_currentTime);
    }

    void UpdateTimer(float time)
    {
        if (timerText == null) return;
        float remaining = Mathf.Max(0, _totalTime - time);
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (timerProgressBar != null)
            timerProgressBar.fillAmount = 1f - (time / _totalTime);

        float ratio = remaining / _totalTime;
        timerText.color = ratio > 0.5f ? timerNormalColor : ratio > 0.25f ? timerWarningColor : timerCriticalColor;
    }

    void UpdateHazardDisplay(string hazardType, float level)
    {
        if (hazardTypeText != null)
            hazardTypeText.text = LanguageManager.Instance?.Get($"scenario_{hazardType}") ?? hazardType;
        if (hazardLevelBar != null)
        {
            hazardLevelBar.fillAmount = level;
            hazardLevelBar.color = GetLevelColor(level);
        }
        if (hazardIndicator != null)
            hazardIndicator.color = GetLevelColor(level);
    }

    Color GetLevelColor(float level)
    {
        if (level < 0.25f) return normalColor;
        if (level < 0.5f) return warningColor;
        if (level < 0.75f) return dangerColor;
        return criticalColor;
    }

    void ShowActionButtons(List<RequiredAction> actions)
    {
        if (actionButtonContainer != null) actionButtonContainer.SetActive(true);
        Button[] buttons = { actionButton1, actionButton2, actionButton3 };
        TextMeshProUGUI[] texts = { actionText1, actionText2, actionText3 };

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < actions.Count)
            {
                if (buttons[i] != null) buttons[i].gameObject.SetActive(true);
                if (texts[i] != null) texts[i].text = actions[i].ActionName;
                int index = i;
                buttons[i]?.onClick.RemoveAllListeners();
                buttons[i]?.onClick.AddListener(() => OnActionButtonClicked(actions[index]));
            }
            else
            {
                if (buttons[i] != null) buttons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnActionButtonClicked(RequiredAction action)
    {
        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak(action.CompletionFeedback, lang);
        if (warningInstructionsText != null) warningInstructionsText.text = action.CompletionFeedback;
        ARScenarioPlayer.Instance?.CompleteAction(action.ActionId);
    }

    public void SetAlertColor(Color color) { if (alertBackground != null) alertBackground.color = color; }
    public void ShowAlert(string message) { if (alertPanel != null) alertPanel.SetActive(true); if (alertText != null) alertText.text = message; }
    public void SetAlertVisible(bool visible) { if (alertPanel != null) alertPanel.SetActive(visible); }

    public void ShowCriticalFail(string message)
    {
        _timerActive = false;
        if (hudPanel != null) hudPanel.SetActive(false);
        if (criticalFailPanel != null) criticalFailPanel.SetActive(true);
        if (criticalFailTitle != null) criticalFailTitle.text = "SCENARIO FAILED";
        if (criticalFailMessage != null) criticalFailMessage.text = message;
        if (criticalFailScore != null) criticalFailScore.text = "Score: 0 / 100";
    }

    public void ShowEvacuation(string message)
    {
        if (evacuationPanel != null) evacuationPanel.SetActive(true);
        if (evacuationText != null) evacuationText.text = message;
    }

    public void ApplyScorePenalty(int penalty)
    {
        _currentScore = Mathf.Max(0, _currentScore - penalty);
        if (penaltyText != null) penaltyText.text = $"Penalty: -{penalty}%";
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (currentScoreText != null) currentScoreText.text = $"Score: {_currentScore}";
    }

    public void ShowResults(int actionScore, int questionScore, int totalScore, bool passed)
    {
        _timerActive = false;
        if (hudPanel != null) hudPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(true);

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string resultText = passed ? LanguageManager.Instance?.Get("training_passed") ?? "PASSED" : LanguageManager.Instance?.Get("training_failed") ?? "FAILED";
        VoiceModule.Instance?.Speak($"Training complete. You {resultText}. Score: {totalScore}", lang);
    }

    void OnRetryClicked()
    {
        HideAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    void OnViewResultsClicked()
    {
        if (scorePanel != null) scorePanel.SetActive(true);
        if (criticalFailPanel != null) criticalFailPanel.SetActive(false);
    }

    public void HideAll()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (alertPanel != null) alertPanel.SetActive(false);
        if (criticalFailPanel != null) criticalFailPanel.SetActive(false);
        if (evacuationPanel != null) evacuationPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
    }
}
