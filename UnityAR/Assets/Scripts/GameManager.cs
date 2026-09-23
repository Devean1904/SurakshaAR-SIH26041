using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Training Configuration")]
    private string _selectedDomain = "Fire & Explosion";
    private string _selectedScenarioId;
    private string _selectedModuleId;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartTraining(string domain)
    {
        _selectedDomain = domain;
        Debug.Log($"[GameManager] Starting training for domain: {domain}");

        if (domain.Contains("Fire") || domain.Contains("Explosion"))
        {
            _selectedModuleId = "fire-safety-101";
            _selectedScenarioId = "fire-1";
        }
        else if (domain.Contains("Gas") || domain.Contains("Confined"))
        {
            _selectedModuleId = "gas-leak-101";
            _selectedScenarioId = "gas-1";
        }
        else if (domain.Contains("Machinery") || domain.Contains("LOTO"))
        {
            _selectedModuleId = "fire-safety-101";
            _selectedScenarioId = "fire-1";
        }
        else if (domain.Contains("Electrical"))
        {
            _selectedModuleId = "fire-safety-101";
            _selectedScenarioId = "fire-1";
        }
        else if (domain.Contains("Heights"))
        {
            _selectedModuleId = "fire-safety-101";
            _selectedScenarioId = "fire-1";
        }
        else
        {
            _selectedModuleId = "fire-safety-101";
            _selectedScenarioId = "fire-1";
        }

        SceneManager.LoadScene("ARScene");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "ARScene") return;

        Debug.Log("[GameManager] ARScene loaded, starting scenario...");

        if (ScenarioManager.Instance != null)
        {
            ScenarioManager.Instance.StartScenario(_selectedModuleId, _selectedScenarioId);
        }
    }

    public void CompleteTraining(int score, bool passed)
    {
        string lang = LanguageManager.Instance?.CurrentLanguage ?? "en";

        if (passed)
        {
            if (OfflineManager.Instance != null)
                OfflineManager.Instance.SaveTrainingResult(_selectedModuleId, score, passed);

            Debug.Log($"[GameManager] Training PASSED. Score: {score}");

            if (CertificateGenerator.Instance != null)
            {
                // Pass score as both components so gate treats it as the 0-100 total.
                CertificateGenerator.Instance.GenerateCertificate(_selectedModuleId, score, score);
            }
        }
        else
        {
            Debug.Log($"[GameManager] Training FAILED. Score: {score}");
            string failMsg = LanguageManager.Instance?.Get("training_failed") ?? "Training failed. Try again.";
            VoiceModule.Instance?.Speak(failMsg, lang);
        }
    }

    public string GetSelectedDomain() => _selectedDomain;
    public string GetSelectedModuleId() => _selectedModuleId;
    public string GetSelectedScenarioId() => _selectedScenarioId;
}