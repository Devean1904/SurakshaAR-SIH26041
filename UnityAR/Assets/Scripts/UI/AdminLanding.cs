using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminLanding : MonoBehaviour
{
    [Header("Header")]
    public TextMeshProUGUI welcomeText;
    public TextMeshProUGUI roleBadgeText;
    public TextMeshProUGUI siteText;

    [Header("Dashboard Grid")]
    public Button manageTrainingBtn;
    public Button userManagementBtn;
    public Button blockchainKeysBtn;
    public Button systemAnalyticsBtn;

    [Header("Dashboard Card Labels")]
    public TextMeshProUGUI manageTrainingLabel;
    public TextMeshProUGUI userManagementLabel;
    public TextMeshProUGUI blockchainKeysLabel;
    public TextMeshProUGUI systemAnalyticsLabel;

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

    private string _currentLanguage = "en";

    void Start()
    {
        _currentLanguage = LanguageManager.Instance?.CurrentLanguage ?? "en";
        string name = PlayerPrefs.GetString("UserName", "Admin");
        string role = PlayerPrefs.GetString("Role", "Admin");
        string site = PlayerPrefs.GetString("SiteName", "System Admin");

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

        UpdateCardLabels();

        if (manageTrainingBtn != null) manageTrainingBtn.onClick.AddListener(OnManageTraining);
        if (userManagementBtn != null) userManagementBtn.onClick.AddListener(OnUserManagement);
        if (blockchainKeysBtn != null) blockchainKeysBtn.onClick.AddListener(OnBlockchainKeys);
        if (systemAnalyticsBtn != null) systemAnalyticsBtn.onClick.AddListener(OnSystemAnalytics);

        if (homeNavBtn != null) homeNavBtn.onClick.AddListener(OnHomeNav);
        if (trainingNavBtn != null) trainingNavBtn.onClick.AddListener(OnTrainingNav);
        if (certificateNavBtn != null) certificateNavBtn.onClick.AddListener(OnCertificateNav);
        if (profileNavBtn != null) profileNavBtn.onClick.AddListener(OnProfileNav);

        UpdateNavHighlight("home");
    }

    void UpdateCardLabels()
    {
        if (manageTrainingLabel != null)
            manageTrainingLabel.text = LanguageManager.Instance?.Get("manage_training_content") ?? "Manage Training Content";
        if (userManagementLabel != null)
            userManagementLabel.text = LanguageManager.Instance?.Get("user_management") ?? "User Management";
        if (blockchainKeysLabel != null)
            blockchainKeysLabel.text = LanguageManager.Instance?.Get("blockchain_keys") ?? "Blockchain Keys";
        if (systemAnalyticsLabel != null)
            systemAnalyticsLabel.text = LanguageManager.Instance?.Get("system_analytics") ?? "System Analytics";
    }

    void OnManageTraining()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("admin_module_config") ?? "Manage training content", lang);
    }

    void OnUserManagement()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("admin_user_mgmt") ?? "User management", lang);
    }

    void OnBlockchainKeys()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak("Blockchain keys management", lang);
    }

    void OnSystemAnalytics()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("admin_analytics") ?? "System analytics", lang);
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
