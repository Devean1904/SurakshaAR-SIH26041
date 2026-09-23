using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsScreen : MonoBehaviour
{
    [Header("Theme")]
    public Button themeToggleButton;
    public TextMeshProUGUI themeLabel;

    [Header("Language")]
    public TMP_Dropdown languageDropdown;
    public Button downloadVoiceButton;
    public TextMeshProUGUI voiceStatusText;

    [Header("Accessibility")]
    public Toggle talkBackToggle;
    public Toggle autoAnnounceToggle;

    [Header("Info")]
    public TextMeshProUGUI userIdText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI versionText;

    void Start()
    {
        string userId = PlayerPrefs.GetString("UserId", "N/A");
        string role = PlayerPrefs.GetString("Role", "N/A");

        if (userIdText != null) userIdText.text = $"ID: {userId}";
        if (roleText != null) roleText.text = $"Role: {role}";
        if (versionText != null) versionText.text = "SurakshaAR v1.0.0";

        if (themeToggleButton != null)
        {
            themeToggleButton.onClick.AddListener(OnThemeToggle);
            UpdateThemeLabel();
        }

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "English", "Hindi", "Santali"
            });
            string currentLang = PlayerPrefs.GetString("Language", "en");
            languageDropdown.value = currentLang switch
            {
                "hi" => 1,
                "sat" => 2,
                _ => 0
            };
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if (downloadVoiceButton != null)
            downloadVoiceButton.onClick.AddListener(OnDownloadVoice);
    }

    void OnThemeToggle()
    {
        ThemeManager.Instance.ToggleTheme();
        UpdateThemeLabel();
    }

    void UpdateThemeLabel()
    {
        if (themeLabel != null)
            themeLabel.text = $"Theme: {ThemeManager.Instance.CurrentTheme}";
    }

    void OnLanguageChanged(int index)
    {
        string code = index switch
        {
            1 => "hi",
            2 => "sat",
            _ => "en"
        };
        LanguageManager.Instance.SetLanguage(code);
        LocalBlockchain.Instance.AddBlock("language_changed", PlayerPrefs.GetString("UserId"), code);

        string label = LanguageManager.Instance.Get("settings");
        VoiceModule.Instance.AnnounceForLanguage(label);
    }

    void OnDownloadVoice()
    {
        string lang = LanguageManager.Instance.CurrentLanguage;
        VoiceModule.Instance.DownloadVoicePack(lang);
        if (voiceStatusText != null) voiceStatusText.text = $"Downloading: {lang}...";
    }
}
