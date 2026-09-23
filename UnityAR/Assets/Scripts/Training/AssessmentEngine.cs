using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AssessmentEngine : MonoBehaviour
{
    public static AssessmentEngine Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject assessmentPanel;
    public GameObject questionPanel;
    public GameObject resultPanel;

    [Header("Question Display")]
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI questionText;
    public Image progressBar;
    public Button[] optionButtons = new Button[4];
    public TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
    public Button nextButton;

    [Header("Result Display")]
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI actionScoreText;
    public TextMeshProUGUI questionScoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI resultMessageText;
    public Image resultIndicator;
    public Button viewCertificateButton;
    public Button retryButton;
    public Button homeButton;

    private string _moduleId;
    private List<AssessmentQuestion> _questions;
    private int _currentQuestionIndex;
    private int _correctAnswers;
    private int _totalQuestionScore;
    private int _actionScore;
    private int _selectedOption;
    private List<int> _userAnswers;
    private bool _answeredCurrent;
    private int _lastTotalScore;
    private bool _lastPassed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(OnNextQuestion);
        if (viewCertificateButton != null) viewCertificateButton.onClick.AddListener(OnViewCertificate);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (homeButton != null) homeButton.onClick.AddListener(OnGoHome);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i]?.onClick.AddListener(() => OnOptionSelected(index));
        }
        HideAll();
    }

    public void StartAssessment(string moduleId, int actionScore)
    {
        _moduleId = moduleId;
        _actionScore = actionScore;
        _currentQuestionIndex = 0;
        _correctAnswers = 0;
        _totalQuestionScore = 0;
        _userAnswers = new List<int>();

        _questions = TrainingDataStore.GetRandomQuestions(moduleId, 5);
        if (_questions.Count == 0)
        {
            FinishAssessment();
            return;
        }
        Show();
        ShowQuestion(0);
        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        VoiceModule.Instance?.Speak("Assessment started. Answer the questions.", lang);
    }

    void ShowQuestion(int index)
    {
        if (index >= _questions.Count) { FinishAssessment(); return; }

        var question = _questions[index];
        _selectedOption = -1;
        _answeredCurrent = false;

        if (questionPanel != null) questionPanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);
        if (nextButton != null) nextButton.interactable = false;

        if (questionNumberText != null)
            questionNumberText.text = $"Question {index + 1} / {_questions.Count}";
        if (questionText != null)
            questionText.text = question.QuestionText;
        if (progressBar != null)
            progressBar.fillAmount = (float)(index + 1) / _questions.Count;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < question.Options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionTexts[i].text = question.Options[i];
                optionButtons[i].interactable = true;
                var colors = optionButtons[i].colors;
                colors.normalColor = Color.white;
                optionButtons[i].colors = colors;
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnOptionSelected(int index)
    {
        if (_answeredCurrent) return;
        _selectedOption = index;
        _answeredCurrent = true;

        var question = _questions[_currentQuestionIndex];
        bool correct = (index == question.CorrectAnswerIndex);

        if (correct)
        {
            _correctAnswers++;
            _totalQuestionScore += question.ScoreValue;
            var colors = optionButtons[index].colors;
            colors.normalColor = Color.green;
            optionButtons[index].colors = colors;
        }
        else
        {
            var colors = optionButtons[index].colors;
            colors.normalColor = Color.red;
            optionButtons[index].colors = colors;

            if (question.CorrectAnswerIndex < optionButtons.Length)
            {
                var correctColors = optionButtons[question.CorrectAnswerIndex].colors;
                correctColors.normalColor = Color.green;
                optionButtons[question.CorrectAnswerIndex].colors = correctColors;
            }
        }

        _userAnswers.Add(index);

        foreach (var btn in optionButtons)
            if (btn != null) btn.interactable = false;

        if (nextButton != null) nextButton.interactable = true;

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string feedback = correct ? "Correct!" : $"Incorrect. {question.Explanation}";
        VoiceModule.Instance?.Speak(feedback, lang);
    }

    void OnNextQuestion()
    {
        _currentQuestionIndex++;
        if (_currentQuestionIndex >= _questions.Count)
            FinishAssessment();
        else
            ShowQuestion(_currentQuestionIndex);
    }

    void FinishAssessment()
    {
        if (questionPanel != null) questionPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);

        int maxQuestionScore = 0;
        foreach (var q in _questions) maxQuestionScore += q.ScoreValue;
        if (maxQuestionScore <= 0) maxQuestionScore = 1;

        // 50/50 weighting — matches server AssessmentController and Android.
        int questionPercent = (_totalQuestionScore * 100) / maxQuestionScore;
        if (questionPercent > 100) questionPercent = 100;
        int actionClamped = Mathf.Clamp(_actionScore, 0, 100);
        int totalScore = (actionClamped + questionPercent) / 2;
        float threshold = 70f;
        var module = TrainingDataStore.GetModule(_moduleId);
        if (module != null && module.PassThreshold > 0f) threshold = module.PassThreshold;
        bool passed = totalScore >= threshold;
        _lastTotalScore = totalScore;
        _lastPassed = passed;

        if (actionScoreText != null) actionScoreText.text = $"Action Score: {_actionScore}/100 (50%)";
        if (questionScoreText != null) questionScoreText.text = $"Question Score: {questionPercent}/100 (50%)";
        if (totalScoreText != null) totalScoreText.text = $"Total: {totalScore}/100";
        if (resultTitleText != null) resultTitleText.text = passed ? "PASSED" : "FAILED";
        if (resultMessageText != null) resultMessageText.text = passed ? "Congratulations! You passed the assessment." : "You did not meet the passing threshold. Please try again.";

        if (resultIndicator != null)
            resultIndicator.color = passed ? Color.green : Color.red;

        if (viewCertificateButton != null) viewCertificateButton.gameObject.SetActive(passed);
        if (retryButton != null) retryButton.gameObject.SetActive(!passed);

        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string result = passed ? "passed" : "failed";
        VoiceModule.Instance?.Speak($"Assessment complete. You {result}. Total score: {totalScore} out of 100.", lang);

        LocalBlockchain.Instance?.AddBlock("assessment_completed", PlayerPrefs.GetString("UserId"), $"Score: {totalScore}, Passed: {passed}");

        Debug.Log($"[Assessment] Finished: Score={totalScore}, Passed={passed}");
    }

    void OnViewCertificate()
    {
        // Pass already-computed 50/50 total (0-100) as both components so gate matches result screen.
        CertificateGenerator.Instance?.GenerateCertificate(_moduleId, _lastTotalScore, _lastTotalScore);
    }

    void OnRetry()
    {
        StartAssessment(_moduleId, _actionScore);
    }

    void OnGoHome()
    {
        HideAll();
        UINavigator.Instance?.ShowHome();
    }

    public void Show() { if (assessmentPanel != null) assessmentPanel.SetActive(true); }
    public void HideAll()
    {
        if (assessmentPanel != null) assessmentPanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }
}
