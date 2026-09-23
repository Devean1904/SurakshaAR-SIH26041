using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DashboardManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _dashboardPanel;
    [SerializeField] private Button _logoutButton;
    [SerializeField] private Button _startTrainingButton;
    [SerializeField] private Button _viewCertificateButton;
    [SerializeField] private Button _viewProgressButton;
    [SerializeField] private Button _openSupportButton;
    [SerializeField] private TextMeshProUGUI _welcomeText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Transform _domainListParent;
    [SerializeField] private GameObject _domainButtonPrefab;

    private string _currentUserId;
    private string _currentUserName;
    private string _currentRole;
    private string _currentLanguage = "en";

    private readonly List<string> _domains = new()
    {
        "Fire & Explosion",
        "Gas Leak & Confined Space",
        "Machinery (LOTO)",
        "Electrical Hazards",
        "Heights & Fall Protection"
    };

    void Start()
    {
        if (_logoutButton != null) _logoutButton.onClick.AddListener(OnLogout);
        if (_startTrainingButton != null) _startTrainingButton.onClick.AddListener(OnStartTraining);
        if (_viewCertificateButton != null) _viewCertificateButton.onClick.AddListener(OnViewCertificate);
        if (_viewProgressButton != null) _viewProgressButton.onClick.AddListener(OnViewProgress);
        if (_openSupportButton != null) _openSupportButton.onClick.AddListener(OnOpenSupport);

        if (LanguageManager.Instance != null)
        {
            _currentLanguage = LanguageManager.Instance.CurrentLanguage;
            LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }

        PopulateDomainList();
    }

    void OnDestroy()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged(string newLang)
    {
        _currentLanguage = newLang;
        RefreshDashboardTexts();
    }

    void PopulateDomainList()
    {
        if (_domainListParent == null || _domainButtonPrefab == null) return;

        foreach (Transform child in _domainListParent)
            Destroy(child.gameObject);

        foreach (string domain in _domains)
        {
            var go = Instantiate(_domainButtonPrefab, _domainListParent);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = GetDomainDisplayName(domain);

            var button = go.GetComponent<Button>();
            if (button != null)
            {
                string capturedDomain = domain;
                button.onClick.AddListener(() => OnDomainSelected(capturedDomain));
            }
        }
    }

    string GetDomainDisplayName(string domain)
    {
        if (LanguageManager.Instance == null) return domain;

        return domain switch
        {
            "Fire & Explosion" => LanguageManager.Instance.Get("scenario_fire_title"),
            "Gas Leak & Confined Space" => LanguageManager.Instance.Get("scenario_gas_title"),
            "Machinery (LOTO)" => LanguageManager.Instance.Get("scenario_machinery_title"),
            "Electrical Hazards" => LanguageManager.Instance.Get("scenario_electrical_title"),
            "Heights & Fall Protection" => LanguageManager.Instance.Get("scenario_heights_title"),
            _ => domain
        };
    }

    void RefreshDashboardTexts()
    {
        if (_welcomeText != null && !string.IsNullOrEmpty(_currentUserName))
        {
            string greeting = _currentRole == "worker"
                ? LanguageManager.Instance?.Get("ui_dashboard_greeting_worker") ?? "Welcome, {0}"
                : LanguageManager.Instance?.Get("ui_dashboard_greeting_manager") ?? "Manager Dashboard - {0}";
            _welcomeText.text = string.Format(greeting, _currentUserName);
        }
    }

    public void ShowDashboard(string userId, string userName, string role)
    {
        _currentUserId = userId;
        _currentUserName = userName;
        _currentRole = role;

        if (_dashboardPanel != null) _dashboardPanel.SetActive(true);

        RefreshDashboardTexts();

        if (_startTrainingButton != null)
            _startTrainingButton.interactable = (role == "worker");
        if (_viewCertificateButton != null)
            _viewCertificateButton.interactable = (role == "worker");

        Debug.Log($"[Dashboard] Shown for {userName} ({role})");
    }

    public void HideDashboard()
    {
        if (_dashboardPanel != null) _dashboardPanel.SetActive(false);
    }

    void OnDomainSelected(string domain)
    {
        Debug.Log($"[Dashboard] Domain selected: {domain}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartTraining(domain);
        }
        else
        {
            SceneLoader.Instance?.LoadARScene();
        }
    }

    void OnStartTraining()
    {
        if (_domains.Count > 0)
            OnDomainSelected(_domains[0]);
    }

    void OnViewCertificate()
    {
        Debug.Log("[Dashboard] Viewing certificates");
        string msg = LanguageManager.Instance?.Get("cert_no_certs") ?? "No certificates yet.";
        ShowStatus(msg);
    }

    void OnViewProgress()
    {
        Debug.Log("[Dashboard] Viewing progress");
        int completed = PlayerPrefs.GetInt("ModulesCompleted", 0);
        int certs = PlayerPrefs.GetInt("CertCount", 0);
        ShowStatus($"Modules completed: {completed}, Certificates: {certs}");
    }

    void OnOpenSupport()
    {
        if (SupportController.Instance != null)
        {
            SupportController.Instance.OpenSupport();
        }
    }

    void OnLogout()
    {
        Debug.Log("[Dashboard] Logging out");

        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.DeleteKey("UserRole");
        PlayerPrefs.DeleteKey("Token");
        PlayerPrefs.Save();

        _currentUserId = "";
        _currentUserName = "";
        _currentRole = "";

        HideDashboard();
        SceneLoader.Instance?.LoadLoginScene();
    }

    void ShowStatus(string message)
    {
        if (_statusText != null) _statusText.text = message;
        Debug.Log($"[Dashboard] {message}");
    }

    public string GetCurrentUserId() => _currentUserId;
    public string GetCurrentUserName() => _currentUserName;
    public string GetCurrentRole() => _currentRole;
}