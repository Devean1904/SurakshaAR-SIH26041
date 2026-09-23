using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages the Login Screen — ID+PIN login, OTP login, language selection,
/// offline fallback, and role-based redirect after successful authentication.
/// </summary>
public class LoginManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Panels
    // ─────────────────────────────────────────────
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject otpPanel;
    public GameObject loadingOverlay;

    // ─────────────────────────────────────────────
    //  ID / PIN Login Fields
    // ─────────────────────────────────────────────
    [Header("ID Login")]
    public TMP_InputField userIdInput;
    public TMP_InputField passwordInput;
    public Button         loginButton;
    public TextMeshProUGUI loginButtonLabel;
    public Toggle         rememberToggle;

    // ─────────────────────────────────────────────
    //  Language Selector
    // ─────────────────────────────────────────────
    [Header("Language")]
    public TMP_Dropdown languageDropdown;

    // ─────────────────────────────────────────────
    //  OTP Login Fields
    // ─────────────────────────────────────────────
    [Header("OTP Login")]
    public TMP_InputField phoneInput;
    public TMP_InputField otpInput;
    public Button         sendOtpButton;
    public Button         verifyOtpButton;
    public Button         backToLoginButton;

    // ─────────────────────────────────────────────
    //  Action Buttons
    // ─────────────────────────────────────────────
    [Header("Action Buttons")]
    public Button forgotPasswordButton;
    public Button otpLoginTabButton;
    public Button idLoginTabButton;

    // ─────────────────────────────────────────────
    //  UI Feedback
    // ─────────────────────────────────────────────
    [Header("UI Feedback")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI hintText;

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────
    private string BASE_URL => ServerConfig.Instance.ApiBase;

    // ISO codes matching the dropdown order
    private readonly string[] _langCodes =
        { "en", "hi", "mr", "ta", "te", "kn", "bn" };

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────
    void Start()
    {
        // Show/hide panels
        SafeSetActive(loginPanel,     true);
        SafeSetActive(otpPanel,       false);
        SafeSetActive(loadingOverlay, false);

        // Wire listeners
        loginButton        ?.onClick.AddListener(OnIdLogin);
        forgotPasswordButton?.onClick.AddListener(OnForgotPassword);
        sendOtpButton      ?.onClick.AddListener(OnSendOtp);
        verifyOtpButton    ?.onClick.AddListener(OnVerifyOtp);
        backToLoginButton  ?.onClick.AddListener(OnBackToLogin);
        otpLoginTabButton  ?.onClick.AddListener(() => SwitchMode(otp: true));
        idLoginTabButton   ?.onClick.AddListener(() => SwitchMode(otp: false));
        languageDropdown   ?.onValueChanged.AddListener(OnLanguageChanged);

        // Restore remembered credentials
        string savedId = PlayerPrefs.GetString("UserId", "");
        if (!string.IsNullOrEmpty(savedId) && userIdInput != null)
        {
            userIdInput.text = savedId;
            if (rememberToggle != null) rememberToggle.isOn = true;
        }

        // Restore saved language
        string savedLang = PlayerPrefs.GetString("Language", "en");
        if (languageDropdown != null)
        {
            int idx = System.Array.IndexOf(_langCodes, savedLang);
            if (idx >= 0) languageDropdown.value = idx;
        }

        ClearStatus();
        ApplyThemeColors();
    }

    void ApplyThemeColors()
    {
        if (ThemeManager.Instance == null) return;

        var tm = ThemeManager.Instance;

        if (loginPanel != null)
        {
            var bg = loginPanel.GetComponent<Image>();
            if (bg != null) bg.color = tm.CurrentBackground;
        }

        if (userIdInput != null)
        {
            var img = userIdInput.GetComponent<Image>();
            if (img != null) img.color = tm.CurrentInputBg;
            if (userIdInput.textComponent != null) userIdInput.textComponent.color = tm.CurrentText;
            if (userIdInput.placeholder != null) userIdInput.placeholder.color = tm.CurrentTextSecondary;
        }

        if (passwordInput != null)
        {
            var img = passwordInput.GetComponent<Image>();
            if (img != null) img.color = tm.CurrentInputBg;
            if (passwordInput.textComponent != null) passwordInput.textComponent.color = tm.CurrentText;
            if (passwordInput.placeholder != null) passwordInput.placeholder.color = tm.CurrentTextSecondary;
        }

        if (loginButton != null)
        {
            var img = loginButton.GetComponent<Image>();
            if (img != null) img.color = tm.CurrentPrimary;
        }

        if (loginButtonLabel != null) loginButtonLabel.color = tm.CurrentText;

        foreach (var tmp in loginPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp.gameObject.CompareTag("ThemeText"))
                tmp.color = tm.CurrentText;
            else if (tmp.gameObject.CompareTag("ThemeTextSecondary"))
                tmp.color = tm.CurrentTextSecondary;
            else if (tmp.gameObject.CompareTag("ThemePrimary"))
                tmp.color = tm.CurrentPrimary;
        }
    }

    // ─────────────────────────────────────────────
    //  ID + PIN Login
    // ─────────────────────────────────────────────
    void OnIdLogin()
    {
        string uid = userIdInput?.text?.Trim() ?? "";
        string pwd = passwordInput?.text?.Trim() ?? "";

        if (string.IsNullOrEmpty(uid))
        {
            ShowStatus("Please enter your Employee ID or Phone.", error: true);
            return;
        }
        if (string.IsNullOrEmpty(pwd))
        {
            ShowStatus("Please enter your Password / PIN.", error: true);
            return;
        }

        StartCoroutine(LoginWithId(uid, pwd));
    }

    IEnumerator LoginWithId(string userId, string password)
    {
        ShowStatus("Verifying credentials…", error: false);
        SetLoading(true);

        string body = JsonUtility.ToJson(new LoginPayload { UserId = userId, Password = password });

        using var req = new UnityWebRequest($"{BASE_URL}/auth/login", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 10;

        yield return req.SendWebRequest();

        SetLoading(false);

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            if (res.Success)
            {
                // Persist session
                PlayerPrefs.SetString("Token",    res.Token    ?? "");
                PlayerPrefs.SetString("UserId",   res.UserId   ?? userId);
                PlayerPrefs.SetString("Role",     res.Role     ?? "worker");
                PlayerPrefs.SetString("UserName", res.Name     ?? userId);
                PlayerPrefs.SetString("Theme",    res.Theme    ?? "dark");
                PlayerPrefs.SetString("Language", res.Language ?? "en");
                if (rememberToggle != null && rememberToggle.isOn) PlayerPrefs.Save();

                ThemeManager.Instance?.ApplyTheme(res.Theme ?? "dark");
                LanguageManager.Instance?.SetLanguage(res.Language ?? "en");

                ShowStatus("Login successful! Loading…", error: false);
                SceneManager.LoadScene("MainScene");
            }
            else
            {
                ShowStatus(res.Message ?? "Invalid ID or Password. Please try again.", error: true);
            }
        }
        else
        {
            ShowStatus("Cannot reach server — switching to Offline Mode.", error: true);
            OfflineManager.Instance?.LoadOfflineSession();
        }
    }

    // ─────────────────────────────────────────────
    //  OTP Login
    // ─────────────────────────────────────────────
    void OnSendOtp()
    {
        string phone = phoneInput?.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(phone))
        {
            ShowStatus("Please enter your phone number.", error: true);
            return;
        }
        StartCoroutine(SendOtp(phone));
    }

    IEnumerator SendOtp(string phone)
    {
        ShowStatus($"Sending OTP to {phone}…", error: false);
        SetLoading(true);

        string body = JsonUtility.ToJson(new OtpPayload { PhoneNumber = phone });
        using var req = new UnityWebRequest($"{BASE_URL}/auth/otp/send", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        SetLoading(false);

        if (req.result == UnityWebRequest.Result.Success)
        {
            SafeSetActive(otpInput?.gameObject,     true);
            SafeSetActive(verifyOtpButton?.gameObject, true);
            ShowStatus("OTP sent! Enter the code below.", error: false);
        }
        else
        {
            ShowStatus("Failed to send OTP. Please check your number.", error: true);
        }
    }

    void OnVerifyOtp()
    {
        string phone = phoneInput?.text?.Trim() ?? "";
        string otp   = otpInput?.text?.Trim()   ?? "";
        if (string.IsNullOrEmpty(otp))
        {
            ShowStatus("Please enter the OTP.", error: true);
            return;
        }
        StartCoroutine(VerifyOtp(phone, otp));
    }

    IEnumerator VerifyOtp(string phone, string otp)
    {
        ShowStatus("Verifying OTP…", error: false);
        SetLoading(true);

        string body = JsonUtility.ToJson(new OtpVerifyPayload { PhoneNumber = phone, Otp = otp });
        using var req = new UnityWebRequest($"{BASE_URL}/auth/otp/verify", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        SetLoading(false);

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            if (res.Success)
            {
                PlayerPrefs.SetString("Token",  res.Token  ?? "");
                PlayerPrefs.SetString("UserId", res.UserId ?? "");
                PlayerPrefs.SetString("Role",   res.Role   ?? "worker");
                PlayerPrefs.Save();
                SceneManager.LoadScene("MainScene");
            }
            else
            {
                ShowStatus(res.Message ?? "OTP verification failed.", error: true);
            }
        }
        else
        {
            ShowStatus("Verification error. Please try again.", error: true);
        }
    }

    // ─────────────────────────────────────────────
    //  Other Actions
    // ─────────────────────────────────────────────
    public void OnBackToLogin()
    {
        SwitchMode(otp: false);
        ClearStatus();
    }

    public void OnForgotPassword()
    {
        ShowStatus("Please contact your Supervisor or HR to reset your password.", error: false);
    }

    public void OnToggleLanguage()
    {
        var lp = GameObject.Find("LanguagePanel");
        if (lp != null) lp.SetActive(!lp.activeSelf);
    }

    void SwitchMode(bool otp)
    {
        SafeSetActive(loginPanel, !otp);
        SafeSetActive(otpPanel,    otp);
        ClearStatus();
    }

    void OnLanguageChanged(int idx)
    {
        if (idx >= 0 && idx < _langCodes.Length)
        {
            string lang = _langCodes[idx];
            PlayerPrefs.SetString("Language", lang);
            LanguageManager.Instance?.SetLanguage(lang);
        }
    }

    // ─────────────────────────────────────────────
    //  UI Helpers
    // ─────────────────────────────────────────────
    static readonly Color ColError   = new Color(0.90f, 0.20f, 0.20f, 1f); // Red danger
    static readonly Color ColSuccess = new Color(0.20f, 0.80f, 0.40f, 1f); // Green success
    static readonly Color ColInfo    = new Color(0.70f, 0.85f, 1.00f, 1f); // Light blue

    void ShowStatus(string msg, bool error)
    {
        if (statusText == null) return;
        statusText.text  = msg;
        statusText.color = error ? ColError : ColInfo;
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
    }

    void ClearStatus()
    {
        if (statusText != null)
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
    }

    void SetLoading(bool loading)
    {
        if (loginButton != null)       loginButton.interactable = !loading;
        if (loadingOverlay != null)    loadingOverlay.SetActive(loading);
        if (loginButtonLabel != null)  loginButtonLabel.text = loading ? "Verifying…" : "LOGIN";
    }

    static void SafeSetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    // ─────────────────────────────────────────────
    //  Serializable Data Models
    // ─────────────────────────────────────────────
    [System.Serializable]
    class LoginPayload    { public string UserId;      public string Password; }

    [System.Serializable]
    class OtpPayload      { public string PhoneNumber; }

    [System.Serializable]
    class OtpVerifyPayload { public string PhoneNumber; public string Otp; }

    [System.Serializable]
    class AuthResponse
    {
        public bool   Success;
        public string Token;
        public string UserId;
        public string Role;
        public string Name;
        public string Theme;
        public string Language;
        public string Message;
    }
}
