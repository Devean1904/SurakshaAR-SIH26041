using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorkerLanding : MonoBehaviour
{
    [Header("Header")]
    public TextMeshProUGUI welcomeText;
    public TextMeshProUGUI roleBadgeText;
    public TextMeshProUGUI siteText;

    [Header("Training Card")]
    public TextMeshProUGUI trainingTitle;
    public TextMeshProUGUI moduleNameText;
    public TextMeshProUGUI moduleProgressText;
    public Slider progressBar;
    public Button continueBtn;

    [Header("Stats")]
    public TextMeshProUGUI modulesCompletedText;
    public TextMeshProUGUI certificatesText;
    public TextMeshProUGUI progressPercentText;

    [Header("Bottom Navigation")]
    public Button homeNavBtn;
    public Button trainingNavBtn;
    public Button certificateNavBtn;
    public Button profileNavBtn;
    public Image homeNavIcon;
    public Image trainingNavIcon;
    public Image certificateNavIcon;
    public Image profileNavIcon;

    [Header("Navigation Colors")]
    public Color activeNavColor = new Color(0f, 0.75f, 0.85f);
    public Color inactiveNavColor = new Color(0.5f, 0.6f, 0.7f);

    private int _modulesCompleted = 0;
    private int _totalModules = 5;
    private int _certCount = 0;
    private string _currentLanguage = "en";

    void Start()
    {
        _currentLanguage = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string name = PlayerPrefs.GetString("UserName", "Worker");
        string role = PlayerPrefs.GetString("Role", "Worker");
        string site = PlayerPrefs.GetString("SiteName", "Mine Site A");

        if (welcomeText != null)
        {
            string welcome = LanguageManager.Instance?.Get("voice_welcome") ?? "Welcome";
            welcomeText.text = $"{welcome}, {name}";
        }

        if (roleBadgeText != null)
        {
            roleBadgeText.text = role;
        }

        if (siteText != null)
        {
            siteText.text = $"| {site}";
        }

        if (trainingTitle != null)
        {
            trainingTitle.text = LanguageManager.Instance?.Get("todays_training") ?? "Today's Training";
        }

        if (moduleNameText != null)
        {
            moduleNameText.text = LanguageManager.Instance?.Get("fire_explosion") ?? "Fire & Explosion";
        }

        if (continueBtn != null) continueBtn.onClick.AddListener(OnContinue);
        if (homeNavBtn != null) homeNavBtn.onClick.AddListener(OnHomeNav);
        if (trainingNavBtn != null) trainingNavBtn.onClick.AddListener(OnTrainingNav);
        if (certificateNavBtn != null) certificateNavBtn.onClick.AddListener(OnCertificateNav);
        if (profileNavBtn != null) profileNavBtn.onClick.AddListener(OnProfileNav);

        LoadProgress();
        UpdateNavHighlight("home");
    }

    void LoadProgress()
    {
        _modulesCompleted = PlayerPrefs.GetInt("ModulesCompleted", 0);
        _certCount = PlayerPrefs.GetInt("CertCount", 0);

        if (moduleProgressText != null)
        {
            string moduleLabel = LanguageManager.Instance?.Get("module") ?? "Module";
            moduleProgressText.text = $"{moduleLabel} {_modulesCompleted + 1} / {_totalModules}";
        }

        if (progressBar != null)
        {
            progressBar.value = (float)_modulesCompleted / _totalModules;
        }

        if (modulesCompletedText != null)
        {
            string label = LanguageManager.Instance?.Get("modules_completed") ?? "Modules Completed";
            modulesCompletedText.text = $"{label}: {_modulesCompleted}/{_totalModules}";
        }

        if (certificatesText != null)
        {
            string label = LanguageManager.Instance?.Get("certificates") ?? "Certificates";
            certificatesText.text = $"{label}: {_certCount}";
        }

        if (progressPercentText != null)
        {
            float percent = _totalModules > 0 ? (float)_modulesCompleted / _totalModules * 100f : 0f;
            progressPercentText.text = $"{percent:F0}%";
        }
    }

    void OnContinue()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("voice_training_start") ?? "Starting training", lang);
        UINavigator.Instance?.ShowScenario();
    }

    void OnHomeNav()
    {
        UpdateNavHighlight("home");
    }

    void OnTrainingNav()
    {
        UpdateNavHighlight("training");
        UINavigator.Instance?.ShowScenario();
    }

    void OnCertificateNav()
    {
        UpdateNavHighlight("certificate");
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("cert_view") ?? "View certificates", lang);
    }

    void OnProfileNav()
    {
        UpdateNavHighlight("profile");
        UINavigator.Instance?.ShowSettings();
    }

    void UpdateNavHighlight(string activeTab)
    {
        SetNavIconColor(homeNavIcon, activeTab == "home");
        SetNavIconColor(trainingNavIcon, activeTab == "training");
        SetNavIconColor(certificateNavIcon, activeTab == "certificate");
        SetNavIconColor(profileNavIcon, activeTab == "profile");
    }

    void SetNavIconColor(Image icon, bool active)
    {
        if (icon != null)
        {
            icon.color = active ? activeNavColor : inactiveNavColor;
        }
    }
}
