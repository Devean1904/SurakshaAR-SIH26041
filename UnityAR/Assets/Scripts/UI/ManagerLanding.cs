using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagerLanding : MonoBehaviour
{
    [Header("Header")]
    public TextMeshProUGUI welcomeText;
    public TextMeshProUGUI roleBadgeText;
    public TextMeshProUGUI siteText;

    [Header("Dashboard Grid")]
    public Button workerRosterBtn;
    public Button siteComplianceBtn;
    public Button certificatesBtn;
    public Button analyticsBtn;

    [Header("Dashboard Card Labels")]
    public TextMeshProUGUI workerRosterLabel;
    public TextMeshProUGUI siteComplianceLabel;
    public TextMeshProUGUI certificatesLabel;
    public TextMeshProUGUI analyticsLabel;

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
        string name = PlayerPrefs.GetString("UserName", "Manager");
        string role = PlayerPrefs.GetString("Role", "Manager");
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

        UpdateCardLabels();

        if (workerRosterBtn != null) workerRosterBtn.onClick.AddListener(OnWorkerRoster);
        if (siteComplianceBtn != null) siteComplianceBtn.onClick.AddListener(OnSiteCompliance);
        if (certificatesBtn != null) certificatesBtn.onClick.AddListener(OnCertificates);
        if (analyticsBtn != null) analyticsBtn.onClick.AddListener(OnAnalytics);

        if (homeNavBtn != null) homeNavBtn.onClick.AddListener(OnHomeNav);
        if (trainingNavBtn != null) trainingNavBtn.onClick.AddListener(OnTrainingNav);
        if (certificateNavBtn != null) certificateNavBtn.onClick.AddListener(OnCertificateNav);
        if (profileNavBtn != null) profileNavBtn.onClick.AddListener(OnProfileNav);

        UpdateNavHighlight("home");
    }

    void UpdateCardLabels()
    {
        if (workerRosterLabel != null)
            workerRosterLabel.text = LanguageManager.Instance?.Get("view_worker_roster") ?? "View Worker Roster";
        if (siteComplianceLabel != null)
            siteComplianceLabel.text = LanguageManager.Instance?.Get("site_compliance") ?? "Site Compliance";
        if (certificatesLabel != null)
            certificatesLabel.text = LanguageManager.Instance?.Get("certificates_verification") ?? "Certificates Verification";
        if (analyticsLabel != null)
            analyticsLabel.text = LanguageManager.Instance?.Get("analytics") ?? "Analytics";
    }

    void OnWorkerRoster()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("manager_worker_roster") ?? "Worker roster", lang);
    }

    void OnSiteCompliance()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("manager_compliance") ?? "Site compliance", lang);
    }

    void OnCertificates()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("manager_verify_cert") ?? "Verify certificates", lang);
        QRCheckpointScanner.Instance?.StartScanning();
    }

    void OnAnalytics()
    {
        string lang = _currentLanguage;
        VoiceModule.Instance?.Speak(LanguageManager.Instance?.Get("admin_analytics") ?? "Analytics", lang);
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
        QRCheckpointScanner.Instance?.StartScanning();
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
