using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginScreenUI : MonoBehaviour
{
    [Header("References")]
    public LoginManager loginManager;
    public LanguageManager languageManager;

    [Header("Prefab UI Elements")]
    public Canvas mainCanvas;
    public GameObject loginPanel;
    public GameObject otpPanel;

    [Header("Login Panel Elements")]
    public Image background;
    public Image workerIcon;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public TMP_InputField userIdInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI loginButtonText;
    public TextMeshProUGUI forgotPasswordText;
    public TextMeshProUGUI taglineText;
    public TMP_Dropdown languageDropdown;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.05f, 0.10f, 0.18f);
    public Color cardColor = new Color(0.10f, 0.18f, 0.28f);
    public Color inputColor = new Color(0.12f, 0.20f, 0.32f);
    public Color cyanAccent = new Color(0f, 0.75f, 0.85f);
    public Color whiteText = Color.white;
    public Color lightBlueText = new Color(0.7f, 0.85f, 1f);
    public Color placeholderColor = new Color(0.5f, 0.6f, 0.7f);

    private string[] _langCodes = { "en", "hi", "mr", "ta", "te", "kn", "bn" };
    private string[] _langNames = { "English", "Hindi", "Marathi", "Tamil", "Telugu", "Kannada", "Bengali" };

    void Start()
    {
        if (mainCanvas == null) return;

        if (ThemeManager.Instance != null)
        {
            backgroundColor = ThemeManager.Instance.CurrentBackground;
            cardColor = ThemeManager.Instance.CurrentCard;
            inputColor = ThemeManager.Instance.CurrentInputBg;
            cyanAccent = ThemeManager.Instance.CurrentPrimary;
        }

        ApplyColors();
        SetupLanguageDropdown();
    }

    void ApplyColors()
    {
        if (background != null) background.color = backgroundColor;

        if (loginButton != null)
        {
            loginButton.GetComponent<Image>().color = cyanAccent;
        }

        if (titleText != null) titleText.color = whiteText;
        if (subtitleText != null) subtitleText.color = lightBlueText;
        if (forgotPasswordText != null) forgotPasswordText.color = cyanAccent;
        if (taglineText != null) taglineText.color = lightBlueText;

        if (userIdInput != null)
        {
            userIdInput.GetComponent<Image>().color = inputColor;
            if (userIdInput.textComponent != null) userIdInput.textComponent.color = whiteText;
            if (userIdInput.placeholder != null) userIdInput.placeholder.color = placeholderColor;
        }

        if (passwordInput != null)
        {
            passwordInput.GetComponent<Image>().color = inputColor;
            if (passwordInput.textComponent != null) passwordInput.textComponent.color = whiteText;
            if (passwordInput.placeholder != null) passwordInput.placeholder.color = placeholderColor;
        }
    }

    void SetupLanguageDropdown()
    {
        if (languageDropdown == null) return;

        languageDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>(_langNames);
        languageDropdown.AddOptions(options);

        string savedLang = PlayerPrefs.GetString("Language", "en");
        int idx = System.Array.IndexOf(_langCodes, savedLang);
        if (idx >= 0) languageDropdown.value = idx;

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    void OnLanguageChanged(int idx)
    {
        if (idx >= 0 && idx < _langCodes.Length)
        {
            string lang = _langCodes[idx];
            PlayerPrefs.SetString("Language", lang);
            LanguageManager.Instance?.SetLanguage(lang);
            UpdateUIText();
        }
    }

    void UpdateUIText()
    {
        if (titleText != null)
            titleText.text = "AR Safety Training";

        if (subtitleText != null)
            subtitleText.text = "Real Hazards. Real Decisions. Safer Tomorrow.";

        if (forgotPasswordText != null)
            forgotPasswordText.text = LanguageManager.Instance?.Get("forgot_password") ?? "Forgot Password?";

        if (loginButtonText != null)
            loginButtonText.text = LanguageManager.Instance?.Get("login") ?? "Login";

        if (taglineText != null)
            taglineText.text = "Safe Workers • Safe Industry • Stronger India";

        if (userIdInput != null && userIdInput.placeholder != null)
        {
            var ph = userIdInput.placeholder as TextMeshProUGUI;
            if (ph != null) ph.text = LanguageManager.Instance?.Get("employee_id") ?? "Employee ID / Phone";
        }

        if (passwordInput != null && passwordInput.placeholder != null)
        {
            var ph = passwordInput.placeholder as TextMeshProUGUI;
            if (ph != null) ph.text = LanguageManager.Instance?.Get("password_pin") ?? "Password / PIN";
        }
    }

    public void OnThemeChanged()
    {
        if (ThemeManager.Instance != null)
        {
            backgroundColor = ThemeManager.Instance.CurrentBackground;
            cardColor = ThemeManager.Instance.CurrentCard;
            inputColor = ThemeManager.Instance.CurrentInputBg;
            cyanAccent = ThemeManager.Instance.CurrentPrimary;
        }
        ApplyColors();
    }
}
